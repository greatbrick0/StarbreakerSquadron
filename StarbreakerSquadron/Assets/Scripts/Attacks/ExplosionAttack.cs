using UnityEngine;

public class ExplosionAttack : Attack
{
    [SerializeField]
    private float visualRemainTime = 2.0f;

    protected override void Awake()
    {
        return; // do nothing 
    }

    protected override void Update()
    {
        if (!used) return;

        age += Time.deltaTime;
        if (IsServer)
        {
            if(age >= visualRemainTime)
            {
                ResetToHiddenRpc();
            }
            else if (age >= lifetime)
            {
                GetComponent<Collider2D>().enabled = false;
            }

        }
        else
        {
            sprite.color = sprite.color.ChangeAlpha(1 - (age / visualRemainTime));
        }
    }

    protected override void HitTerrain()
    {
        return; // do nothing 
    }

    protected override void HitTargetable(Targetable targetable)
    {
        if (targetable.team != team)
        {
            targetable.TakeDamage(primaryPower);
        }
    }

    protected override void ValueInitialize()
    {
        base.ValueInitialize();
        
        GetComponent<CircleCollider2D>().radius = aoeSize;
        sprite.transform.localScale = Vector2.one * aoeSize;
    }
}
