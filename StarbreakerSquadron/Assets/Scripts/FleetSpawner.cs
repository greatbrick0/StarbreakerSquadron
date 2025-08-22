using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class FleetSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject warpEffectObj;
    private GameObject warpEffectRef;

    [SerializeField]
    private float scheduledSpawnTime = 300.0f;
    private bool spawned = false;
    [SerializeField]
    private HealthDetector clearZone;
    [SerializeField]
    private float warpRadius = 5.0f;
    [SerializeField]
    private Vector3 warpCentre = Vector3.zero;
    [SerializeField]
    private float warpDuration = 3.0f;

    [SerializeField]
    private List<Vector3> waypoints = new List<Vector3>();

    private void Update()
    {
        if (!spawned)
        {
            if(GameStateController.instance.GetGameRemianingTime() <= scheduledSpawnTime)
            {
                if(clearZone.GetClosestTarget() == null)
                {
                    spawned = true;
                    StartFleetSpawn();
                }
            }
        }
    }

    private void StartFleetSpawn()
    {
        WarpEffect.WarpCallback spawnFunc = () =>
        {

        };
        CreateWarpEffectRpc(warpRadius, warpDuration, warpCentre);
        warpEffectRef.GetComponent<WarpEffect>().warpCallback = spawnFunc;
    }

    [Rpc(SendTo.Everyone)]
    private void CreateWarpEffectRpc(float sizeMult, float duration, Vector3 location = default)
    {
        warpEffectRef = Instantiate(warpEffectObj);
        warpEffectRef.transform.position = location;
        warpEffectRef.transform.localScale = Vector3.one * sizeMult;
        warpEffectRef.GetComponent<WarpEffect>().Initialize(IsServer, duration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(warpCentre, warpRadius);
        Gizmos.color = Color.magenta;
        for (int ii = 0; ii < waypoints.Count; ii++)
        {
            Gizmos.DrawLine(waypoints[ii], waypoints[(ii + 1) % waypoints.Count]);
            Gizmos.DrawWireSphere(waypoints[ii], warpRadius);
        }
    }
}
