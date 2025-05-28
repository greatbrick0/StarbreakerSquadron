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
    private Transform canvas;
    [SerializeField, Display]
    private int activeScreen = 1;
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
        if (Application.isEditor) ActivateEditorMode();

        if(_bcNetwork.IsDedicatedServer)
        {
            
        }
        else
        {
            _bcNetwork.authenticationRequestCompleted += OnAuthenticationRequestComplete;
        }
    }
    private void OnDisable()
    {
        _bcNetwork.authenticationRequestCompleted -= OnAuthenticationRequestComplete;
        _bcNetwork.shareLobbyData -= SetMatchLoadingText;
    }

    public void OnAuthenticationRequestComplete(Dictionary<string, object> initialUserData)
    {
        string username = initialUserData["playerName"] as string;
        if(username != string.Empty) profileEditScreen.GetComponent<EditProfileManager>().SetUsername(username);
        else
        {
            string profileIdSubstring = (initialUserData["profileId"] as string).Substring(0, 8);
            Debug.Log(profileIdSubstring);
            profileEditScreen.GetComponent<EditProfileManager>().AttemptChangeUsername("Player_" + profileIdSubstring);
        }

        ChangeActiveScreen(1);
        Debug.Log("brainCloud client version: " + Network.sharedInstance.BrainCloudClientVersion);
    }

    public void BeginClientJoinLobby()
    {
        var algo = new Dictionary<string, object>();
        algo["strategy"] = "ranged-absolute";
        algo["alignment"] = "center";
        algo["ranges"] = new List<int> { 1000 };
        Network.sharedInstance._wrapper.LobbyService.FindOrCreateLobby(
            "CustomGame", 0, 1, algo,
            new Dictionary<string, object>(), true,
            new Dictionary<string, object>(), "all",
            new Dictionary<string, object>(),
            null, null, null
            );

        ChangeActiveScreen(3);
    }

    public void ForceConnectClient()
    {
        _netManager.StartClient();
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

    public void FinishProfileEdit()
    {
        if(canLeaveProfileEdit) ChangeActiveScreen(1);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    public void ChangeActiveScreen(int newIndex)
    {
        canvas.GetChild(activeScreen).gameObject.SetActive(false);
        canvas.GetChild(newIndex).gameObject.SetActive(true);
        activeScreen = newIndex;
    }
}
