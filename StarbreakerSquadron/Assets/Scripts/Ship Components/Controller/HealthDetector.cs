using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthDetector : MonoBehaviour
{
    private List<Transform> targets = new List<Transform>();
    private Transform closestTarget;

    [SerializeField]
    private Teams team = Teams.Environment;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) return;

        if (collision.gameObject.TryGetComponent(out SmallHealth smallTargetable))
        {
            if (smallTargetable.team != team)
            {
                targets.Add(collision.transform);
                if(closestTarget == null)
                {
                    closestTarget = FindClosestTarget();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) return;

        if (targets.Contains(collision.transform))
        {
            targets.Remove(collision.transform);
            closestTarget = FindClosestTarget();
        }
    }

    private Transform FindClosestTarget()
    {
        if (targets.Count == 0) return null;

        Transform output = targets[0];
        float shortestDistance = (targets[0].position - transform.position).sqrMagnitude;
        if (targets.Count == 1) return output;

        foreach (Transform ii in targets)
        {
            if ((ii.position - transform.position).sqrMagnitude < shortestDistance)
            {
                output = ii;
                shortestDistance = (ii.position - transform.position).sqrMagnitude;
            }
        }

        return output;
    }

    public Transform GetClosestTarget()
    {
        return closestTarget;
    }
}
