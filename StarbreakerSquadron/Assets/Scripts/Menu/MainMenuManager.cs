using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    private Network _bcNetwork;
    private NetworkManager _netManager;

    [Header("Screens")]
    [SerializeField]
    private GameObject authenticateScreen;
    [SerializeField]
    private GameObject matchLoadingScreen;
    [SerializeField]
    private GameObject profileEditScreen;
    [Display]
    public bool canLeaveProfileEdit = true;

    [Header("Other")]
    [SerializeField]
    private GameObject JoinLobbyButton;
    [SerializeField]
    private GameObject forceStartClientButton;
    [SerializeField]
    private GameObject matchLoadingBox;

    private void Start()
    {
        _bcNetwork = Network.sharedInstance;
        _netManager = _bcNetwork.GetComponent<NetworkManager>();
        _bcNetwork.shareLobbyData += SetMatchLoadingText;
        if(Application.isEditor) ActivateEditorMode();

        if(_bcNetwork.IsDedicatedServer)
        {
            BeginPlayServer("OpenLevel");
        }
        else
        {
            HandleAuthentication();
        }
    }
    private void OnDisable()
    {
        _bcNetwork.shareLobbyData -= SetMatchLoadingText;
    }

    public void HandleAuthentication()
    {
        if (Network.sharedInstance.HasAuthenticatedPreviously())
            Network.sharedInstance.Reconnect(OnAuthenticationRequestComplete);
        else
            Network.sharedInstance.RequestAnonymousAuthentication(OnAuthenticationRequestComplete);
    }

    public void OnAuthenticationRequestComplete(Dictionary<string, object> initialUserData)
    {
        string username = initialUserData["playerName"] as string;
        profileEditScreen.GetComponent<EditProfileManager>().SetUsername(username);

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

        matchLoadingScreen.SetActive(true);
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

    public void SetMatchLoadingText(string newText)
    {
        matchLoadingBox.GetComponent<TMP_InputField>().text = newText;
    }

    private void ActivateEditorMode()
    {
        forceStartClientButton.SetActive(true);
        RectTransform rect = JoinLobbyButton.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, rect.sizeDelta.y);
    }

    public void StartProfileEdit()
    {
        profileEditScreen.SetActive(true);
    }

    public void FinishProfileEdit()
    {
        if(canLeaveProfileEdit) profileEditScreen.SetActive(false);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
