using Unity.Netcode;
using UnityEngine;

public class BombBarrel : FixedBarrel
{
    [SerializeField, Display]
    private float explosionRadius = 3;

    private void Start()
    {
        if (rb == null) inheritVelocity = false;

        PropertyGetter properties = PropertyGetter.propertiesInstance;
        string statColour = gameObject.tag;
        StartCoroutine(properties.GetValue((val) => bulletDamage = Mathf.RoundToInt(val), "Damage", property, statColour));
        StartCoroutine(properties.GetValue((val) => bulletLifeTime = val, "BulletLifetime", property, statColour));
        StartCoroutine(properties.GetValue((val) => bulletSpeed = val, "BulletSpeed", property, statColour));
        StartCoroutine(properties.GetValue((val) => explosionRadius = val, "AreaOfEffectRadius", property, statColour));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 barrel in barrels)
        {
            Gizmos.DrawRay(barrel.SetZ() + transform.position, transform.up.RotateDegrees(barrel.z) * 0.3f);
        }
    }

    protected override void FireBullet(Vector3 barrel)
    {
        AttackInfo attackInfo;
        attackInfo = new AttackInfo(
            team,
            0,
            transform.localToWorldMatrix.MultiplyPoint3x4(barrel.SetZ()),
            bulletLifeTime,
            bulletColour,
            bulletSpeed,
            explosionRadius,
            transform.up.RotateDegrees(barrel.z),
            inheritVelocity ? InheritedVector() : Vector2.zero,
            bulletDamage
            );
        bulletRef = Instantiate(bulletObj);
        bulletRef.transform.position = attackInfo.originPos;
        bulletRef.GetComponent<NetworkObject>().Spawn(true);
        bulletRef.GetComponent<BombAttack>().SetValuesRpc(attackInfo);
    }
}
