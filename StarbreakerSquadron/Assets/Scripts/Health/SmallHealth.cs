using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SmallHealth : Targetable
{
    [SerializeField]
    private string property = "NewSchool";

    public delegate void MaxHealthChanged(int newMax);
    private MaxHealthChanged maxHealthChanged;
    public int maxHealth { get; private set; } = 100;
    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private int regenChunkSize = 1;
    [Display]
    public float flatRegen = 0.0f;
    [SerializeField, Display]
    private float regenRemainder = 0.0f;


    private void Start()
    {
        ResetHealth();
        StartCoroutine(PropertyGetter.propertiesInstance.GetValue(
            (val) => { 
                maxHealth = Mathf.RoundToInt(val);
                ResetHealth();
                if (maxHealthChanged != null) maxHealthChanged(maxHealth);
            }, 
            "Health", property, gameObject.tag));
    }

    private void Update()
    {
        timeSinceLastDamage += Time.deltaTime;
        if (IsServer)
        {
            HandleRegen(Time.deltaTime);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AddHealthReactor((int prevValue, int newValue) => { if (newValue <= 0) Die(); });
    }

    public void AddHealthReactor(NetworkVariable<int>.OnValueChangedDelegate reaction, MaxHealthChanged maxHealthReactor = null)
    {
        currentHealth.OnValueChanged += reaction;
        if(maxHealthReactor != null) maxHealthChanged += maxHealthReactor;
    }

    public override void TakeDamage(int amount)
    {
        if (!IsServer) return;
        if (!isAlive) return;
        if (amount < 0) return;

        currentHealth.Value -= amount;
        timeSinceLastDamage = 0f;
    }

    private void HandleRegen(float delta)
    {
        if (GetHealth() < maxHealth)
        {
            regenRemainder += flatRegen * delta;
            if (regenRemainder >= regenChunkSize) currentHealth.Value += Mathf.FloorToInt(regenRemainder);
            regenRemainder %= regenChunkSize;
        }
        else
        {
            regenRemainder = 0.0f;
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " dead");
        BecomeHidden();
    }

    public void ResetHealth()
    {
        if (IsServer) currentHealth.Value = maxHealth;
    }

    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
