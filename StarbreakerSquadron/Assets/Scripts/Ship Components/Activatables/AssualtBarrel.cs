using System.Collections;
using System.Threading;
using UnityEngine;

public class AssualtBarrel : FixedBarrel
{
    [Header("Assualt properties")]
    [SerializeField]
    private float fireSpacingTime = 0.2f;
    [SerializeField]
    private int assualtFireCount = 3;
    [SerializeField]
    private bool cycleBarrels = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 barrel in barrels)
        {
            Gizmos.DrawRay(barrel.SetZ() + transform.position, Vector3.up.RotateDegrees(barrel.z) * 0.3f);
        }
    }

    public override void Activate()
    {
        if (IsServer)
        {
            StartCoroutine(FireAssualt());
        }
        else
        {
            GetComponent<AudioSource>().Play();
        }
    }

    private IEnumerator FireAssualt()
    {
        for(int ii = 0;  ii < assualtFireCount; ii++)
        {
            if (cycleBarrels)
                FireBullet(barrels[ii % barrels.Count]);
            else
                foreach (Vector3 barrel in barrels) FireBullet(barrel);

            yield return new WaitForSeconds(fireSpacingTime);
        }
    }
}
