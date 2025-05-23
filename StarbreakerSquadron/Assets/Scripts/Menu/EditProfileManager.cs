using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BrainCloud.JsonFx.Json;

public class EditProfileManager : MonoBehaviour
{
    private Network _bcNetwork;

    [SerializeField]
    private MainMenuManager mainMenuManager;
    [SerializeField]
    private TMP_Text usernameLabel;
    [SerializeField]
    private TMP_InputField usernameInputField;
    [SerializeField]
    private Transform bufferSpinner;
    [SerializeField]
    private float bufferSpinnerSpeed = 180;

    private string previousName;


    private void Start()
    {
        _bcNetwork = Network.sharedInstance;
    }

    private void Update()
    {
        bufferSpinner.Rotate(0, 0, -bufferSpinnerSpeed * Time.deltaTime);
    }

    public void SetUsername(string newUsername)
    {
        previousName = newUsername;
        usernameLabel.text = newUsername;
        usernameInputField.text = newUsername;
    }

    public void AttemptChangeUsername(string newUsername)
    {
        LeaveScreenPermission(false);
        _bcNetwork._wrapper.PlayerStateService.UpdateName(newUsername, NameChangeSuccess, NameChangeFailure);
    }

    private void NameChangeSuccess(string jsonResponse, object cbObject)
    {
        LeaveScreenPermission(true);
        Debug.Log(jsonResponse);
        var response = JsonReader.Deserialize<Dictionary<string, object>>(jsonResponse);
        var data = response["data"] as Dictionary<string, object>;
        string newName = data["playerName"] as string;

        usernameLabel.text = newName;
        previousName = newName;
    }

    private void NameChangeFailure(int status, int reasonCode, string jsonError, object cbObject)
    {
        Debug.Log("Name change failure");
        usernameInputField.text = previousName;
        LeaveScreenPermission(true);
    }

    private void LeaveScreenPermission(bool permission)
    {
        bufferSpinner.gameObject.SetActive(!permission);
        mainMenuManager.canLeaveProfileEdit = permission;
    }
}
