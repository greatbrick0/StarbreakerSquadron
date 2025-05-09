using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;

public class HeavyMovement : Movement
{
    private Rigidbody2D rb;
    private AnticipatedNetworkTransform anticipator;
    private Targetable health;

    [SerializeField]
    private float maxRotationSpeed = 90f;
    [SerializeField]
    private float rotationPower = 100f;
    [SerializeField]
    private float angularDragPower = 20f;

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
            rb.angularVelocity = ApplyAngularDrag(rb.angularVelocity, angularDragPower);

            if (stunRemaining > 0.0f)
            {
                stunRemaining -= 1.0f * Time.deltaTime;
                return;
            }
            if (!health.isAlive) return;

            //Handle angular velocity
            float rotDirection = 0;
            if (inputVector.x > 0) rotDirection += rotationPower;
            else if (inputVector.x < 0) rotDirection += -rotationPower;
            
            rb.angularVelocity += rotDirection * Time.deltaTime;
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxRotationSpeed, maxRotationSpeed);

            //Handle linear velocity
            Vector2 accelDirection = Vector2.zero;
            if (inputVector.y > 0) accelDirection += accelPower * transform.up.FlattenVec3();
            else if (inputVector.y < 0) accelDirection += 0.6f * accelPower * -transform.up.FlattenVec3();

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

    private float ApplyAngularDrag(float velocity, float strength)
    {
        float drag = -Mathf.Sign(velocity) * strength;
        return velocity + Mathf.Clamp(drag * Time.deltaTime, -Mathf.Abs(velocity), Mathf.Abs(velocity));
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

    public void InstantStopVelocity()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
    }
}
