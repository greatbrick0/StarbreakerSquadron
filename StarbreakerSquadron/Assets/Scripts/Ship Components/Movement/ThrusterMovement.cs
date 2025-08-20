using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections.Generic;

public class ThrusterMovement : AccelMovement
{
    private Targetable health;

    [SerializeField]
    private string property = "NewSchool";

    [SerializeField, Display]
    private float rotationSpeed = 90f;
    [SerializeField, Min(0.0f)]
    private float reverseStrength = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anticipator = GetComponent<AnticipatedNetworkTransform>();
        health = GetComponent<Targetable>();
    }

    private void Start()
    {
        PropertyGetter properties = PropertyGetter.propertiesInstance;
        string statColour = gameObject.tag;
        StartCoroutine(properties.GetValue((val) => rotationSpeed = val, "RotationSpeed", property, statColour));
        StartCoroutine(properties.GetValue((val) => maxSpeed = val, "Speed", property, statColour));
        StartCoroutine(properties.GetValue((val) => accelPower = val, "Acceleration", property, statColour));
        StartCoroutine(properties.GetValue((val) => dragPower = val, "DragPower", property, statColour));
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

    private void HandleThrustVisuals(Vector2 accel)
    {
        MainThrustVisuals(accel);
    }

    public override float RecommendTurnDirection(Vector2 targetPoint)
    {
        //rough average of the two ideas of "forward" gives desired behaviour for the ai
        Vector2 forward = Vector2.Lerp(ReadVelocity().normalized, transform.up, 0.5f).normalized;
        float wantedAngle = Vector3.Cross(forward, (targetPoint - transform.position.FlattenVec3()).normalized).z;
        wantedAngle *= -1;
        return Mathf.Clamp(wantedAngle, -1, 1);
    }
}
