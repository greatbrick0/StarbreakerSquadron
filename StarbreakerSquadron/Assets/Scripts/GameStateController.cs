using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameStateController : NetworkBehaviour
{
    public enum EndGameBehavior { Quit, Restart }

    public static GameStateController instance;

    [HideInInspector]
    public UnityEvent attemptLeaveEvent;

    [field: SerializeField, Display]
    public long gameStartTime { get; private set; }
    [SerializeField]
    private float serverReadyTime = 3.0f;
    private bool isServerReady = false;
    [SerializeField]
    private float gameMaxDuration = 600;
    [SerializeField]
    private float postGameMaxDuration = 60;
    [SerializeField]
    private EndGameBehavior endGameBehavior = EndGameBehavior.Restart;

    private NetworkVariable<bool> forcePostGame = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<double> postGameStartTime = new NetworkVariable<double>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsPostGame => forcePostGame.Value || GetCurrentGameAge() > gameMaxDuration;

    NetworkVariable<long> sendGameStartTime = new NetworkVariable<long>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        if (IsServer)
        {
            gameStartTime = GetTime();
            sendGameStartTime.Value = gameStartTime;
        }
        else
        {
            gameStartTime = sendGameStartTime.Value;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

            if (!isServerReady && GetCurrentGameAge() > serverReadyTime)
{
                Network.sharedInstance.BeginServerReady();
                isServerReady = true;
            }

            double currentGameAge = GetCurrentGameAge();

            // Auto-trigger post-game if timer expires
            if (!forcePostGame.Value && currentGameAge > gameMaxDuration)
            {
                TriggerPostGame();
            }

            // Check if post-game duration has ended
            if (forcePostGame.Value && currentGameAge > postGameStartTime.Value + postGameMaxDuration)
            {
                HandleEndGame();
            }
        }
    }

    private void HandleEndGame()
    {
        if (endGameBehavior == EndGameBehavior.Quit)
        {
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            Application.Quit();
        }
        else
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        if (ClientManager.instance != null) ClientManager.instance.ClearSpawnSpots();
        NetworkManager.Singleton.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void TriggerPostGame()
    {
        if (!IsServer) return;
        if (forcePostGame.Value) return;

        forcePostGame.Value = true;
        postGameStartTime.Value = GetCurrentGameAge();
    }

    private void OnEnable()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDisable()
    {
        if (instance == this) instance = null;
    }

    public double GetCurrentGameAge()
    {
        return TimeSpan.FromMilliseconds(GetTime() - gameStartTime).TotalSeconds;
    }

    public float GetGameMaxDuration()
    {
        return gameMaxDuration;
    }

    public float GetGameRemianingTime()
    {
        return GetGameMaxDuration() - (float)GetCurrentGameAge();
    }

    private long GetTime()
    {
        return (long)(DateTime.UtcNow - StringUtils.epochStart).TotalMilliseconds;
    }
}
