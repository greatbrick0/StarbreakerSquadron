using UnityEngine;

public class ThrusterMovement : Movement
{
    private Rigidbody2D rb;

    [SerializeField]
    private float rotationSpeed = 90f;
    [SerializeField]
    private float accelPower = 500f;
    [SerializeField]
    private float dragPower = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 drag = -rb.linearVelocity.normalized * dragPower;
        rb.linearVelocity += Vector2.ClampMagnitude(drag * Time.deltaTime, rb.linearVelocity.magnitude);

        Vector2 accelDirection = Vector2.zero;
        transform.Rotate(inputVector.x * rotationSpeed * Time.deltaTime * Vector3.back);
        if (inputVector.y > 0)
        {
            accelDirection += accelPower * Time.deltaTime * VecUtils.FlattenVec3(transform.up);
        }

        rb.linearVelocity += accelDirection * Time.deltaTime;
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        rb.angularVelocity = 0;
    }
}
