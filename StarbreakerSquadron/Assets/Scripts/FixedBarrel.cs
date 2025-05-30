using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FixedBarrel : NetworkBehaviour, IActivatable
{
    [SerializeField]
    private GameObject bulletObj;
    private GameObject bulletRef;

    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private List<Vector3> barrels = new List<Vector3>();

    [SerializeField]
    private string property = "FixedSingleBarrel";
    [SerializeField]
    private Teams team = Teams.Environment;
    [SerializeField]
    private string bulletColour = "#cccccc";
    [SerializeField]
    private float cooldown = 1.0f;
    [SerializeField]
    private bool inheritVelocity = false;
    [SerializeField, Range(0f, 1f)]
    private float inheritPortion = 0.7f;

    [Header("Bullet properties")]
    [SerializeField, Display]
    private int bulletDamage = 10;
    [SerializeField, Display]
    private float bulletLifeTime = 0.7f;
    [SerializeField, Display]
    private float bulletSpeed = 30f;

    private void Start()
    {
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
            Gizmos.DrawRay(barrel.SetZ(), Vector3.up.RotateDegrees(barrel.z) * 0.3f);
        }
    }

    public void Activate()
    {
        
        AttackInfo attackInfo;
        foreach (Vector3 barrel in barrels)
        {
            attackInfo = new AttackInfo(
                team,
                bulletDamage,
                transform.localToWorldMatrix.MultiplyPoint3x4(barrel.SetZ()),
                bulletLifeTime,
                bulletColour,
                bulletSpeed,
                transform.up.RotateDegrees(barrel.z),
                inheritVelocity ? InheritedVector() : Vector2.zero
                );
            if (IsServer)
            {
                bulletRef = Instantiate(bulletObj);
                bulletRef.transform.position = attackInfo.originPos;
                bulletRef.GetComponent<NetworkObject>().Spawn(true);
                bulletRef.GetComponent<Attack>().SetValuesRpc(attackInfo);
            }
            else
            {
                GetComponent<AudioSource>().Play();
            }
        }
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    private Vector2 InheritedVector()
    {
        return inheritPortion * Vector2.Dot(rb.linearVelocity, rb.transform.up) * rb.transform.up;
    }
}
