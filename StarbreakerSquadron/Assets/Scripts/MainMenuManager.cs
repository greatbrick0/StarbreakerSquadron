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

    public void BeginPlayClient(string sceneIndex)
    {
        _netManager.StartClient();
        _netManager.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }

    public void BeginPlayServer(string sceneIndex)
    {
        _netManager.StartServer();
        _netManager.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
    }
}
