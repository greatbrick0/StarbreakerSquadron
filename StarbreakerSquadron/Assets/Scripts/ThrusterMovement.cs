using UnityEngine;

public class ThrusterMovement : Movement
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = inputVector;
    }
}
