using UnityEngine;
using Unity.Netcode;

public abstract class Targetable : NetworkBehaviour
{
    public bool isAlive { get; protected set; } = true;
    public float timeSinceLastDamage { get; protected set; } = 0.0f;

    [SerializeField]
    public Teams team;

    public abstract void TakeDamage(int amount);
}

public enum Teams
{
    Environment,
    Green,
    Yellow,
}
