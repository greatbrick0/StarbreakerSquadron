using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SmallHealth : Targetable
{
    [field: SerializeField]
    public int maxHealth { get; private set; } = 100;
    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Start()
    {
        ResetHealth();
        StartCoroutine(PropertyGetter.propertiesInstance.GetValue(
            (val) => { 
                maxHealth = Mathf.RoundToInt(val);
                ResetHealth();
            }, 
            "HealthMult", gameObject.tag, maxHealth));
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

    public void AddHealthReactor(NetworkVariable<int>.OnValueChangedDelegate reaction)
    {
        currentHealth.OnValueChanged += reaction;
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
