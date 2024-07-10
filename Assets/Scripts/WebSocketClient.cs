using UnityEngine;
using WebSocketSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using UnityEngine.SceneManagement;

public class WsEventData<T>
{
  [JsonPropertyName("event")]
  public string Event { get; set; }

  [JsonPropertyName("data")]
  public T Data { get; set; }
}

public class LogOnDto
{
  [JsonPropertyName("username")]
  public string Username { get; set; }

  [JsonPropertyName("password")]
  public string Password { get; set; }
}

public class WebSocketClient : MonoBehaviour
{
  private static WebSocketClient _instance;

  public static WebSocketClient Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<WebSocketClient>();

        if (_instance == null)
        {
          GameObject singletonObject = new();
          _instance = singletonObject.AddComponent<WebSocketClient>();
          singletonObject.name = typeof(WebSocketClient).ToString() + " (Singleton)";
        }

        DontDestroyOnLoad(_instance.gameObject);
      }
      return _instance;
    }
  }

  private WebSocket ws;

  [SerializeField]
  private string token = "invalid";
  [SerializeField]
  private string serverIP = "ws://121.168.23.171:8080/";

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

    // ws = new WebSocket("ws://localhost:8080/"); // Nest.js 서버 주소
    ws = new WebSocket(serverIP); // 외부 서버 주소

    ws.OnMessage += HandleValidate;
    ws.OnMessage += HandleLogOn;
    ws.OnMessage += HandleMessageFromServer;
    ws.OnMessage += HandleLatency;

    ws.Connect();
    ws.Send(JsonSerializer.Serialize(new WsEventData<string>
    {
      Event = "events",
      Data = DateTimeOffset.UtcNow.ToString()
    }));
  }

  private void HandleMessageFromServer(object sender, MessageEventArgs e)
  {
    Debug.Log("Message from server: " + ((WebSocket)sender).Url + ", Data : " + e.Data);
  }

  private void HandleLatency(object sender, MessageEventArgs e)
  { // Handle Latency Check
    WsEventData<long> des = null;
    try
    {
      des = JsonSerializer.Deserialize<WsEventData<long>>(e.Data);
    }
    catch { }
    if (des?.Event == "message")
    {
      Debug.Log("Latency: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - des.Data) + "ms");
    }
  }

  private void HandleLogOn(object sender, MessageEventArgs e)
  {
    WsEventData<string> des = null;
    try
    {
      des = JsonSerializer.Deserialize<WsEventData<string>>(e.Data);
    }
    catch { }
    if (des?.Event == "logon")
    {
      token = des.Data;
      if (token == null)
      {
        Debug.Log("Invalid User");
        return;
      }
      Debug.Log("Your token is: " + token);
      MainThreadDispatcher.Instance.Enqueue(() =>
      {
        SceneManager.LoadScene(1);
        GameManager.Instance.onWait = false;
      });
    }
  }

  private void HandleValidate(object sender, MessageEventArgs e)
  {
    WsEventData<string> des = null;
    try
    {
      des = JsonSerializer.Deserialize<WsEventData<string>>(e.Data);
    }
    catch { }
    if (des?.Event == "validate")
    {
      Debug.Log("LogIn Message From Server: " + des.Data);
    }
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      ws.Send(JsonSerializer.Serialize(new WsEventData<long>
      {
        Event = "message",
        Data = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
      }));
    }

    if (Input.GetKeyDown(KeyCode.B))
    {
      ws.Send(JsonSerializer.Serialize(new WsEventData<string>
      {
        Event = "validate",
        Data = token
      }));
    };
  }

  public void OnLoginSubmit(string id, string password)
  {
    if (!ws.IsAlive) return;

    ws.Send(JsonSerializer.Serialize(new WsEventData<LogOnDto>
    {
      Event = "logon",
      Data = new LogOnDto
      {
        Username = id,
        Password = password
      }
    }));
  }

  void OnDestroy()
  {
    ws.Close();
  }
}
