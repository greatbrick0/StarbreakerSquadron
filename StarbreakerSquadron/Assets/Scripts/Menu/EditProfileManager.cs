using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BrainCloud.JsonFx.Json;
using UnityEngine.SceneManagement;

public class EditProfileManager : MonoBehaviour
{
    private BrainCloudWrapper _wrapper;

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
        _wrapper = Network.sharedInstance._wrapper;
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

    public void GenerateUsername(string profileId)
    {
        string profileIdSubstring = profileId.Substring(0, 8);
        AttemptChangeUsername(string.Format(StringUtils.GUEST_NAME_FORMAT, profileIdSubstring));
    }

    public void AttemptChangeUsername(string newUsername)
    {
        if (previousName == newUsername) return;
        if(newUsername.Length < 3)
        {
            usernameInputField.text = previousName;
            return;
        }

        LeaveScreenPermission(false);
        _wrapper.PlayerStateService.UpdateName(newUsername, NameChangeSuccess, NameChangeFailure);
    }

    public void AttemptSignOut()
    {
        LeaveScreenPermission(false);
        BrainCloud.SuccessCallback success = (responseData, cbObject) => { Application.Quit(); };
        _wrapper.Logout(true, success, null);
    }

    private void NameChangeSuccess(string jsonResponse, object cbObject)
    {
        LeaveScreenPermission(true);
        Debug.Log(jsonResponse);
        var response = JsonReader.Deserialize<Dictionary<string, object>>(jsonResponse);
        var data = response["data"] as Dictionary<string, object>;
        string newName = data["playerName"] as string;

        usernameInputField.text = newName;
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
