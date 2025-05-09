using Unity.Netcode;
using UnityEngine;
using BrainCloud;
using BrainCloud.JsonFx.Json;
using System;
using Unity.VisualScripting;
using Unity.Multiplayer.Playmode;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;

public class Network : MonoBehaviour
{
    public delegate void AuthenticationRequestCompleted();

    public static Network sharedInstance;
    public BrainCloudWrapper _wrapper { get; private set; }
    private BrainCloudS2S _bcS2S = new BrainCloudS2S();
    private NetworkManager _netManager;
    private UnityTransport _unityTransport;
    private string _roomAddress;
    private int _roomPort;

    public bool IsDedicatedServer;

    private void Awake()
    {
        IsDedicatedServer = (Application.isBatchMode && !Application.isEditor) || (CurrentPlayer.ReadOnlyTags().Contains("server") && Application.isEditor);
        Debug.Log(IsDedicatedServer ? "This is dedicated server" : "This is a client");

        _netManager = GetComponent<NetworkManager>();
        _unityTransport = GetComponent<UnityTransport>();

        sharedInstance = this;
        DontDestroyOnLoad(gameObject);

        if (IsDedicatedServer)
        {
            _bcS2S = new BrainCloudS2S();
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string serverName = Environment.GetEnvironmentVariable("SERVER_NAME");
            string serverSecret = Environment.GetEnvironmentVariable("SERVER_SECRET");
            string serverUrl = "https://api.braincloudservers.com/s2sdispatcher";
            _bcS2S.Init(appId, serverName, serverSecret, true, serverUrl);
            _bcS2S.LoggingEnabled = true;
            PlayerPrefs.SetInt("IsUser", 0);

            Dictionary<string, object> request = new Dictionary<string, object>
                {
                    { "service", "lobby" },
                    { "operation", "GET_LOBBY_DATA" },
                    { "data", new Dictionary<string, object>
                    {
                        { "lobbyId", "id" }
                    }}
                };
            _bcS2S.Request(request, OnLobbyData);
        }
        else
        {
            _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
            _wrapper.Init();
            PlayerPrefs.SetInt("IsUser", 1);
        }
    }

    private void Start()
    {
        if (IsDedicatedServer)
        {
            _unityTransport.SetConnectionData("0.0.0.0", 7777);
        }
    }

    private void Update()
    {
        if (IsDedicatedServer) _bcS2S.RunCallbacks();
        else _wrapper.RunCallbacks();
    }

    public string BrainCloudClientVersion
    {
        get { return IsDedicatedServer ? string.Empty : _wrapper.Client.BrainCloudClientVersion; }
    }

    public bool HasAuthenticatedPreviously()
    {
        return _wrapper.GetStoredProfileId() != "" && _wrapper.GetStoredAnonymousId() != "";
    }

    public bool IsAuthenticated()
    {
        return IsDedicatedServer ? _bcS2S.IsAuthenticated : _wrapper.Client.Authenticated;
    }

    public void Reconnect(AuthenticationRequestCompleted authenticationRequestCompleted = null)
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            HandleAuthenticationSuccess(responseData, cbObject, authenticationRequestCompleted);
            Debug.Log("Reconnected " + _wrapper.GetStoredAnonymousId());
        };

        _wrapper.Reconnect(success, null);
    }

    public void RequestAnonymousAuthentication(AuthenticationRequestCompleted authenticationRequestCompleted = null)
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            HandleAuthenticationSuccess(responseData, cbObject, authenticationRequestCompleted);
            Debug.Log("Anonymously connected");
        };

        _wrapper.AuthenticateAnonymous(success, null);
    }

    private void HandleAuthenticationSuccess(string responseData, object cbObject, AuthenticationRequestCompleted authenticationRequestCompleted)
    {
        if (authenticationRequestCompleted != null)
            authenticationRequestCompleted();

        _wrapper.RTTService.RegisterRTTLobbyCallback(OnLobbyEvent);
        _wrapper.RTTService.EnableRTT(OnRttEnabled, null, RTTConnectionType.WEBSOCKET);
    }

    private void OnRttEnabled(string jsonResponse, object cbObject)
    {
        
    }

    private void OnLobbyEvent(string json)
    {
        var response = JsonReader.Deserialize<Dictionary<string, object>>(json);
        var data = response["data"] as Dictionary<string, object>;

        switch (response["operation"] as string)
        {
            case "DISBANDED":
                var reason = data["reason"]
                    as Dictionary<string, object>;
                var reasonCode = (int)reason["code"];
                break;

            case "ROOM_READY":
                var connectData = data["connectData"] as Dictionary<string, object>;
                var ports = connectData["ports"] as Dictionary<string, object>;
                _roomAddress = (string)connectData["address"];
                _roomPort = (int)ports["7777/tcp"];
                _unityTransport.ConnectionData.Address = _roomAddress;
                _unityTransport.ConnectionData.Port = (ushort)_roomPort;
                break;

            case "ROOM_ASSIGNED":
                _netManager.StartClient();
                break;
        }
    }

    private void OnLobbyData(string responseString)
    {
        Dictionary<string, object> response =
        JsonReader.Deserialize<Dictionary<string, object>>(responseString);
        int status = (int)response["status"];
        if (status != 200)
        {
            Application.Quit(1);
            return;
        }

        // Tell brainCloud we are ready to accept connections
        Dictionary<string, object> request = new Dictionary<string, object>
            {
                { "service", "lobby" },
                { "operation", "SYS_ROOM_READY" },
                { "data", new Dictionary<string, object>
                {
                    { "lobbyId", "id" }
                }}
            };
        _bcS2S.Request(request, null);
    }

    public void PrintSpawnedObjects()
    {
        string output = "";
        foreach (var obj in _netManager.SpawnManager.SpawnedObjects)
        {
            output += obj.Key + " " + obj.Value.name + ". ";
        }
        Debug.Log(output);
    }

    public void PrintIsDedicatedServer()
    {
        Debug.Log("IsUser: " + PlayerPrefs.GetInt("IsUser"));
    }
}
