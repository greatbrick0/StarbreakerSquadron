using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TestEnemyController : NetworkBehaviour
{
    private Vector2 inputVec = Vector2.zero;
    private byte inputActives = 0;

    [SerializeField]
    private List<Vector3> waypoints = new List<Vector3>() { Vector3.zero };
    private int currentWaypoint = 0;
    private float secondsRotated = 0f;
    [SerializeField]
    bool clockwise = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsServer) return;

        inputVec = Vector2.zero;
        if(waypoints.Count <= 1)
        {
            inputVec = new Vector2(clockwise ? 1 : -1, 1);
        }
        else
        {
            Debug.DrawLine(transform.position, waypoints[currentWaypoint], Color.yellow);
            float product = Vector3.Dot(transform.up, (waypoints[currentWaypoint] - transform.position).normalized);
            float distance = Vector3.Distance(waypoints[currentWaypoint], transform.position);
            float speed = rb.linearVelocity.magnitude;
            if (product > 0.95f)
            {
                inputVec.y = (distance < 10.0f && speed > 2.0f) ? 0 : 1;
            }
            else if(product > 0.8f)
            {
                inputVec.x = clockwise ? 1 : -1;
                secondsRotated += Time.deltaTime;
                inputVec.y = (distance < 10.0f && speed > 2.0f) ? 0 : 1;
            }
            else
            {
                inputVec.x = clockwise ? 1 : -1;
                secondsRotated += Time.deltaTime;
            }

            if(secondsRotated > 4.0f || distance < 3.0f)
            {
                currentWaypoint++;
                currentWaypoint %= waypoints.Count;
                secondsRotated = 0f;
            }
        }
        GetComponent<Movement>().inputVector = inputVec;
    }
}
