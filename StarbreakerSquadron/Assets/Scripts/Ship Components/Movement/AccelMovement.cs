using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

public abstract class AccelMovement : Movement
{
    protected Rigidbody2D rb;
    protected AnticipatedNetworkTransform anticipator;

    [SerializeField, Display]
    protected float accelPower = 15f;
    [SerializeField, Display]
    protected float dragPower = 1.0f;

    [SerializeField, Display]
    protected float stunRemaining = 0.0f;

    [SerializeField]
    protected List<Thrust> forwardThrustVisuals = new List<Thrust>();
    [SerializeField]
    protected List<Thrust> backwardThrustVisuals = new List<Thrust>();

    protected NetworkVariable<Vector2> sendVelocity = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    protected NetworkVariable<Vector2> sendAccel = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override Vector2 ReadVelocity()
    {
        return sendVelocity.Value;
    }

    protected Vector2 ApplyDrag(Vector2 velocity, float strength, float delta)
    {
        Vector2 drag = -velocity.normalized * strength;
        return velocity + Vector2.ClampMagnitude(drag * delta, velocity.magnitude);
    }

    public override void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default)
    {
        if (setVelocity) rb.linearVelocity = newVelocity;
        stunRemaining = duration;
    }

    public void InstantStopVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected void MainThrustVisuals(Vector2 accel)
    {
        foreach (Thrust visual in forwardThrustVisuals)
        {
            visual.powered = Vector2.Dot(accel.normalized, transform.up) > 0.5f;
        }
        foreach (Thrust visual in backwardThrustVisuals)
        {
            visual.powered = Vector2.Dot(accel.normalized, transform.up) < -0.5f;
        }
    }
}
