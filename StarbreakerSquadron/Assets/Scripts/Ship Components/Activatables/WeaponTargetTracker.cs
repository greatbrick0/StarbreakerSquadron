using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTargetTracker : MonoBehaviour
{
    private List<Transform> targets = new List<Transform>();
    private Transform closestTarget;

    [SerializeField]
    private bool leadingAim = true;
    [SerializeField]
    private FixedBarrel barrel;
    [SerializeField]
    private Transform spriteTransform;
    [SerializeField]
    private Teams team = Teams.Environment;

    void Update()
    {
        closestTarget = FindClosestTarget();
        if(closestTarget != null)
        {
            Vector3 aimPos = closestTarget.position;
            if (leadingAim) aimPos = CalcLeadPos(closestTarget);
            transform.rotation = Quaternion.FromToRotation(Vector2.up, aimPos - transform.position);
            spriteTransform.rotation = transform.rotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) return;

        if (collision.gameObject.TryGetComponent(out SmallHealth smallTargetable))
        {
            if (smallTargetable.team != team)
            {
                targets.Add(collision.transform);
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
        if(targets.Count == 1) return output;

        foreach (Transform ii in targets)
        {
            if((ii.position - transform.position).sqrMagnitude < shortestDistance)
            {
                output = ii;
                shortestDistance = (ii.position - transform.position).sqrMagnitude;
            }
        }

        return output;
    }

    private Vector3 CalcLeadPos(Transform target)
    {
        if (target.gameObject.TryGetComponent(out Movement movement))
        {
            float distToTarget = Vector3.Distance(target.position, transform.position);
            float timeToTarget = distToTarget / barrel.GetProjectileSpeed();
            return target.position + (movement.ReadVelocity() * timeToTarget).SetZ();
        }
        else
        {
            return target.position;
        }
    }
}
