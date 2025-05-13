using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private Network _bcNetwork;
    private NetworkManager _netManager;

    [SerializeField]
    private GameObject authenticateScreen;

    private void Start()
    {
        _bcNetwork = Network.sharedInstance;
        _netManager = _bcNetwork.GetComponent<NetworkManager>();

        if(_bcNetwork.IsDedicatedServer)
        {
            BeginPlayServer("OpenLevel");
        }
        else
        {
            HandleAuthentication();
        }
        
    }

    public void HandleAuthentication()
    {
        if (Network.sharedInstance.HasAuthenticatedPreviously())
            Network.sharedInstance.Reconnect(OnAuthenticationRequestComplete);
        else
            Network.sharedInstance.RequestAnonymousAuthentication(OnAuthenticationRequestComplete);
    }

    public void OnAuthenticationRequestComplete()
    {
        authenticateScreen.SetActive(false);
        Debug.Log("brainCloud client version: " + Network.sharedInstance.BrainCloudClientVersion);
    }

    public void BeginClientJoinLobby()
    {
        var algo = new Dictionary<string, object>();
        algo["strategy"] = "ranged-absolute";
        algo["alignment"] = "center";
        List<int> ranges = new List<int> { 1000 };
        algo["ranges"] = ranges;
        Network.sharedInstance._wrapper.LobbyService.FindOrCreateLobby(
            "CustomGame", 0, 1, algo,
            new Dictionary<string, object>(), true,
            new Dictionary<string, object>(), "all",
            new Dictionary<string, object>(),
            null, null, null
            );
    }

    public void ForceConnectClient()
    {
        _netManager.StartClient();
    }

    public void BeginPlayServer(string sceneIndex)
    {
        _netManager.StartServer();
        _netManager.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }
}
