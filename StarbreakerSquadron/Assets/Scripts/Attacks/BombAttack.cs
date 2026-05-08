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
            transform.position = originPos + (age * ((speed * direction) + extraVelocity));
            if(anticipator != null) anticipator.AnticipateMove(transform.position);
        }
        else
        {
            Vector2 currentPos = transform.position;
            Vector2 displacement = ((speed * direction) + extraVelocity) * Time.deltaTime;
            Vector2 nextPos = currentPos + displacement;

            float dist = displacement.magnitude;
            if (dist > 0.001f)
            {
                RaycastHit2D[] hits = Physics2D.CircleCastAll(currentPos, collisionRadius, displacement.normalized, dist, collisionMask);
                foreach (var hit in hits)
                {
                    if (HandleCollision(hit.collider)) return;
                }
            }

            transform.position = nextPos;

            if (age >= lifetime)
            {
                CreateExplosion();
                ResetToHiddenRpc();
            }
        }
    }

    protected override bool HandleCollision(Collider2D other)
    {
        if (other.gameObject.layer == 3) // Terrain
        {
            CreateExplosion();
            ResetToHiddenRpc();
            return true;
        }

        if (other.gameObject.TryGetComponent(out Targetable targetable))
        {
            if (targetable.team != team)
            {
                CreateExplosion();
                ResetToHiddenRpc();
                return true;
            }
        }

        // Pass through friendly units or Default layer objects that aren't targetable
        return false;
    }

    private void CreateExplosion()
    {
        if (!IsServer) return;

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
        
        explosionRef = BulletPoolManager.instance.GetBullet(explosionObj, attackInfo.originPos);
        explosionRef.GetComponent<ExplosionAttack>().SetValuesRpc(attackInfo);
    }
}
