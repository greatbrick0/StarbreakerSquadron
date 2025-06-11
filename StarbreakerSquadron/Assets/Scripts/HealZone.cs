using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HealZone : MonoBehaviour
{
    private bool isServer;
    [field: SerializeField]
    public Teams team { get; protected set; }
    [SerializeField]
    private LineRenderer beamRenderer;
    [SerializeField]
    private float beamBaseWidth = 0.1f;
    [SerializeField]
    private float beamWidthVariance = 0.05f;
    [SerializeField]
    private float beamVarianceFrequency = 2.0f;
    private float animTime = 0.0f;

    private List<GameObject> targetedUnits = new List<GameObject>();

    private void Start()
    {
        isServer = Network.sharedInstance.IsDedicatedServer;
    }

    private void Update()
    {
        if (!isServer)
        {
            animTime += Time.deltaTime;
            if (animTime > 100) animTime -= Mathf.PI * 30;

            List<Vector3> points = new List<Vector3>();
            foreach (GameObject targetedUnit in targetedUnits)
            {
                var health = targetedUnit.GetComponent<SmallHealth>();
                if (health.GetHealth() < health.maxHealth)
                {
                    points.Add(beamRenderer.transform.position);
                    points.Add(targetedUnit.transform.position);
                }
            }
            beamRenderer.positionCount = points.Count;
            beamRenderer.startWidth = beamBaseWidth + Mathf.Sin(animTime * beamVarianceFrequency) * beamWidthVariance;
            beamRenderer.endWidth = beamBaseWidth + Mathf.Sin(animTime * beamVarianceFrequency) * beamWidthVariance;
            if (points.Count > 0) beamRenderer.SetPositions(points.ToArray());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) return;

        if (collision.gameObject.TryGetComponent(out SmallHealth smallTargetable))
        {
            if(smallTargetable.team == team)
            {
                targetedUnits.Add(collision.gameObject);
                Debug.Log("Added to targets");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3) return;

        if (targetedUnits.Contains(collision.gameObject))
        {
            targetedUnits.Remove(collision.gameObject);
            Debug.Log("Removed from targets");
        }
    }
}
