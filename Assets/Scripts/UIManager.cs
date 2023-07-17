using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class UIManager : MonoBehaviour
{
  [HideInInspector]
  public static UIManager Instance;

  public Button rollBtn;


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

  public void Start()
  {
    Init();
  }

  public void Init()
  {
    HideRollBtn();
  }

  public void ShowRollBtn() { rollBtn.gameObject.SetActive(true); }
  public void HideRollBtn() { rollBtn.gameObject.SetActive(false); }
  public void SetRollBtnListener(UnityAction _action)
  {
    rollBtn.onClick.AddListener(_action);
  }
}
