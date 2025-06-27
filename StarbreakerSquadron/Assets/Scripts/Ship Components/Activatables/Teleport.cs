using Unity.Netcode;
using UnityEngine;

public class Teleport : NetworkBehaviour, IActivatable
{
    [SerializeField]
    private float cooldown = 12f;
    [SerializeField]
    private Transform target;

    public void Activate()
    {
        
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public void Preview()
    {

    }
}
