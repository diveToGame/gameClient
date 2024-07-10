using UnityEngine;

public class GameManager : MonoBehaviour
{
  private static GameManager _instance;

  public static GameManager Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<GameManager>();

        if (_instance == null)
        {
          GameObject singletonObject = new();
          _instance = singletonObject.AddComponent<GameManager>();
          singletonObject.name = typeof(GameManager).ToString() + " (Singleton)";
        }

        DontDestroyOnLoad(_instance.gameObject);
      }
      return _instance;
    }
  }

  public bool onWait = false;

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

  public void ExitGame()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }
}
