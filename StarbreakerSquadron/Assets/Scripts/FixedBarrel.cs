using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FixedBarrel : MonoBehaviour
{
    [SerializeField]
    private GameObject bulletObj;
    private GameObject bulletRef;

    [SerializeField]
    private List<Vector3> barrels = new List<Vector3>();

    [SerializeField]
    Teams team = Teams.Environment;
    [SerializeField]
    string bulletColour = "#cccccc";
    [SerializeField]
    float cooldown = 1.0f;

    public void Activate(WeaponsHolder.WeaponSlot slot)
    {
        AttackInfo attackInfo;
        foreach (Vector3 barrel in barrels)
        {
            bulletRef = Instantiate(bulletObj);
            bulletRef.transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(barrel.SetZ());
            bulletRef.GetComponent<NetworkObject>().Spawn(true);
            attackInfo = new AttackInfo(
                team,
                10,
                bulletRef.transform.position,
                1.0f,
                bulletColour,
                3000,
                transform.up.RotateDegrees(barrel.z)
                );
            bulletRef.GetComponent<Attack>().SetValuesRpc(attackInfo);
        }

        slot.SetCooldown(cooldown);
    }
}
