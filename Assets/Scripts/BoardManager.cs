using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
  public static BoardManager Instance;

  public List<Location> locations = new List<Location>();

  [ReadOnly] public Location startPlayerLocation;
  [ReadOnly] public Location startRemnantLocation;

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
