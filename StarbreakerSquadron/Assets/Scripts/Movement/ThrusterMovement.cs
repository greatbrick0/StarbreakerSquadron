using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

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

    private float stunRemaining = 0.0f;

    private NetworkVariable<Vector2> sendVelocity = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anticipator = GetComponent<AnticipatedNetworkTransform>();
        health = GetComponent<Targetable>();
    }

    void Update()
    {
        if (!IsServer)
        {
            anticipator.AnticipateMove(transform.position + (Time.deltaTime * sendVelocity.Value.SetZ()));
        }
        else
        {
            rb.linearVelocity = ApplyDrag(rb.linearVelocity, dragPower);
            rb.angularVelocity = 0;

            if (stunRemaining > 0.0f)
            {
                stunRemaining -= 1.0f * Time.deltaTime;
                return;
            }
            if (!health.isAlive) return;

            Vector2 accelDirection = Vector2.zero;
            transform.Rotate(inputVector.x * rotationSpeed * Time.deltaTime * Vector3.back);
            if (inputVector.y > 0)
            {
                accelDirection += accelPower * Time.deltaTime * transform.up.FlattenVec3();
            }
            else if (inputVector.y < 0)
            {
                accelDirection += 0.3f * accelPower * Time.deltaTime * -transform.up.FlattenVec3();
            }

            rb.linearVelocity += accelDirection * Time.deltaTime;
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
            sendVelocity.Value = rb.linearVelocity;
        }
    }

    private Vector2 ApplyDrag(Vector2 velocity, float strength)
    {
        Vector2 drag = -velocity.normalized * strength;
        return velocity + Vector2.ClampMagnitude(drag * Time.deltaTime, velocity.magnitude);
    }

    public override void Stun(float duration, bool setVelocity = true, Vector2 newVelocity = default)
    {
        if (setVelocity) rb.linearVelocity = newVelocity;
        
    }

    public void InstantStopVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
