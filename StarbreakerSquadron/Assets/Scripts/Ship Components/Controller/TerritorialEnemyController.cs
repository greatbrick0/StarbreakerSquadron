using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TerritorialEnemyController : NetworkBehaviour
{
    private Vector2 inputVec = Vector2.zero;
    private byte inputActives = 0;

    [SerializeField, Display]
    private string state = "idle";
    [SerializeField]
    private Vector2 territoryCentre = Vector2.zero;
    [SerializeField]
    private float territoryRadius = 25.0f;
    [SerializeField]
    private float retreatSuccessRadius = 3.0f;

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
            case "idle":
                if(playerDetector.GetClosestTarget() != null)
                {
                    state = "battling";
                }
                break;
            case "battling":
                if(playerDetector.GetClosestTarget() == null)
                {
                    state = "retreating";
                }
                if(Vector2.Distance(territoryCentre, transform.position) > territoryRadius)
                {
                    state = "retreating";
                }
                inputVec.x = movement.RecommendTurnDirection(playerDetector.GetClosestTarget().position);
                inputVec.y = 0.5f;
                inputActives = 0b1111;
                break;
            case "retreating":
                if(Vector2.Distance(territoryCentre, transform.position) <= retreatSuccessRadius)
                {
                    state = "idle";
                }
                inputVec.x = movement.RecommendTurnDirection(territoryCentre);
                inputVec.y = 0.5f;
                break;
        }
            
        movement.inputVector = inputVec;
        weaponsHolder.inputActives = inputActives;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(territoryCentre, territoryRadius);
    }
}
