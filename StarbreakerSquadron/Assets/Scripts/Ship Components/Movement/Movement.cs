using UnityEngine;
using Unity.Netcode;

public abstract class Movement : NetworkBehaviour
{
    [Display]
    public Vector2 inputVector;

    public float maxSpeed = 5f;

    public abstract void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default);

    public abstract Vector2 ReadVelocity();

    public virtual float RecommendTurnDirection(Vector2 targetPoint)
    {
        float wantedAngle = Vector2.SignedAngle(transform.up, targetPoint);
        return Mathf.Clamp(wantedAngle / 360, -1, 1);
    }
}
