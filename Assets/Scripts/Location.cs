using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocationType
{
  Draw1,
  Draw2,
  Empty,
  Event,
  StartPlayer,
  StartRemnant,
  Impulse,
  Freeze,
  TimeTravel
}
public class Location : MonoBehaviour
{
  public LocationType locationType;

  [ShowIf("locationType", LocationType.Impulse)]
  public int locationsToMoveForward = 3;

  [ShowIf("locationType", LocationType.Freeze)]
  public int locationsToMoveBack = 3;

  [ShowIf("locationType", LocationType.TimeTravel)]
  public Location locationToTravel;

  #region Odin Inspector

  public void UpdateColor(Color color)
  {
    GetComponent<SpriteRenderer>().color = color;
  }

  #endregion

  public void Init()
  {

  }

  public void Activate(PlayerManager _player)
  {
    switch (locationType)
    {
      case LocationType.Draw1: DrawCard(_player, 1); break;
      case LocationType.Draw2: DrawCard(_player, 2); break;
      case LocationType.Impulse: Impulse(_player); break;
      case LocationType.Freeze: Freeze(_player); break;
      case LocationType.Event: PlayEvent(_player); break;
      case LocationType.TimeTravel: TimeTravel(_player); break;
      case LocationType.Empty:
      case LocationType.StartPlayer:
      case LocationType.StartRemnant:
      default:
        break;
    }
  }

  #region Effects

  public void DrawCard(PlayerManager _player, int _amount)
  {

  }

  public void PlayEvent(PlayerManager _player)
  {

  }

  public void Impulse(PlayerManager _player)
  {
    ParadogManager.Instance.SendMessageClientRpc($"Player activated an impulse location");
    ParadogManager.Instance.MovePlayer(_player, locationsToMoveForward, false);
  }

  public void Freeze(PlayerManager _player)
  {
    ParadogManager.Instance.SendMessageClientRpc($"Player activated an impulse location");
    ParadogManager.Instance.MovePlayer(_player, locationsToMoveBack * -1, false);
  }

  public void TimeTravel(PlayerManager _player)
  {

  }

  #endregion
}
