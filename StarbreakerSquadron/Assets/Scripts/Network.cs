using Unity.Netcode;
using UnityEngine;
using BrainCloud;
using BrainCloud.JsonFx.Json;
using System;
using static Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.AutomaticSelection;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using Unity.Multiplayer.Playmode;
using System.Linq;

public class Network : MonoBehaviour
{
    public delegate void AuthenticationRequestCompleted();

    public static Network sharedInstance;
    private BrainCloudWrapper _wrapper;
    private BrainCloudS2S _bcS2S = new BrainCloudS2S();
    private NetworkManager _netManager;

    public bool IsDedicatedServer;

    void Awake()
    {
        IsDedicatedServer = (Application.isBatchMode && !Application.isEditor) || (CurrentPlayer.ReadOnlyTags().Contains("server") && Application.isEditor);
        Debug.Log(IsDedicatedServer ? "This is dedicated server" : "This is a client");

        _netManager = GetComponent<NetworkManager>();

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
            //_bcS2S.Authenticate();
            _bcS2S.LoggingEnabled = true;
        }
        else
        {
            _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
            _wrapper.Init();
        }

        Debug.Log("brainCloud client version: " + BrainCloudClientVersion);
    }

    void Update()
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
        return false;
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
