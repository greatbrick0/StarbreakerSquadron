using Unity.Netcode;
using UnityEngine;
using BrainCloud;
using BrainCloud.JsonFx.Json;
using System;
using Unity.Multiplayer.Playmode;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq.Expressions;
using Unity.Mathematics;

public class Network : MonoBehaviour
{
    public static Network sharedInstance;

    public delegate void AuthenticationRequestCompleted();
    public AuthenticationRequestCompleted authenticationRequestCompleted;
    public delegate void ShareLobbyData(string data);
    public ShareLobbyData shareLobbyData;
    public delegate void LobbyDisbanded(int reason);
    public LobbyDisbanded lobbyDisbanded;
    public delegate bool FinishAttemptsCondition();

    public BrainCloudWrapper _wrapper { get; private set; }
    public BrainCloudS2S _bcS2S { get; private set; } = new BrainCloudS2S();
    private NetworkManager _netManager;
    private ClientManager _clientManager;
    private UnityTransport _unityTransport;
    private string _roomAddress;
    private int _roomPort;
    public string clientPasscode { get; private set; } = "000000";
    public string clientProfileId { get; private set; }
    [SerializeField]
    private string _lobbyId;
    public ushort selectedShipIndex = 0;
    [Display]
    public bool selectionDataApplied = false;
    private SuccessCallback onUpdateReadySuccess;

    public bool IsDedicatedServer { get; private set; }

    private void Awake()
    {
        if (sharedInstance != null)
        {
            Destroy(gameObject);
            return;
        }

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
            _bcS2S.Request(request, OnLobbyDataInit);
        }
        else
        {
            _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
            _wrapper.Init();

            if (HasAuthenticatedPreviously()) Reconnect();
            else SceneManager.LoadScene("Login", LoadSceneMode.Single);
        }

        _clientManager = GetComponent<ClientManager>();
        _clientManager.Initialize(IsDedicatedServer, _lobbyId);
    }

    private void Start()
    {
        if (IsDedicatedServer)
        {
            _unityTransport.SetConnectionData("0.0.0.0", 7777);
            BeginPlayServer("OpenLevel");
        }
    }

    private void Update()
    {
        if (IsDedicatedServer) _bcS2S.RunCallbacks();
        else _wrapper.RunCallbacks();
    }

    public void BeginPlayServer(string sceneIndex)
    {
        _netManager.StartServer();
        _netManager.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
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

    private void Reconnect()
    {
        SuccessCallback success = (responseData, cbObject) =>
        {
            HandleAuthenticationSuccess(responseData, cbObject);
            Debug.Log("Reconnected " + _wrapper.GetStoredAnonymousId());
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        };

        _wrapper.Reconnect(success, null);
    }

    public void HandleAuthenticationSuccess(string responseData, object cbObject)
    {
        Debug.Log("brainCloud client version: " + BrainCloudClientVersion);

        var response = JsonReader.Deserialize<Dictionary<string, object>>(responseData);
        var data = response["data"] as Dictionary<string, object>;
        clientProfileId = data["id"] as string;
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
                var reason = data["reason"] as Dictionary<string, object>;
                var reasonCode = (int)reason["code"];
                if (lobbyDisbanded != null) lobbyDisbanded(reasonCode);
                StopCoroutine(AttemptStartClient());
                selectionDataApplied = false;
                break;

            case "ROOM_READY":
                UpdateConnectData(data);
                _wrapper.LobbyService.UpdateReady(_lobbyId, true, FormatExtraLobbyData(), onUpdateReadySuccess);
                break;

            case "ROOM_ASSIGNED":
                UpdateConnectData(data);
                break;

            case "MEMBER_UPDATE":
                var newMember = data["member"] as Dictionary<string, object>;
                var newMemberProfileId = newMember["profileId"] as string;
                if(newMemberProfileId == clientProfileId)
                {
                    StartCoroutine(AttemptStartClient());
                }
                break;
        }
    }

    private IEnumerator AttemptStartClient()
    {
        yield return new WaitUntil(() => selectionDataApplied);
        _netManager.StartClient();
    }

    private void UpdateConnectData(Dictionary<string, object> data)
    {
        var connectData = data["connectData"] as Dictionary<string, object>;

        var ports = connectData["ports"] as Dictionary<string, object>;
        _roomPort = (int)ports["7777/tcp"];
        _roomAddress = (string)connectData["address"];
        _unityTransport.ConnectionData.Address = _roomAddress;
        _unityTransport.ConnectionData.Port = (ushort)_roomPort;
        clientPasscode = data["passcode"] as string;
    }

    private void OnLobbyDataInit(string responseString)
    {
        // Called once, on the server

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

    public void DisconnectFromSession()
    {
        SuccessCallback success = (responseData, cbObject) =>
        {
            _netManager.Shutdown();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            selectionDataApplied = false;
        };
        _wrapper.LobbyService.LeaveLobby(_lobbyId, success, null);
    }

    private Dictionary<string, object> FormatExtraLobbyData()
    {
        onUpdateReadySuccess = (string jsonResponse, object cbObject) => { selectionDataApplied = true; };

        Dictionary<string, object> extraData = new Dictionary<string, object>()
        {
            {"selectedShipIndex", selectedShipIndex },
        };
        return extraData;
    }

    public static void StartRepeatAttemptServerRequest(Dictionary<string, object> request, BrainCloudS2S.S2SCallback callback, FinishAttemptsCondition condition, float frequency)
    {
        if (!sharedInstance.IsDedicatedServer) return;
        sharedInstance.StartCoroutine(sharedInstance.RepeatAttemptServerRequest(request, callback, condition, frequency));
    }

    private IEnumerator RepeatAttemptServerRequest(Dictionary<string, object> request, BrainCloudS2S.S2SCallback callback, FinishAttemptsCondition condition, float frequency)
    {
        while (!condition())
        {
            _bcS2S.Request(request, callback);
            yield return new WaitForSeconds(frequency);
        }
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

    [ContextMenu("Attempt Parse Json Test")]
    public void AttemptParse()
    {
        JsonParseFunc("");
    }

    private void JsonParseFunc(string responseJson)
    {
        
    }
}
