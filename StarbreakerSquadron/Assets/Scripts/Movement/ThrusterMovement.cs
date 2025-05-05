using UnityEngine;

public class ThrusterMovement : Movement
{
    private Rigidbody2D rb;
    private Targetable health;

    [SerializeField]
    private float rotationSpeed = 90f;
    [SerializeField]
    private float accelPower = 500f;
    [SerializeField]
    private float dragPower = 1.0f;

    private float stunRemaining = 0.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Targetable>();
    }

    void Update()
    {
        if(!IsServer) return;

        rb.linearVelocity = ApplyDrag(rb.linearVelocity, dragPower);

        if(stunRemaining > 0.0f)
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

        rb.linearVelocity += accelDirection * Time.deltaTime;
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        rb.angularVelocity = 0;
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
