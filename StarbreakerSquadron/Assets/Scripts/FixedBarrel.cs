using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FixedBarrel : NetworkBehaviour, IActivatable
{
    [SerializeField]
    private GameObject bulletObj;
    [SerializeField]
    private GameObject fakeBulletObj;
    private GameObject bulletRef;

    [SerializeField]
    private List<Vector3> barrels = new List<Vector3>();

    [SerializeField]
    private Teams team = Teams.Environment;
    [SerializeField]
    private string bulletColour = "#cccccc";
    [SerializeField]
    private float cooldown = 1.0f;

    public void Activate()
    {
        GetComponent<AudioSource>().Play();
        AttackInfo attackInfo;
        foreach (Vector3 barrel in barrels)
        {
            attackInfo = new AttackInfo(
                team,
                10,
                transform.localToWorldMatrix.MultiplyPoint3x4(barrel.SetZ()),
                0.7f,
                bulletColour,
                30f,
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
                bulletRef = Instantiate(fakeBulletObj);
                bulletRef.transform.position = attackInfo.originPos;
                bulletRef.GetComponent<FakeAttack>().SetValuesFake(attackInfo);
            }
        }
    }

    public float GetCooldown()
    {
        return cooldown;
    }
}
