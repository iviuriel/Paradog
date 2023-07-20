using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : NetworkBehaviour
{
  [SerializeField] private Button rollBtn;
  [SerializeField] private Color circleColor;
  [SerializeField] private Color remnantColor;
  [SerializeField] private SpriteRenderer spriteRend;
  [SerializeField] private SpriteRenderer remnantSpriteRend;

  private NetworkVariable<Vector3> networkColor = new NetworkVariable<Vector3>
    (Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
  private NetworkVariable<Vector3> remnantNetworkColor = new NetworkVariable<Vector3>
    (Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


  void Start()
  {
    UpdateColor(networkColor.Value);
    UpdateRemnantColor(remnantNetworkColor.Value);
    networkColor.OnValueChanged += OnColorChanged;
    remnantNetworkColor.OnValueChanged += OnRemnantColorChanged;

    if (IsOwner)
    {
      UIManager.Instance.SetRollBtnListener(RequestRollServerRpc);
      RegisterPlayerServerRpc();
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

  public void OnRemnantColorChanged(Vector3 previous, Vector3 current)
  {
    UpdateRemnantColor(current);
    Logger.Log($"[Client] Player with id {OwnerClientId} updated remnant color to {remnantColor}");
  }

  public void UpdateColor(Vector3 color)
  {
    circleColor = new Color(color.x, color.y, color.z, 1);
    spriteRend.color = circleColor;
  }

  public void UpdateRemnantColor(Vector3 color)
  {
    remnantColor = new Color(color.x, color.y, color.z, 1);
    remnantSpriteRend.color = remnantColor;
  }



  #region Server Functions

  [ServerRpc]
  public void RegisterPlayerServerRpc()
  {
    ParadogManager.Instance.RegisterPlayer(OwnerClientId);
  }

  [ServerRpc]
  public void SetPlayerColorServerRpc(Vector3 _color)
  {
    networkColor.Value = _color;
  }

  [ServerRpc]
  public void SetRemnantColorServerRpc(Vector3 _color)
  {
    remnantNetworkColor.Value = _color;
  }

  #endregion

  #region Client Functions

  [ClientRpc]
  public void SetPlayerPositionClientRpc(Vector3 pos)
  {
    /// MODIFICAR PORQUE YA NO VAS A MOVER EL PLAYER EN SI SI NO A SUS HIJOS
    transform.position = pos;
    Logger.Log($"[Client] Updated position of client {OwnerClientId} to {pos}");

  }

  [ClientRpc]
  public void SetRemnantPositionClientRpc(Vector3 pos)
  {
    transform.position = pos;
    Logger.Log($"[Client] Updated position of client {OwnerClientId} to {pos}");

  }
  #endregion
}
