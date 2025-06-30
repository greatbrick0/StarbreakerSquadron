using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Netcode.Components;

public class Attack : NetworkBehaviour
{
    protected bool used = false;

    protected Rigidbody2D rb;
    protected AnticipatedNetworkTransform anticipator;
    [SerializeField]
    protected SpriteRenderer sprite;
    [SerializeField]
    protected TrailRenderer trail;
    [SerializeField, Range(0f, 1f)]
    private float trailOpacity = 0.3f;

    [Header("Values")]
    [SerializeField]
    protected Teams team;
    [SerializeField, Display]
    protected string colourString;
    [SerializeField, Display]
    protected int primaryPower;
    [SerializeField, Display]
    protected int secondaryPower;
    [SerializeField, Display]
    protected float lifetime;
    protected float age;
    [SerializeField, Display]
    protected float speed;
    [SerializeField, Display]
    protected Vector2 direction;
    protected Vector2 originPos;
    protected Vector2 extraVelocity;

    protected virtual void Awake()
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
            targetable.TakeDamage(primaryPower);
            ResetToHiddenRpc();
        }
    }

    protected virtual void ValueInitialize()
    {
        ColorUtility.TryParseHtmlString(colourString, out Color parsedColour);
        sprite.color = parsedColour;
        SetTrailColour(parsedColour);

        age = 0;
    }

    protected void SetTrailColour(Color fullColour)
    {
        if (trail == null) return;

        fullColour.a = trailOpacity;
        trail.startColor = fullColour;
        trail.endColor = fullColour;
    }

    [Rpc(SendTo.Everyone)]
    public void SetValuesRpc(AttackInfo newAttackInfo)
    {
        GetComponent<Collider2D>().enabled = true;
        sprite.gameObject.SetActive(true);
        used = true;

        team = newAttackInfo.team;
        colourString = newAttackInfo.colour;
        primaryPower = newAttackInfo.primaryPower;
        secondaryPower = newAttackInfo.secondaryPower;
        lifetime = newAttackInfo.lifetime;
        speed = newAttackInfo.speed;
        direction = newAttackInfo.direction;
        originPos = newAttackInfo.originPos;
        extraVelocity = newAttackInfo.extraVelocity;

        ValueInitialize();
    }

    [Rpc(SendTo.Everyone)]
    protected void ResetToHiddenRpc()
    {
        GetComponent<Collider2D>().enabled = false;
        sprite.gameObject.SetActive(false);
        if(rb != null) rb.linearVelocity = Vector2.zero;
        used = false;

        team = Teams.Environment;
        sprite.color = Color.white;
        SetTrailColour(Color.white);
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
