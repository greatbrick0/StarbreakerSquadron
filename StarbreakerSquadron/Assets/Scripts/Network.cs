using Unity.Netcode;
using UnityEngine;
using BrainCloud;
using BrainCloud.JsonFx.Json;
using System;
using Unity.Multiplayer.Playmode;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;

public class Network : MonoBehaviour
{
    public static Network sharedInstance;

    public delegate void AuthenticationRequestCompleted();
    public delegate void ShareLobbyData(string data);
    public ShareLobbyData shareLobbyData;

    public BrainCloudWrapper _wrapper { get; private set; }
    public BrainCloudS2S _bcS2S { get; private set; } = new BrainCloudS2S();
    private NetworkManager _netManager;
    private UnityTransport _unityTransport;
    private string _roomAddress;
    private int _roomPort;
    [SerializeField]
    private string _lobbyId;

    public bool IsDedicatedServer { get; private set; }

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
            _lobbyId = Environment.GetEnvironmentVariable("LOBBY_ID");
            _bcS2S.Init(appId, serverName, serverSecret, true, serverUrl);
            _bcS2S.LoggingEnabled = true;

            Dictionary<string, object> request = new Dictionary<string, object>
                {
                    { "service", "lobby" },
                    { "operation", "GET_LOBBY_DATA" },
                    { "data", new Dictionary<string, object>
                    {{ "lobbyId", _lobbyId }}
                    }
                };
            _bcS2S.Request(request, OnLobbyData);
        }
        else
        {
            _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
            _wrapper.Init();
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
        if (shareLobbyData != null) shareLobbyData(json);
        else Debug.Log(json);

        var response = JsonReader.Deserialize<Dictionary<string, object>>(json);
        var data = response["data"] as Dictionary<string, object>;

        if (data.ContainsKey("lobby") && (string)response["operation"] != "SETTINGS_UPDATE")
        {
            _lobbyId = data["lobbyId"] as string;
        }

        switch (response["operation"] as string)
        {
            case "DISBANDED":
                var reason = data["reason"]
                    as Dictionary<string, object>;
                var reasonCode = (int)reason["code"];
                break;

            case "ROOM_READY":
                UpdateConnectData(data);

                _netManager.StartClient();
                break;

            case "ROOM_ASSIGNED":
                UpdateConnectData(data);

                _wrapper.LobbyService.UpdateReady(_lobbyId, true, new Dictionary<string, object>());
                break;
        }
    }

    private void UpdateConnectData(Dictionary<string, object> data)
    {
        var connectData = data["connectData"] as Dictionary<string, object>;

        try
        {
            _roomPort = (int?)connectData["ports"] ?? -1;
        }
        catch (Exception)
        {
            var ports = connectData["ports"] as Dictionary<string, object>;
            _roomPort = (int)ports["7777/tcp"];
        }
        _roomAddress = (string)connectData["address"];
        _unityTransport.ConnectionData.Address = _roomAddress;
        _unityTransport.ConnectionData.Port = (ushort)_roomPort;
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
                {{ "lobbyId", _lobbyId }}
                }
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
}
