using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;

public struct PlayerData
{
  public ulong playerId;
  public int playerLocationIdx;
  public int remnantLocationIdx;


  public void SetPlayerLocation(int _loc)
  {
    playerLocationIdx = _loc;
  }

  public void SetRemnantLocation(int _loc)
  {
    remnantLocationIdx = _loc;
  }
}

[Serializable]
public class CoresPerPlayer
{
  public int players;
  public int cores;
}

public class ParadogManager : NetworkBehaviour
{
  public static ParadogManager Instance;

  [Title("Game Rules")]
  [OnValueChanged("MaxPlayersChanged"), MinValue(2)]
  public int maxPlayers = 2;

  [TableList]
  public List<CoresPerPlayer> coresDictionary = new List<CoresPerPlayer>();


  [Title("Colors")]
  [ListDrawerSettings(ShowIndexLabels = true, ShowItemCount = false)]
  public List<Color> playerColors;

  [ListDrawerSettings(ShowIndexLabels = true, ShowItemCount = false)]
  public List<Color> remnantColors;


  [Title("Game Generated")]
  [SerializeField, ReadOnly]
  private NetworkVariable<int> turnId = new NetworkVariable<int>
    (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

  private List<PlayerManager> players = new List<PlayerManager>();
  private List<PlayerData> playersData = new List<PlayerData>();

  private int coresToWinGame = 0;

  #region Odin Inspector

  void MaxPlayersChanged()
  {
    if (maxPlayers < 2) return;

    if (playerColors.Count < maxPlayers)
    {
      int iterations = maxPlayers - playerColors.Count;

      for (int i = 0; i < iterations; i++)
      {
        playerColors.Add(Color.white);
        remnantColors.Add(Color.white);
      }
    }
    else if (playerColors.Count > maxPlayers)
    {
      int iterations = playerColors.Count - maxPlayers;

      for (int i = 0; i < iterations; i++)
      {
        playerColors.RemoveAt(playerColors.Count - 1);
        remnantColors.RemoveAt(remnantColors.Count - 1);

      }
    }
  }

  #endregion

  #region Init
  // Start is called before the first frame update
  void Start()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }

    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    NetworkManager.Singleton.OnServerStarted += OnServerStarted;

    turnId.OnValueChanged += OnTurnChanged;
  }

  #endregion

  #region Connection Events

  private void OnClientConnected(ulong id)
  {

    Logger.Log($"Connected client with id {id}");
  }

  private void OnClientDisconnected(ulong id)
  {
    if (IsServer)
    {
      PlayerManager p = FindPlayerById(id);
      if (p != null)
      {
        players.Remove(p);
        UpdatePlayersPositions();
      }
    }
    Logger.Log($"Diconnected client with id {id}");
  }

  private void OnServerStarted()
  {
    Logger.Log($"Server connected");
  }

  #endregion

  #region Game Events

  public void StartGame()
  {
    ShowRollBtnClientRpc(new ClientRpcParams
    {
      Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { players[turnId.Value].OwnerClientId } }
    }); ;
    SendMessageClientRpc($"Turn of player with id {players[turnId.Value].OwnerClientId}");
  }

  public void OnTurnChanged(int _previous, int _current)
  {
    if (!IsServer) return;

    HideRollBtnClientRpc(new ClientRpcParams
    {
      Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { players[_previous].OwnerClientId } }
    });

    ShowRollBtnClientRpc(new ClientRpcParams
    {
      Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { players[_current].OwnerClientId } }
    });
  }

  public void OnTurnEnd(ulong _playerId)
  {

  }

  public void PlayerRequestRoll(ulong _playerId)
  {
    int dice = UnityEngine.Random.Range(1, 7);

    PlayerRollClientRpc(_playerId, dice);

    PlayerManager player = FindPlayerById(_playerId);
    int index = players.IndexOf(player);

    if (index != turnId.Value)
    {
      SendMessageClientRpc($"Not your turn");
    }

    MovePlayer(player, dice);

    if (turnId.Value + 1 >= players.Count)
    {
      turnId.Value = 0;
    }
    else
    {
      turnId.Value = turnId.Value + 1;
    }
  }

  public void MovePlayer(PlayerManager _player, int _locations, bool _activateNewLocation = true)
  {
    int index = players.IndexOf(_player);

    PlayerData newData = playersData[index];

    int newLocPos = newData.playerLocationIdx + _locations;
    /// MODIFICAR ESTO PORQUE YA NO SE "SUMA" SINO QUE SE SETEA LA POS
    

    if (newLocPos >= BoardManager.Instance.locations.Count)
    {
      newLocPos -= BoardManager.Instance.locations.Count;
    }

    newData.SetPlayerLocation(newLocPos);

    Location loc = BoardManager.Instance.locations[newData.playerLocationIdx];
    _player.SetPlayerPositionClientRpc(loc.transform.position + Vector3.back);
    playersData[index] = newData;

    if (_activateNewLocation)
    {
      BoardManager.Instance.locations[newData.playerLocationIdx].Activate(_player);
    }
  }


  public void PlayerPlayedCard()
  {

  }

  public void PlayerPlayedEvent()
  {

  }

  public void PlayerDrawCard()
  {

  }



  #endregion

  #region Functions

  public void RegisterPlayer(ulong _playerId)
  {
    Logger.Log($"[Server] Registered player with id {_playerId}");
    NetworkObject playerGo = NetworkManager.Singleton.ConnectedClients[_playerId].PlayerObject;
    PlayerManager player = playerGo.GetComponent<PlayerManager>();
    players.Add(player);
    int index = players.IndexOf(player);

    /// Set player color
    Color pColor = playerColors[index];
    Vector3 pColorVector = new Vector3(pColor.r, pColor.g, pColor.b);
    player.SetPlayerColorServerRpc(pColorVector);

    /// Set player color
    Color rColor = remnantColors[index];
    Vector3 rColorVector = new Vector3(rColor.r, rColor.g, rColor.b);
    player.SetRemnantColorServerRpc(rColorVector);

    PlayerData data = new PlayerData()
    {
      playerId = _playerId,
      playerLocationIdx = 0
    };

    playersData.Add(data);

    UpdatePlayersPositions();

    if (players.Count < 2)
    {
      SendMessageClientRpc($"[System] Waiting for players");
    }
    else
    {
      SendMessageClientRpc($"[System] Starting the game");
      StartGame();
    }

  }

  public void UpdatePlayersPositions()
  {
    for (int i = 0; i < players.Count; i++)
    {
      PlayerData data = playersData[i];
      Vector3 pos = BoardManager.Instance.locations[data.playerLocationIdx].transform.position;
      players[i].SetPlayerPositionClientRpc(pos + Vector3.back);
    }
  }

  public PlayerManager FindPlayerById(ulong _id)
  {
    return players.Find(x => x.OwnerClientId == _id);
  }



  #endregion

  #region Server Functions

  #endregion

  #region Client Functions

  [ClientRpc]
  public void SendMessageClientRpc(string _msg)
  {
    Logger.Log(_msg);
  }

  [ClientRpc]
  public void PlayerRollClientRpc(ulong _playerId, int dice)
  {
    Logger.Log($"[Client] Player {_playerId} roll a {dice}");
  }

  [ClientRpc]
  public void ShowRollBtnClientRpc(ClientRpcParams clientParams)
  {
    UIManager.Instance.ShowRollBtn();
  }

  [ClientRpc]
  public void HideRollBtnClientRpc(ClientRpcParams clientParams)
  {
    UIManager.Instance.HideRollBtn();
  }

  #endregion
}
