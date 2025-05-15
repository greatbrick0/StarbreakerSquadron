using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SmallHealth : Targetable
{
    [field: SerializeField]
    public int maxHealth { get; private set; } = 100;
    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    public UnityEvent deathEvent;

    private void Start()
    {
        if (IsServer) currentHealth.Value = maxHealth;
    }

    private void Update()
    {
        timeSinceLastDamage += Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.OnValueChanged += (int prevValue, int newValue) => { if (newValue <= 0) Die(); };
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
        isAlive = false;
        deathEvent.Invoke();
    }

    public int GetHealth()
    {
        return currentHealth.Value;
    }
}
