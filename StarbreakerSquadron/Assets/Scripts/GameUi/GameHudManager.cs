using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static StringUtils;

public class GameHudManager : MonoBehaviour
{
    public GameHudState state { get; private set; } = GameHudState.Shocked;

    [SerializeField]
    private Transform hudHolder;
    [SerializeField]
    private GameObject gameMenu;
    [SerializeField]
    private GameObject respawnTimer;
    [SerializeField] 
    private GameObject closeMenuButton;
    [SerializeField]
    private GameObject versionLabel;

    [HideInInspector]
    public UnityEvent attemptLeaveEvent;

    [Header("Game State Display")]
    [SerializeField]
    private TMP_Text gameTimeLabel;
    [SerializeField]
    private TMP_Text matchEndTimeLabel;
    [SerializeField]
    private TMP_Text matchEndLabel;
    private GameStateController gameStateController;

    [Header("Cooldown Display")]
    [SerializeField]
    private GameObject cooldownDisplayObj;

    [Header("Health Display")]
    [Display]
    public int maxHealth = 100;
    [SerializeField, Display]
    private int currentHealth = 100;
    [SerializeField]
    private Color baseHealthColour = Color.green;
    [SerializeField, Min(0)]
    private int criticalHealthAmount = 20;
    [SerializeField]
    private Color criticalHealthColour = Color.red;
    [SerializeField, Range(0f, 1f)]
    private float warningHealthAmount = 0.4f;
    [SerializeField]
    private Color warningHealthColour = Color.yellow;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private Image healthBarFill;
    [SerializeField]
    private Image healthBarCritical;
    [SerializeField]
    private Image healthBarWarning;
    [SerializeField]
    private TMP_Text healthLabel;

    [Header("Respawn Display")]
    [SerializeField, Min(0)]
    private float deathShockTime = 2.0f;
    public float respawnProgress = 0.0f;
    [SerializeField]
    private TMP_Text respawnTimeLabel;
    [SerializeField]
    private GameObject respawnCountdownHolder;
    [SerializeField]
    private GameObject respawnButtonHolder;
    public Button respawnButton;

    private float animTime = 0.0f;

    private void Awake()
    {
        if(Network.sharedInstance.IsDedicatedServer) Destroy(gameObject);
        ChangeGameHudState(GameHudState.Shocked);
    }

    private void Update()
    {
        animTime += Time.deltaTime;
        if (animTime > 100) animTime -= Mathf.PI * 30;

        switch (state)
        {
            case GameHudState.Gameplay:
                HandleHealthBarAnimation();
                HandleGameTimer(gameTimeLabel);
                break;
            case GameHudState.Paused:
                HandleMatchEndTimer();
                break;
            case GameHudState.Shocked:
                break;
            case GameHudState.Respawn:
                HandleMatchEndTimer();
                respawnCountdownHolder.SetActive(respawnProgress > 0.0f);
                respawnButtonHolder.SetActive(respawnProgress <= 0.0f);
                respawnTimeLabel.text = string.Format(SPAWN_TIME_FORMAT, respawnProgress);
                break;
            case GameHudState.Spectating:
                break;
        }
    }

    public IEnumerator StartRespawningHud()
    {
        ChangeGameHudState(GameHudState.Shocked);
        yield return new WaitForSeconds(deathShockTime);
        respawnCountdownHolder.SetActive(respawnProgress > 0.0f);
        respawnButtonHolder.SetActive(respawnProgress <= 0.0f);
        ChangeGameHudState(GameHudState.Respawn);
    }

    public void ToggleMenuHotKey()
    {
        switch (state)
        {
            case GameHudState.Gameplay:
                ChangeGameHudState(GameHudState.Paused);
                break;
            case GameHudState.Paused:
                ChangeGameHudState(GameHudState.Gameplay);
                break;
            case GameHudState.Shocked:
                break;
            case GameHudState.Respawn:
                ChangeGameHudState(GameHudState.Spectating);
                break;
            case GameHudState.Spectating:
                ChangeGameHudState(GameHudState.Respawn);
                break;
        }
    }

