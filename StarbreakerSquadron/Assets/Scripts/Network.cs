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
    private NetworkManager _netManager;

    public bool IsDedicatedServer;

    void Awake()
    {
        IsDedicatedServer = (Application.isBatchMode && !Application.isEditor) || (CurrentPlayer.ReadOnlyTags().Contains("server") && Application.isEditor);
        Debug.Log(IsDedicatedServer ? "This is dedicated server" : "This is a client");

        _netManager = GetComponent<NetworkManager>();

        sharedInstance = this;
        DontDestroyOnLoad(gameObject);

        _wrapper = gameObject.AddComponent<BrainCloudWrapper>();
        _wrapper.Init();

        Debug.Log("brainCloud client version: " + BrainCloudClientVersion);
    }

    void Update()
    {
        _wrapper.RunCallbacks();
    }

    public string BrainCloudClientVersion
    {
        get { return _wrapper.Client.BrainCloudClientVersion; }
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
}
