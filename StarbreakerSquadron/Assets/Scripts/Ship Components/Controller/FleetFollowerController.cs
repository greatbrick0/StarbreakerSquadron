using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class FleetFollowerController : NetworkBehaviour
{
    private Vector2 inputVec = Vector2.zero;
    private byte inputActives = 0;

    private Transform waypoint;
    private Vector3 waypointOffset = Vector3.zero;

    [SerializeField]
    private float waypointCarefulRadius = 1.0f;
    [SerializeField]
    private float carefulSpeed = 2.0f;

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

        if(waypoint != null)
        {
            inputVec = MoveToLocation(waypoint.position + waypointOffset, waypoint.up);
        }

        movement.inputVector = inputVec;
        weaponsHolder.inputActives = inputActives;
    }

    public void SetTrackedTransform(Transform newWaypoint, Vector3 newWaypointOffset)
    {
        waypoint = newWaypoint;
        waypointOffset = newWaypointOffset;
    }

    private Vector2 MoveToLocation(Vector2 location, Vector2 forward, float maxSpeed = float.MaxValue)
    {
        Vector2 output;
        float dist = Vector2.Distance(location, transform.position);
        if (dist < 1.0f) output.x = movement.RecommendTurnDirection(location - (forward * 10));
        else output.x = movement.RecommendTurnDirection(location); 
        if (rb.linearVelocity.magnitude > maxSpeed) output.y = 0;
        else output.y = Vector2.Dot(transform.up, (location - transform.position.FlattenVec3()).normalized);
        Debug.DrawLine(transform.position, location, Color.yellow);
        return output;
    }
}
