using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class StraferMovement : AccelMovement
{
    private Targetable health;

    [SerializeField]
    private string property = "NewSchool";

    [SerializeField]
    private List<Thrust> leftThrustVisuals = new List<Thrust>();
    [SerializeField]
    private List<Thrust> rightThrustVisuals = new List<Thrust>();

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

                if (inputVector.magnitude > 0) accelDirection += (accelPower * ((inputVector.y * transform.up) + (inputVector.x * transform.right))).FlattenVec3();

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
        foreach (Thrust visual in leftThrustVisuals)
        {
            visual.powered = Vector2.Dot(accel.normalized, transform.right) < -0.5f;
        }
        foreach (Thrust visual in rightThrustVisuals)
        {
            visual.powered = Vector2.Dot(accel.normalized, transform.right) > 0.5f;
        }
    }
}
