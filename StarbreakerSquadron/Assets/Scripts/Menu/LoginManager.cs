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

    [Header("Sign In")]
    [SerializeField]
    private TMP_Text signInErrorLabel;
    [SerializeField]
    private TMP_InputField signInEmail;
    [SerializeField]
    private TMP_InputField signInPassword;
    [SerializeField]
    private GameObject signInEnterButton;
    [SerializeField]
    private GameObject signInBackButton;

    void Start()
    {
        _bcNetwork = Network.sharedInstance;
        _bcNetwork.authenticationRequestCompleted += ContinueToMain;
    }

    private void OnDisable()
    {
        _bcNetwork.authenticationRequestCompleted -= ContinueToMain;
    }

    private void ContinueToMain()
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

        _bcNetwork._wrapper.AuthenticateEmailPassword(email, password, true, setUsername, EmailAuthenticateError);
    }

    private void EmailAuthenticateError(int status, int reasonCode, string jsonError, object cbObject)
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
        Debug.Log(jsonError);
    }

    public void RequestEmailSignIn(string email, string password)
    {
        BrainCloud.SuccessCallback success = (responseData, cbObject) =>
        {
            _bcNetwork.HandleAuthenticationSuccess(responseData, cbObject);
            Debug.Log("Connected with email");
        };

        _bcNetwork._wrapper.AuthenticateEmailPassword(email, password, false, success, EmailSignInError);
    }

    private void EmailSignInError(int status, int reasonCode, string jsonError, object cbObject)
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
            case ReasonCodes.TOKEN_DOES_NOT_MATCH_USER:
                emailCreateErrorLabel.text = "Wrong password!";
                break;
        }
        Debug.Log(jsonError);
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

    public void AttemptEmailSignIn()
    {
        signInBackButton.SetActive(false);
        signInEnterButton.SetActive(false);
        RequestEmailSignIn(signInEmail.text, signInPassword.text);
    }
}
