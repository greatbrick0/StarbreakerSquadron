using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using BrainCloud.JsonFx.Json;

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
    private EditProfileManager profileEditManager;
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
        _bcNetwork._wrapper.PlayerStateService.ReadUserState(ReadUserStateSuccess, null);
        if (Application.isEditor) ActivateEditorMode();
    }
    private void OnDisable()
    {
        _bcNetwork.shareLobbyData -= SetMatchLoadingText;
    }

    private void ReadUserStateSuccess(string jsonResponse, object cbObject)
    {
        var response = JsonReader.Deserialize<Dictionary<string, object>>(jsonResponse);
        var data = response["data"] as Dictionary<string, object>;
        ParsePlayerData(data);
    }

    public void ParsePlayerData(Dictionary<string, object> newUserData)
    {
        string username = newUserData["playerName"] as string;
        if (username != string.Empty) profileEditScreen.GetComponent<EditProfileManager>().SetUsername(username);
        else profileEditScreen.GetComponent<EditProfileManager>().GenerateUsername(newUserData["profileId"] as string);
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
        if (!canLeaveProfileEdit) return;

        ChangeActiveScreen(1);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    public void ChangeActiveScreen(int newIndex)
    {
        PlayerPrefs.Save();
        canvas.GetChild(activeScreen).gameObject.SetActive(false);
        canvas.GetChild(newIndex).gameObject.SetActive(true);
        activeScreen = newIndex;
    }
}
