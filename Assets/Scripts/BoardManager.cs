using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
  public static BoardManager Instance;

  public List<Location> locations = new List<Location>();

  [ListDrawerSettings(ListElementLabelName = "@(LocationType)$property.Index")]
  public List<Color> locationColors;

  [ReadOnly] public Location startPlayerLocation;
  [ReadOnly] public Location startRemnantLocation;

  [Button]
  public void UpdateLocations()
  {
    locations.Clear();
    startPlayerLocation = null;
    startRemnantLocation = null;

    Location[] locs = GetComponentsInChildren<Location>();

    locations.AddRange(locs);

    foreach(Location loc in locations)
    {
      Color locColor = locationColors[(int)loc.locationType];
      loc.UpdateColor(locColor);
      loc.gameObject.name = loc.locationType.ToString();

      if(startPlayerLocation == null && loc.locationType == LocationType.StartPlayer)
      {
        startPlayerLocation = loc;
      }
      if (startRemnantLocation == null && loc.locationType == LocationType.StartRemnant)
      {
        startRemnantLocation = loc;
      }

    }
  }

  public void Awake()
  {
    if(Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public void Init()
  {

  }
}
