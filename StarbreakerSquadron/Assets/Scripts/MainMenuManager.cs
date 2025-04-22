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
        HandleAuthentication();
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
    }

    public void BeginPlayClient(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        _netManager.StartClient();
    }

    public void BeginPlayServer(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        _netManager.StartServer();
    }
}
