using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class ActiveRotator : NetworkBehaviour, IActivatable
{
    private float cooldown = 0.1f;
    [SerializeField]
    private Transform rotateTarget;
    [SerializeField]
    private ActiveRotator counterRotator;
    [SerializeField]
    private float rotateSpeed = 90.0f;
    [SerializeField]
    private bool clockwise = false;
    private float activeDuration = 0.0f;

    public void Activate()
    {
        counterRotator.Cancel();
        activeDuration = cooldown;
    }

    public void Cancel()
    {
        activeDuration = 0f;
    }

    private void Update()
    {
        if (activeDuration > 0){
            activeDuration -= 1.0f * Time.deltaTime;
            rotateTarget.Rotate((clockwise ? 1 : -1) * rotateSpeed * Time.deltaTime * Vector3.back);
        }
    }

    public float GetCooldown()
    {
        return cooldown;
    }
}
