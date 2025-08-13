using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

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
    [SerializeField]
    float waypointSuccessRadius = 3.0f;
    [SerializeField]
    float waypointCarefulRadius = 10.0f;
    [SerializeField]
    float carefulSpeed = 2.0f;

    private Movement movement;
    private WeaponsHolder weaponsHolder;
    private Rigidbody2D rb;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        weaponsHolder = GetComponent<WeaponsHolder>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsServer) return;

        inputVec = Vector2.zero;
        inputActives = 0b0000;
        float rotDir = clockwise ? 1 : -1;
        if (waypoints.Count <= 1)
        {
            inputVec = new Vector2(rotDir, 1);
        }
        else
        {
            Debug.DrawLine(transform.position, waypoints[currentWaypoint], Color.yellow);
            float product = Vector3.Dot(transform.up, (waypoints[currentWaypoint] - transform.position).normalized);
            float distance = Vector3.Distance(waypoints[currentWaypoint], transform.position);
            float speed = rb.linearVelocity.magnitude;
            if (product > 0.95f)
            {
                inputVec.y = (distance < waypointCarefulRadius && speed > carefulSpeed) ? 0 : 1;
            }
            else if(product > 0.8f)
            {
                inputVec.x = rotDir;
                secondsRotated += Time.deltaTime;
                inputVec.y = (distance < waypointCarefulRadius && speed > carefulSpeed) ? 0 : 1;
            }
            else
            {
                inputVec.x = rotDir;
                secondsRotated += Time.deltaTime;
            }

            if(secondsRotated > 4.0f || distance < waypointSuccessRadius)
            {
                currentWaypoint++;
                currentWaypoint %= waypoints.Count;
                secondsRotated = 0f;
            }
        }
        movement.inputVector = inputVec;
        weaponsHolder.inputActives = inputActives;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int ii = 0; ii < waypoints.Count; ii++) 
        {
            Gizmos.DrawLine(waypoints[ii], waypoints[(ii + 1) % waypoints.Count]);
            Gizmos.DrawWireSphere(waypoints[ii], waypointSuccessRadius);
        }
        foreach (Vector3 ii in waypoints) 
        {
            Gizmos.DrawWireSphere(ii, waypointSuccessRadius);
        }
    }
}
