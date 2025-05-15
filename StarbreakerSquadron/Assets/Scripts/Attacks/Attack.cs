using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Netcode.Components;

public class Attack : NetworkBehaviour
{
    private bool used = false;

    protected Rigidbody2D rb;
    protected AnticipatedNetworkTransform anticipator;
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
    protected float lifetime;
    protected float age;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected Vector2 direction;
    protected Vector2 originPos;
    protected Vector2 extraVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anticipator = GetComponent<AnticipatedNetworkTransform>();
    }

    protected virtual void Update()
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
                ResetToHiddenRpc();
            }
        }
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
            print(targetable.team);
            targetable.TakeDamage(primaryPower);
            ResetToHiddenRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetValuesRpc(AttackInfo newAttackInfo)
    {
        GetComponent<Collider2D>().enabled = true;
        sprite.gameObject.SetActive(true);
        used = true;

        team = newAttackInfo.team;
        ColorUtility.TryParseHtmlString(newAttackInfo.colour, out Color parsedColour);
        sprite.color = parsedColour;
        primaryPower = newAttackInfo.primaryPower;
        secondaryPower = newAttackInfo.secondaryPower;
        lifetime = newAttackInfo.lifetime;
        age = 0;
        speed = newAttackInfo.speed;
        direction = newAttackInfo.direction;
        originPos = newAttackInfo.originPos;
        extraVelocity = newAttackInfo.extraVelocity;
    }

    [Rpc(SendTo.Everyone)]
    private void ResetToHiddenRpc()
    {
        GetComponent<Collider2D>().enabled = false;
        sprite.gameObject.SetActive(false);
        rb.linearVelocity = Vector2.zero;
        used = false;

        team = Teams.Environment;
        sprite.color = Color.white;
        primaryPower = 0;
        secondaryPower = 0;
        lifetime = 0;
        age = 0;
        speed = 0;
        direction = Vector2.zero;
        extraVelocity = Vector2.zero;

        if(IsServer) GetComponent<NetworkObject>().Despawn(true);
    }
}
