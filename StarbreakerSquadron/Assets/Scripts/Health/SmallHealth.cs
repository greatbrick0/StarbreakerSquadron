using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class SmallHealth : Targetable
{
    [field: SerializeField]
    public int maxHealth { get; private set; } = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private UnityEvent deathEvent;

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
        if(!IsServer) return;

        if(amount < 0) return;
        currentHealth.Value -= amount;
        timeSinceLastDamage = 0f;
        //print(currentHealth.Value);
    }

    public void Die()
    {
        print("dead");
        isAlive = false;
        deathEvent.Invoke();
    }
}
