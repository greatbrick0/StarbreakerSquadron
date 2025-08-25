using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FleetSpawner : NetworkBehaviour
{
    [Serializable]
    public class FleetUnit
    {
        [field: SerializeField]
        public GameObject unitObj { get; private set; }
        [field: SerializeField]
        public Vector3 unitOffset { get; private set; }
        [field: SerializeField]
        public bool requiredKill = false;
    }

    [SerializeField]
    private GameObject warpEffectObj;
    private GameObject warpEffectRef;

    [SerializeField]
    private float scheduledSpawnTime = 300.0f;
    private bool spawned = false;
    [SerializeField]
    private Transform fleetWaypoint;
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
    [SerializeField]
    private float fleetWaypointSpeed = 4.0f;
    [SerializeField]
    private int currentWaypoint = 0;
    [SerializeField]
    private int nextWaypoint = 1;
    [SerializeField]
    private List<FleetUnit> fleetUnits = new List<FleetUnit>();
    [SerializeField, Display]
    private int remainingRequiredKills = 0;

    private void Awake()
    {
        foreach (FleetUnit ii in fleetUnits)
        {
            if (ii.requiredKill) remainingRequiredKills += 1;
        }
    }

    private void Update()
    {
        if (!spawned)
        {
            if(GameStateController.instance.GetGameRemianingTime() <= scheduledSpawnTime)
            {
                if(clearZone.GetClosestTarget() == null)
                {
                    spawned = true;
                    if(IsServer) StartFleetSpawn();
                }
            }
        }
        else
        {
            if(fleetWaypoint.position == waypoints[nextWaypoint])
            {
                currentWaypoint = nextWaypoint;
                nextWaypoint += 1;
                nextWaypoint %= waypoints.Count;
            }
            fleetWaypoint.LookAt(waypoints[nextWaypoint]);
            fleetWaypoint.position = fleetWaypoint.position + (fleetWaypoint.forward * Mathf.Min(fleetWaypointSpeed * Time.deltaTime, Vector3.Distance(fleetWaypoint.position, waypoints[nextWaypoint])));
        }
    }

    private void StartFleetSpawn()
    {
        WarpEffect.WarpCallback spawnFunc = () =>
        {
            foreach(FleetUnit ii in fleetUnits)
            {
                GameObject unitRef;
                unitRef = Instantiate(ii.unitObj, transform);
                unitRef.transform.position = warpCentre + ii.unitOffset.SetZ();
                unitRef.transform.rotation = Quaternion.Euler(0, 0, ii.unitOffset.z);
                unitRef.GetComponent<NetworkObject>().Spawn(true);
                FleetFollowerController controller = unitRef.GetComponent<FleetFollowerController>();
                controller.SetTrackedTransform(fleetWaypoint, ii.unitOffset.SetZ());
                if (ii.requiredKill) unitRef.GetComponent<SmallHealth>().deathEvent.AddListener(ReduceRemainingRequiredKills);
            }
        };
        CreateWarpEffectRpc(warpRadius, warpDuration, warpCentre);
        warpEffectRef.GetComponent<WarpEffect>().warpCallback = spawnFunc;
    }

    private void ReduceRemainingRequiredKills()
    {
        remainingRequiredKills -= 1;
        if(remainingRequiredKills <= 0)
        {

        }
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
        Gizmos.color = Color.magenta;
        for (int ii = 0; ii < waypoints.Count; ii++)
        {
            Gizmos.DrawLine(waypoints[ii], waypoints[(ii + 1) % waypoints.Count]);
            Gizmos.DrawWireSphere(waypoints[ii], warpRadius);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(warpCentre, warpRadius);
    }
}
