using UnityEngine;
using Unity.Netcode;

public abstract class Movement : NetworkBehaviour
{
    [Display]
    public Vector2 inputVector;

    public float maxSpeed = 5f;

    public abstract void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default);

    public abstract Vector2 ReadVelocity();
}
