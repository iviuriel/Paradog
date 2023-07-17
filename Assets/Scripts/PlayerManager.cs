using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
  [SerializeField] private Button rollBtn;
  [SerializeField] private Color circleColor;
  [SerializeField] private SpriteRenderer spriteRend;

  private NetworkVariable<Vector3> networkColor = new NetworkVariable<Vector3>
    (Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


  void Start()
  {
    UpdateColor(networkColor.Value);
    networkColor.OnValueChanged += OnColorChanged;

    if (IsOwner)
    {
      UIManager.Instance.SetRollBtnListener(RequestRollServerRpc);
      RegisterPlayerServerRpc();
      SetPlayerColorServerRpc();
    }
  }

  // Update is called once per frame
  void Update()
  {
    /*if (!IsOwner) return;

    if (Input.GetKeyDown(KeyCode.R))
    {
      RollServerRpc();
    }*/
  }

  public void OnRollButtonClicked()
  {
    RequestRollServerRpc();
  }

  [ServerRpc]
  private void RequestRollServerRpc()
  {
    ParadogManager.Instance.PlayerRequestRoll(OwnerClientId);
  }

  public void OnColorChanged(Vector3 previous, Vector3 current)
  {
    UpdateColor(current);
    Logger.Log($"[Client] Player with id {OwnerClientId} updated color to {circleColor}");
  }

  public void UpdateColor(Vector3 color)
  {
    circleColor = new Color(color.x, color.y, color.z, 1);
    spriteRend.color = circleColor;
  }



  #region Server Functions

  [ServerRpc]
  public void RegisterPlayerServerRpc()
  {
    ParadogManager.Instance.RegisterPlayer(OwnerClientId);
  }

  [ServerRpc]
  public void SetPlayerColorServerRpc()
  {
    float r = Random.value;
    float g = Random.value;
    float b = Random.value;

    networkColor.Value = new Vector3(r, g, b);
  }

  #endregion

  #region Client Functions

  [ClientRpc]
  public void SetPositionClientRpc(Vector3 pos)
  {
    transform.position = pos;
    Logger.Log($"[Client] Updated position of client {OwnerClientId} to {pos}");

  }
  #endregion
}
