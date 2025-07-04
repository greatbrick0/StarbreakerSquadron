using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class HeavyMovement : AccelMovement
{
    private Targetable health;

    [SerializeField]
    private float maxRotationSpeed = 90f;
    [SerializeField]
    private float rotationPower = 100f;
    [SerializeField]
    private float angularDragPower = 20f;

    [SerializeField, Min(0.0f)]
    private float reverseStrength = 0.6f;

    [SerializeField]
    private List<Thrust> leftThrustVisuals = new List<Thrust>();
    [SerializeField]
    private List<Thrust> rightThrustVisuals = new List<Thrust>();

    private NetworkVariable<float> sendAngularVelocity = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> sendAngularAccel = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
            HandleThrustVisuals(sendAccel.Value, sendAngularAccel.Value);
            anticipator.AnticipateMove(transform.position + (Time.deltaTime * sendVelocity.Value.SetZ()));
        }
        else
        {
            rb.linearVelocity = ApplyDrag(rb.linearVelocity, dragPower, Time.deltaTime);
            rb.angularVelocity = ApplyAngularDrag(rb.angularVelocity, angularDragPower, Time.deltaTime);
            Vector2 accelDirection = Vector2.zero;
            float rotDirection = 0;

            if (stunRemaining > 0.0f) stunRemaining -= 1.0f * Time.deltaTime;
            else
            {
                if (!health.isAlive) return;

                //Handle angular velocity
                if (inputVector.x > 0) rotDirection += rotationPower;
                if (inputVector.x < 0) rotDirection += -rotationPower;
                rb.angularVelocity += rotDirection * Time.deltaTime;
                rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxRotationSpeed, maxRotationSpeed);

                //Handle linear velocity
                if (inputVector.y > 0) accelDirection += accelPower * transform.up.FlattenVec3();
                else if (inputVector.y < 0) accelDirection += -reverseStrength * accelPower * transform.up.FlattenVec3();
                rb.linearVelocity += accelDirection * Time.deltaTime;
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
            }

            sendAngularVelocity.Value = rb.angularVelocity;
            sendAngularAccel.Value = rotDirection;
            sendVelocity.Value = rb.linearVelocity;
            sendAccel.Value = accelDirection;
        }
    }

    private float ApplyAngularDrag(float velocity, float strength, float delta)
    {
        float drag = -Mathf.Sign(velocity) * strength;
        return velocity + Mathf.Clamp(drag * delta, -Mathf.Abs(velocity), Mathf.Abs(velocity));
    }

    public override void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default)
    {
        if (setVelocity)
        {
            rb.linearVelocity = newVelocity;
            rb.angularVelocity = 0;
        }
        stunRemaining = duration;
    }

    private void HandleThrustVisuals(Vector2 accel, float spin)
    {
        MainThrustVisuals(accel);
        foreach (Thrust visual in leftThrustVisuals)
        {
            visual.powered = spin > 0f;
        }
        foreach (Thrust visual in rightThrustVisuals)
        {
            visual.powered = spin < 0f;
        }
    }
}