    public void ChangeGameHudState(int newState)
    {
        ChangeGameHudState((GameHudState)newState);
    }

    public void ChangeGameHudState(GameHudState newState)
    {
        hudHolder.gameObject.SetActive(newState == GameHudState.Gameplay);
        closeMenuButton.SetActive(newState == GameHudState.Paused);

        switch (newState)
        {
            case GameHudState.Gameplay:
                gameMenu.SetActive(false);
                respawnTimer.SetActive(false);
                break;
            case GameHudState.Paused:
                gameMenu.SetActive(true);
                respawnTimer.SetActive(false);
                break;
            case GameHudState.Shocked:
                gameMenu.SetActive(false);
                respawnTimer.SetActive(false);
                break;
            case GameHudState.Respawn:
                gameMenu.SetActive(true);
                respawnTimer.SetActive(true);
                break;
            case GameHudState.Spectating:
                gameMenu.SetActive(false);
                respawnTimer.SetActive(true);
                break;
        }
        state = newState;
    }

    public void CreateCooldownDisplay(WeaponsHolder weaponsHolder)
    {
        GameObject cooldownDisplayRef;
        int jj = 0;
        for(int ii = 0; ii < weaponsHolder.slots.Count; ii++)
        {
            if (weaponsHolder.slots[ii].showCooldown)
            {
                cooldownDisplayRef = Instantiate(cooldownDisplayObj, hudHolder);
                cooldownDisplayRef.GetComponent<CooldownDisplay>().AssignSlot(weaponsHolder.slots[ii]);
                cooldownDisplayRef.transform.localScale = Vector3.one;
                cooldownDisplayRef.GetComponent<RectTransform>().anchoredPosition = new Vector3(-20, 70 + (jj * 50));
                jj++;
            }
        }
    }

    private void HandleHealthBarAnimation()
    {
        if (currentHealth <= criticalHealthAmount)
        {
            healthBarFill.color = Color.Lerp(baseHealthColour, criticalHealthColour, Mathf.Clamp01(Mathf.Sin(12 * animTime) + 0.7f));
            healthBarCritical.color = Color.white;
            healthBarWarning.color = Color.clear;
        }
        else if ((1.0f * currentHealth) / maxHealth < warningHealthAmount)
        {
            healthBarFill.color = Color.Lerp(baseHealthColour, warningHealthColour, Mathf.Clamp01(Mathf.Sin(12 * animTime) + 0.7f));
            healthBarCritical.color = Color.clear;
            healthBarWarning.color = Color.white;
        }
        else
        {
            healthBarFill.color = baseHealthColour;
            healthBarCritical.color = Color.clear;
            healthBarWarning.color = Color.clear;
        }
    }

    private void HandleGameTimer(TMP_Text label)
    {
        if (gameStateController == null)
        {
            label.text = string.Empty;
            gameStateController = GameStateController.instance;
            return;
        }

        TimeSpan time = TimeSpan.FromSeconds(gameStateController.GetGameRemianingTime());
        label.text = string.Format(GAME_TIME_REMAINING_FORMAT, time);
    }

    private void HandleMatchEndTimer()
    {
        if (gameStateController.GetGameRemianingTime() < 0.0f)
        {
            matchEndLabel.text = GAME_TIME_ENDED_LABEL;
            matchEndTimeLabel.text = string.Empty;
        }
        else
        {
            matchEndLabel.text = GAME_TIME_COUNTDOWN_LABEL;
            HandleGameTimer(matchEndTimeLabel);
        }
    }

    public void UpdateHealthBar(int newHealth)
    {
        currentHealth = newHealth;
        healthBar.value = (1.0f * newHealth) / maxHealth;
        healthLabel.text = newHealth.ToString();
    }

    public void UpdateHealthBarMax(int newMax)
    {
        maxHealth = newMax;
        UpdateHealthBar(currentHealth);
    }

    public void AttemptLeaveSession()
    {
        attemptLeaveEvent.Invoke();
    }
}

public enum GameHudState
{
    Gameplay = 0,
    Paused = 1,
    Shocked = 2,
    Respawn = 3,
    Spectating = 4,
}
