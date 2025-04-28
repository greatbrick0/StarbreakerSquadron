using UnityEngine;
using Unity.Netcode;

public abstract class Targetable : NetworkBehaviour
{
    public enum Teams
    {
        Green,
        Yellow,
    }

    [SerializeField]
    public Teams team;

    public abstract void TakeDamage(int amount);
}
