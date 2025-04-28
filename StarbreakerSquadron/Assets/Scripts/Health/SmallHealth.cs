using Unity.Netcode;
using UnityEngine;

public class SmallHealth : Targetable
{
    [field: SerializeField]
    public int maxHealth { get; private set; } = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        currentHealth.Value = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentHealth.OnValueChanged += (int prevValue, int newValue) => { if (newValue <= 0) Die(); };
    }

    public override void TakeDamage(int amount)
    {
        if(!IsServer) return;

        currentHealth.Value -= amount;
        print(currentHealth.Value);
    }

    public void Die()
    {
        print("dead");
    }
}
