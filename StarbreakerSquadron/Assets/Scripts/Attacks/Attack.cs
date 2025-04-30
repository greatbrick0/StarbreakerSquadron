using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Attack : NetworkBehaviour
{
    protected Rigidbody2D rb;
    [SerializeField]
    protected SpriteRenderer sprite;

    [Header("Values")]
    [SerializeField]
    protected Teams team;
    [SerializeField]
    protected int primaryPower;
    [SerializeField]
    protected int secondaryPower;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected Vector2 direction;
    protected Vector2 originPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (!IsServer) return;

        rb.linearVelocity = speed * Time.deltaTime * direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if(collision.gameObject.layer == 3)
        {
            HitTerrain();
        }
        else if (collision.gameObject.TryGetComponent(out Targetable targetable))
        {
            HitTargetable(targetable);
        }
    }

    protected virtual void HitTerrain()
    {
        ResetToHiddenRpc();
    }

    protected virtual void HitTargetable(Targetable targetable)
    {
        if(targetable.team != team)
        {
            targetable.TakeDamage(0);
            ResetToHiddenRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetValuesRpc(AttackInfo newAttackInfo)
    {
        GetComponent<Collider2D>().enabled = true;
        sprite.gameObject.SetActive(true);

        team = newAttackInfo.team;
        ColorUtility.TryParseHtmlString(newAttackInfo.colour, out Color parsedColour);
        sprite.color = parsedColour;
        primaryPower = newAttackInfo.primaryPower;
        secondaryPower = newAttackInfo.secondaryPower;
        speed = newAttackInfo.speed;
        direction = newAttackInfo.direction;
        originPos = newAttackInfo.originPos;
    }

    [Rpc(SendTo.Everyone)]
    private void ResetToHiddenRpc()
    {
        GetComponent<Collider2D>().enabled = false;
        sprite.gameObject.SetActive(false);

        team = Teams.Environment;
        sprite.color = Color.white;
        primaryPower = 0;
        secondaryPower = 0;
        speed = 0;
        direction = Vector2.zero;
    }
}
