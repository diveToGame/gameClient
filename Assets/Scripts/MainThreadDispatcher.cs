using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
  private static MainThreadDispatcher _instance;

  public static MainThreadDispatcher Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<MainThreadDispatcher>();

        if (_instance == null)
        {
          GameObject singletonObject = new();
          _instance = singletonObject.AddComponent<MainThreadDispatcher>();
          singletonObject.name = typeof(MainThreadDispatcher).ToString() + " (Singleton)";
        }

        DontDestroyOnLoad(_instance.gameObject);
      }
      return _instance;
    }
  }

  private static readonly Queue<Action> _executionQueue = new();

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

  private void Update()
  {
    lock (_executionQueue)
    {
      while (_executionQueue.Count > 0)
      {
        _executionQueue.Dequeue().Invoke();
      }
    }
  }

  public void Enqueue(Action action)
  {
    lock (_executionQueue)
    {
      _executionQueue.Enqueue(action);
    }
  }
}
