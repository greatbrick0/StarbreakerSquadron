using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FixedBarrel : NetworkBehaviour, IActivatable
{
    [SerializeField]
    private GameObject bulletObj;
    private GameObject bulletRef;

    [SerializeField]
    private List<Vector3> barrels = new List<Vector3>();

    [SerializeField]
    private Teams team = Teams.Environment;
    [SerializeField]
    private string bulletColour = "#cccccc";
    [SerializeField]
    private float cooldown = 1.0f;

    [Header("Bullet properties")]
    [SerializeField]
    private int bulletDamage = 10;
    [SerializeField]
    private float bulletLifeTime = 0.7f;
    [SerializeField]
    private float bulletSpeed = 30f;

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
                transform.up.RotateDegrees(barrel.z)
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
}
