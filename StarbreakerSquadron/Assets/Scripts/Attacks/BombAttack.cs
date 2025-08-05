using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class BombAttack : Attack
{
    [SerializeField]
    private GameObject explosionObj;
    private GameObject explosionRef;

    [SerializeField]
    private string explosionColour = "#cccccc";
    [SerializeField]
    private float explosionLifeTime = 0.05f;

    protected override void Update()
    {
        if (!used) return;

        age += Time.deltaTime;
        if (!IsServer)
        {
            anticipator.AnticipateMove(originPos + (age * ((speed * direction) + extraVelocity)));
        }
        else
        {
            rb.linearVelocity = (speed * direction) + extraVelocity;

            if (age >= lifetime)
            {
                CreateExplosion();
                ResetToHiddenRpc();
            }
        }
    }

    protected override void HitTerrain()
    {
        CreateExplosion();
        ResetToHiddenRpc();
    }

    protected override void HitTargetable(Targetable targetable)
    {
        if (targetable.team != team)
        {
            targetable.TakeDamage(secondaryPower);
            CreateExplosion();
            ResetToHiddenRpc();
        }
    }

    private void CreateExplosion()
    {
        AttackInfo attackInfo;
        attackInfo = new AttackInfo(
            team,
            primaryPower,
            transform.position,
            explosionLifeTime,
            explosionColour,
            0,
            aoeSize
            );
        explosionRef = Instantiate(explosionObj);
        explosionRef = Instantiate(explosionObj);
        explosionRef.transform.position = attackInfo.originPos;
        explosionRef.GetComponent<NetworkObject>().Spawn(true);
        explosionRef.GetComponent<Attack>().SetValuesRpc(attackInfo);
    }
}
