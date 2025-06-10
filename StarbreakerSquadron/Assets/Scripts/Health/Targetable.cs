using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public abstract class Targetable : NetworkBehaviour
{
    [field: SerializeField]
    public bool isAlive { get; protected set; } = true;
    public float timeSinceLastDamage { get; protected set; } = 0.0f;

    [field: SerializeField]
    public Teams team { get; protected set; }

    [SerializeField]
    public UnityEvent deathEvent;
    [SerializeField]
    public UnityEvent respawnEvent;

    public abstract void TakeDamage(int amount);

    public void BecomeHidden()
    {
        isAlive = false;
        deathEvent.Invoke();
    }

    public void BecomeShown()
    {
        isAlive = true;
        respawnEvent.Invoke();
    }
}

public enum Teams
{
    Environment,
    Green,
    Yellow,
}
