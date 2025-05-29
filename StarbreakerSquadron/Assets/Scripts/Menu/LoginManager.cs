using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using BrainCloud;

public class LoginManager : MonoBehaviour
{
    private Network _bcNetwork;

    [Header("Email Create")]
    [SerializeField]
    private TMP_Text emailCreateErrorLabel;
    [SerializeField]
    private TMP_InputField emailCreateUsername;
    [SerializeField]
    private TMP_InputField emailCreateEmail;
    [SerializeField]
    private TMP_InputField emailCreatePassword;
    [SerializeField]
    private GameObject emailCreateCreateButton;
    [SerializeField]
    private GameObject emailCreateBackButton;

    void Start()
    {
        _bcNetwork = Network.sharedInstance;
        _bcNetwork.authenticationRequestCompleted += ContinueToMain;
    }

    private void OnDisable()
    {
        _bcNetwork.authenticationRequestCompleted -= ContinueToMain;
    }

    private void ContinueToMain(Dictionary<string, object> initialUserData)
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void RequestAnonymousAuthentication()
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            _bcNetwork.HandleAuthenticationSuccess(responseData, cbObject);
            Debug.Log("Anonymously connected");
        };

        _bcNetwork._wrapper.AuthenticateAnonymous(success, null);
    }

    public void RequestEmailAuthentication(string email, string password)
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            _bcNetwork.HandleAuthenticationSuccess(responseData, cbObject);
            Debug.Log("Made email account");
        };
        BrainCloud.SuccessCallback setUsername = (responseData, cbObject) =>
        {
            _bcNetwork._wrapper.PlayerStateService.UpdateName(emailCreateUsername.text, success, null);
        };

        _bcNetwork._wrapper.AuthenticateEmailPassword(email, password, true, setUsername, EmailAuthError);
    }

    private void EmailAuthError(int status, int reasonCode, string jsonError, object cbObject)
    {
        emailCreateBackButton.SetActive(true);
        emailCreateCreateButton.SetActive(true);
        switch (reasonCode)
        {
            default:
            case ReasonCodes.UNKNOWN_AUTH_ERROR:
                emailCreateErrorLabel.text = "Failed!";
                break;
            case ReasonCodes.EMAIL_NOT_VALID:
                emailCreateErrorLabel.text = "Invalid email format!";
                break;
            case ReasonCodes.INVALID_PASSWORD_CONTENT:
                emailCreateErrorLabel.text = "Password is not strong enough!";
                break;
        }
        Debug.LogError(jsonError);
    }

    public void RequestEmailSignIn(string email, string password)
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            _bcNetwork.HandleAuthenticationSuccess(responseData, cbObject);
            Debug.Log("Connected with email");
        };

        _bcNetwork._wrapper.AuthenticateEmailPassword(email, password, false, success, null);
    }

    public void AttemptCreateEmailAccount()
    {
        if(emailCreateUsername.text.Length < 3)
        {
            emailCreateErrorLabel.text = "Username is too short!";
            return;
        }
        emailCreateBackButton.SetActive(false);
        emailCreateCreateButton.SetActive(false);
        RequestEmailAuthentication(emailCreateEmail.text, emailCreatePassword.text);
    }
}
