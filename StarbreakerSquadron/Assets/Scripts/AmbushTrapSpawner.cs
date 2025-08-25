using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class AmbushTrapSpawner : NetworkBehaviour
{
    private GameObject trapRef;
    private Targetable trapHealth;

    [SerializeField, Display]
    private float timeInactive = 0.0f;
    private NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private float kickPower = 40.0f;
    [SerializeField]
    private float kickTime = 0.3f;

    [Header("Cooldown")]
    [SerializeField]
    private bool useCooldown = true;
    [SerializeField]
    private float cooldownTime = 10.0f;
    [Header("In Proximity")]
    [SerializeField]
    private bool useInProximity = false;
    [SerializeField]
    private HealthDetector inProximityDetector;
    [Header("Out Proximity")]
    [SerializeField]
    private bool useOutProximity = false;
    [SerializeField]
    private HealthDetector outProximityDetector;
    [Header("Schedule")]
    [SerializeField]
    private bool useSchedule = false;
    [SerializeField]
    private List<Vector2> scheduleWindows = new List<Vector2>();
    private List<bool> windowsUsed = new List<bool>();

    private List<Func<bool>> conditions = new List<Func<bool>>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(transform.childCount == 0) { DebugLogLineage(" has no attached unit!"); Destroy(gameObject); return; }
        trapRef = transform.GetChild(transform.childCount - 1).gameObject;
        trapHealth = trapRef.GetComponent<Targetable>();
        if (trapHealth == null) { DebugLogLineage(" has no attached unit!"); Destroy(gameObject); return; }
        trapHealth.deathEvent.AddListener(ResetTrap);
        if (IsServer)
        {
            trapRef.transform.position = transform.position;
            trapRef.transform.rotation = transform.rotation;

            if (useCooldown) conditions.Add(CooldownCondition);
            if (useInProximity) conditions.Add(InProximityCondition);
            if (useOutProximity) conditions.Add(OutProximityCondition);
            if (useSchedule)
            {
                windowsUsed.AddRange(Enumerable.Repeat(false, 50));
                conditions.Add(ScheduleCondition);
            }
        }
        else
        {
            if (isActivated.Value) trapHealth.BecomeShown();
            else trapHealth.BecomeHidden();
        }
    }

    void Update()
    {
        if (IsServer)
        {
            isActivated.Value = trapHealth.isAlive;

            if (!trapHealth.isAlive)
            {
                timeInactive += 1.0f * Time.deltaTime;
                if (CheckSpawnConditions()) ActivateTrapRpc();
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Set Unit As Last Child")]
    private void SetTargetableAsLast()
    {
        for(int ii = 0; ii < transform.childCount; ii++)
        {
            if (transform.GetChild(ii).TryGetComponent(out Targetable health))
            {
                transform.GetChild(ii).SetAsLastSibling();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                return;
            }
        }
    }
#endif

    [Rpc(SendTo.Everyone)]
    public void ActivateTrapRpc()
    {
        trapHealth.BecomeShown();
        if(kickPower > 0) trapRef.GetComponent<Movement>().Stun(kickTime, true, transform.up * kickPower);
    }

    private void ResetTrap()
    {
        timeInactive = 0.0f;
        trapRef.transform.position = transform.position;
        trapRef.transform.rotation = transform.rotation;
    }

    private bool CheckSpawnConditions()
    {
        foreach(Func<bool> ii in conditions)
        {
            if (!ii.Invoke()) return false;
        }
        return true;
    }

    private bool CooldownCondition()
    {
        return timeInactive > cooldownTime;
    }

    private bool InProximityCondition()
    {
        return inProximityDetector.GetClosestTarget() != null;
    }

    private bool OutProximityCondition()
    {
        return outProximityDetector.GetClosestTarget() == null;
    }

    private bool ScheduleCondition()
    {
        float time = GameStateController.instance.GetGameRemianingTime();
        for (int ii = 0; ii < scheduleWindows.Count; ii++)
        {
            if (windowsUsed[ii]) continue;
            if (time <= scheduleWindows[ii].x && time > scheduleWindows[ii].y)
            {
                windowsUsed[ii] = true;
                return true;
            }
        }
        return false;
    }

    private void DebugLogLineage(string message = default)
    {
        Transform ii = transform;
        string output = ii.name;
        bool reachedTop = ii.parent == null;
        while (!reachedTop)
        {
            ii = ii.parent;
            output += ", " + ii.name;
            reachedTop = ii.parent == null;
        }
        output += message;
        Debug.LogError(output);
    }
}
