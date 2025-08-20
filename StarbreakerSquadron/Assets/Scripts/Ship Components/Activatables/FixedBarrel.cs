using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FixedBarrel : NetworkBehaviour, IActivatable
{
    [SerializeField]
    protected GameObject bulletObj;
    protected GameObject bulletRef;

    [field: SerializeField]
    protected Rigidbody2D rb { get; private set; }
    [SerializeField]
    protected List<Vector3> barrels = new List<Vector3>();

    [field: SerializeField]
    protected string property { get; private set; } = "FixedSingleBarrel";
    [field: SerializeField]
    protected Teams team { get; private set; } = Teams.Environment;
    [SerializeField]
    protected string bulletColour = "#cccccc";
    [SerializeField]
    protected float cooldown = 1.0f;
    [SerializeField]
    protected bool inheritVelocity = false;
    [SerializeField, Range(0f, 1f)]
    protected float inheritPortion = 0.7f;

    [Header("Bullet properties")]
    [SerializeField, Display]
    protected int bulletDamage = 10;
    [SerializeField, Display]
    protected float bulletLifeTime = 0.7f;
    [SerializeField, Display]
    protected float bulletSpeed = 30f;

    private void Start()
    {
        if(rb == null) inheritVelocity = false;

        PropertyGetter properties = PropertyGetter.propertiesInstance;
        string statColour = gameObject.tag;
        StartCoroutine(properties.GetValue((val) => bulletDamage = Mathf.RoundToInt(val), "Damage", property, statColour));
        StartCoroutine(properties.GetValue((val) => bulletLifeTime = val, "BulletLifetime", property, statColour));
        StartCoroutine(properties.GetValue((val) => bulletSpeed = val, "BulletSpeed", property, statColour));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 barrel in barrels)
        {
            Gizmos.DrawRay(barrel.SetZ() + transform.position, transform.up.RotateDegrees(barrel.z) * 0.3f);
        }
    }

    public virtual void Activate()
    {
        if(IsServer)
        {
            foreach (Vector3 barrel in barrels) FireBullet(barrel);
        }
        else
        {
            GetComponent<AudioSource>().Play();
        }
    }

    protected virtual void FireBullet(Vector3 barrel)
    {
        AttackInfo attackInfo;
        attackInfo = new AttackInfo(
            team,
            bulletDamage,
            transform.localToWorldMatrix.MultiplyPoint3x4(barrel.SetZ()),
            bulletLifeTime,
            bulletColour,
            bulletSpeed,
            1,
            transform.up.RotateDegrees(barrel.z),
            inheritVelocity ? InheritedVector() : Vector2.zero
            );
        bulletRef = Instantiate(bulletObj);
        bulletRef.transform.position = attackInfo.originPos;
        bulletRef.GetComponent<NetworkObject>().Spawn(true);
        bulletRef.GetComponent<Attack>().SetValuesRpc(attackInfo);
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public float GetProjectileSpeed()
    {
        return bulletSpeed;
    }

    protected Vector2 InheritedVector()
    {
        return inheritPortion * Vector2.Dot(rb.linearVelocity, rb.transform.up) * rb.transform.up;
    }
}
