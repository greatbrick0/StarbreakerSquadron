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

    public void Die()
    {
        print("dead");
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
