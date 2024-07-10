using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  [SerializeField]
  private InputField idField;
  [SerializeField]
  private InputField passwordField;

  private static UIManager _instance;

  public static UIManager Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<UIManager>();

        if (_instance == null)
        {
          GameObject singletonObject = new();
          _instance = singletonObject.AddComponent<UIManager>();
          singletonObject.name = typeof(UIManager).ToString() + " (Singleton)";
        }

        DontDestroyOnLoad(_instance.gameObject);
      }
      return _instance;
    }
  }

  private void Awake()
  {
    if (_instance == null)
    {
      _instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (_instance != this)
    {
      Destroy(gameObject);
    }
  }

  public void OnLogIn()
  {
    if (GameManager.Instance.onWait) return;
    string id = idField.text;
    string password = passwordField.text;

    Debug.Log("ID: " + id + ", PW: " + password);
    WebSocketClient.Instance.OnLoginSubmit(id, password);
    GameManager.Instance.onWait = true;
  }
}
