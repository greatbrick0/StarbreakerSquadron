using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Netcode.Components;

public class Attack : NetworkBehaviour
{
    protected bool used = false;
    public bool pooled = false;
    public GameObject originPrefab;
    public float timeUnused { get; protected set; } = 0.0f;

    protected Rigidbody2D rb;
    protected AnticipatedNetworkTransform anticipator;
    protected Collider2D col;
    [SerializeField]
    protected SpriteRenderer sprite;
    [SerializeField]
    protected SimpleLinearTrail customTrail;
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
    protected float aoeSize;
    [SerializeField, Display]
    protected Vector2 direction;
    protected Vector2 originPos;
    protected Vector2 extraVelocity;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anticipator = GetComponent<AnticipatedNetworkTransform>();
        col = GetComponent<Collider2D>();
        
        // Ensure pooled objects start disabled
        if (pooled && !used) this.enabled = false;
    }

    protected virtual void Update()
    {
        if (!used)
        {
            if (pooled) timeUnused += 1.0f * Time.deltaTime;
            return;
        }

        age += Time.deltaTime;
        if (!IsServer)
        {
            anticipator.AnticipateMove(originPos + (age * ((speed * direction) + extraVelocity)));
        }
        else
        {
            if (age >= lifetime)
            {
                ResetToHiddenRpc();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        
        int layer = collision.gameObject.layer;
        if (layer == 3) // Terrain
        {
            HitTerrain();
        }
        else if (layer == 6 || layer == 7) // SmallUnit or BigUnit
        {
            if (collision.gameObject.TryGetComponent(out Targetable targetable))
            {
                HitTargetable(targetable);
            }
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

    protected virtual void OnDisable()
    {
        if (customTrail != null) customTrail.Clear();
    }

    protected virtual void ValueInitialize()
    {
        ColorUtility.TryParseHtmlString(colourString, out Color parsedColour);
        sprite.color = parsedColour;
        SetTrailColour(parsedColour);
        
        age = 0;

        if (IsServer && rb != null)
        {
            rb.linearVelocity = (speed * direction) + extraVelocity;
        }
    }

    protected void SetTrailColour(Color fullColour)
    {
        if (customTrail == null) return;

        customTrail.SetColor(fullColour);
        customTrail.SetOpacity(trailOpacity);
        customTrail.UpdateTrail(direction, speed, extraVelocity);
    }

    public bool GetUsed()
    {
        return used;
    }

    [Rpc(SendTo.Everyone)]
    public void SetValuesRpc(AttackInfo newAttackInfo)
    {
        this.enabled = true; // Enable script for Update
        if (col != null) col.enabled = true;
        sprite.gameObject.SetActive(true);
        used = true;

        team = newAttackInfo.team;
        colourString = newAttackInfo.colour;
        primaryPower = newAttackInfo.primaryPower;
        secondaryPower = newAttackInfo.secondaryPower;
        lifetime = newAttackInfo.lifetime;
        speed = newAttackInfo.speed;
        aoeSize = newAttackInfo.aoeSize;
        direction = newAttackInfo.direction;
        originPos = newAttackInfo.originPos;
        extraVelocity = newAttackInfo.extraVelocity;
        
        ValueInitialize();
    }

    [Rpc(SendTo.Everyone)]
    protected void ResetToHiddenRpc()
    {
        if (customTrail != null) customTrail.Clear();
        if (col != null) col.enabled = false;
        sprite.gameObject.SetActive(false);
        if(rb != null) rb.linearVelocity = Vector2.zero;
        used = false;
        timeUnused = 0;

        team = Teams.Environment;
        sprite.color = Color.white;
        SetTrailColour(Color.white);
        primaryPower = 0;
        secondaryPower = 0;
        lifetime = 0;
        age = 0;
        speed = 0;
        aoeSize = 1;
        direction = Vector2.zero;
        extraVelocity = Vector2.zero;

        if (IsServer)
        {
            if (pooled)
            {
                BulletPoolManager.instance.ReturnToPool(originPrefab, this);
            }
            else
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
        
        this.enabled = false; // Disable script to stop Update
    }
    }
