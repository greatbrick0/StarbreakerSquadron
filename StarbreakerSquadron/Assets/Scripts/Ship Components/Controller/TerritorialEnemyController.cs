using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TerritorialEnemyController : NetworkBehaviour
{
    private enum States
    {
        Idle,
        Battle,
        Retreat
    }
    private Vector2 inputVec = Vector2.zero;
    private byte inputActives = 0;

    [SerializeField, Display]
    private States state = States.Idle;
    [SerializeField]
    private Vector2 territoryCentre = Vector2.zero;
    [SerializeField]
    private float territoryRadius = 25.0f;
    [SerializeField]
    private float retreatSuccessRadius = 3.0f;
    [SerializeField]
    private float idleMaxSpeed = 3.0f;

    [SerializeField]
    private HealthDetector playerDetector;
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
        switch (state)
        {
            case States.Idle:
                if(playerDetector.GetClosestTarget() != null)
                {
                    state = States.Battle;
                    break;
                }
                if (Vector2.Distance(territoryCentre, transform.position) > retreatSuccessRadius)
                {
                    inputVec = MoveToLocation(territoryCentre, idleMaxSpeed);
                }
                else
                {
                    inputVec = MoveToLocation(territoryCentre, 0);
                }
                    break;
            case States.Battle:
                if(playerDetector.GetClosestTarget() == null)
                {
                    state = States.Retreat;
                    break;
                }
                if(Vector2.Distance(territoryCentre, transform.position) > territoryRadius)
                {
                    state = States.Retreat;
                    break;
                }
                inputVec = MoveToLocation(playerDetector.GetClosestTarget().position);
                inputActives = 0b1111;
                break;
            case States.Retreat:
                if(Vector2.Distance(territoryCentre, transform.position) <= retreatSuccessRadius)
                {
                    state = States.Idle;
                    break;
                }
                inputVec = MoveToLocation(territoryCentre, Mathf.Max(Vector2.Distance(territoryCentre, transform.position), idleMaxSpeed));
                break;
        }

        movement.inputVector = inputVec;
        weaponsHolder.inputActives = inputActives;
    }

    private Vector2 MoveToLocation(Vector2 location, float maxSpeed = float.MaxValue)
    {
        Vector2 output;
        output.x = movement.RecommendTurnDirection(location);
        if (rb.linearVelocity.magnitude > maxSpeed) output.y = 0;
        else output.y = Vector2.Dot(transform.up, (location - transform.position.FlattenVec3()).normalized);
        Debug.DrawLine(transform.position, location, Color.yellow);
        return output;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(territoryCentre, territoryRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(territoryCentre, retreatSuccessRadius);
    }
}
