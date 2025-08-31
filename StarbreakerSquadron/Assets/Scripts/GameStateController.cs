using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameStateController : NetworkBehaviour
{
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
            if (!isServerReady && GetCurrentGameAge() > serverReadyTime)
            {
                Network.sharedInstance.BeginServerReady();
                isServerReady = true;
            }
            if (GetCurrentGameAge() > gameMaxDuration + postGameMaxDuration)
            {
                Application.Quit();
            }
        }
        else
        {
            if(GetCurrentGameAge() > gameMaxDuration + postGameMaxDuration)
            {
                //attemptLeaveEvent.Invoke();
            }
        }
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
