using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
  public static Logger Instance;

  public GameObject logGameObject;
  public RectTransform logContent;
  public Scrollbar verticalScrollbar;

  // Start is called before the first frame update
  void Start()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(this.gameObject);
    }
  }

  public static void Log(string logText)
  {
    if(Instance != null)
    {
      Instance.AddLog(logText);
    }
  }

  public void AddLog(string text)
  {
    StartCoroutine(CoLog(text));
  }

  public IEnumerator CoLog(string logText)
  {
    GameObject go = Instantiate(Instance.logGameObject, Instance.logContent);
    TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();

    if (tmp == null) yield break;

    tmp.text = logText;

    LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.logContent);

    yield return new WaitForEndOfFrame();
    Instance.verticalScrollbar.value = 0;
  }
  

  
}
