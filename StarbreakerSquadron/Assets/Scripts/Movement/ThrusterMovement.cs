using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections.Generic;

public class ThrusterMovement : Movement
{
    private Rigidbody2D rb;
    private AnticipatedNetworkTransform anticipator;
    private Targetable health;

    [SerializeField]
    private float rotationSpeed = 90f;
    [SerializeField]
    private float accelPower = 500f;
    [SerializeField]
    private float dragPower = 1.0f;
    [SerializeField, Min(0.0f)]
    private float reverseStrength = 0.3f;

    [SerializeField, Display]
    private float stunRemaining = 0.0f;

    [SerializeField]
    private List<Thrust> forwardThrustVisuals = new List<Thrust>();
    [SerializeField]
    private List<Thrust> backwardThrustVisuals = new List<Thrust>();

    private NetworkVariable<Vector2> sendVelocity = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<Vector2> sendAccel = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anticipator = GetComponent<AnticipatedNetworkTransform>();
        health = GetComponent<Targetable>();
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            Vector2 predictedVelocity = ApplyDrag(sendVelocity.Value, dragPower, Time.deltaTime);
            if (stunRemaining > 0.0f) stunRemaining -= 1.0f * Time.deltaTime;
            else
            {
                HandleThrustVisuals(sendAccel.Value);
                predictedVelocity += Time.deltaTime * sendAccel.Value;
            }
            anticipator.AnticipateMove(transform.position + (Time.deltaTime * predictedVelocity.SetZ()));
        }
        else
        {
            rb.linearVelocity = ApplyDrag(rb.linearVelocity, dragPower, Time.deltaTime);
            rb.angularVelocity = 0;
            Vector2 accelDirection = Vector2.zero;

            if (stunRemaining > 0.0f) stunRemaining -= 1.0f * Time.deltaTime;
            else
            {
                if (!health.isAlive) return;

                transform.Rotate(inputVector.x * rotationSpeed * Time.deltaTime * Vector3.back);
                if (inputVector.y > 0) accelDirection += accelPower * transform.up.FlattenVec3();
                else if (inputVector.y < 0) accelDirection += -reverseStrength * accelPower * transform.up.FlattenVec3();

                rb.linearVelocity += accelDirection * Time.deltaTime;
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
            }
            
            sendVelocity.Value = rb.linearVelocity;
            sendAccel.Value = accelDirection;
            if (inputVector.magnitude != 0.0f) anticipator.SetState(transform.position);
        }
    }

    private Vector2 ApplyDrag(Vector2 velocity, float strength, float delta)
    {
        Vector2 drag = -velocity.normalized * strength;
        return velocity + Vector2.ClampMagnitude(drag * delta, velocity.magnitude);
    }

    public override void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default)
    {
        if (setVelocity) rb.linearVelocity = newVelocity;
        stunRemaining = duration;
    }

    private void HandleThrustVisuals(Vector2 accel)
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

    public void InstantStopVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
