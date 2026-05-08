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
    protected LayerMask collisionMask;
    [SerializeField]
    protected float collisionRadius = 0.15f;

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
        
        if (collisionMask == 0)
        {
            collisionMask = LayerMask.GetMask("Terrain", "SmallUnit", "BigUnit", "Default");
        }

        // Ensure pooled objects start physically and logically disabled
        if (pooled && !used)
        {
            if (col != null) col.enabled = false;
            if (rb != null) rb.simulated = false;
            this.enabled = false;
        }
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
                ResetToHiddenRpc();
            }
        }
}

    /// <summary>
    /// Handles collision. Returns true if the bullet should be destroyed.
    /// </summary>
    protected virtual bool HandleCollision(Collider2D other)
    {
        if (other.gameObject.layer == 3) // Terrain
        {
            HitTerrain();
            return true;
        }
        
        if (other.gameObject.TryGetComponent(out Targetable targetable))
        {
            if (targetable.team != team)
            {
                HitTargetable(targetable);
                return true;
            }
        }
        
        // If we hit something that isn't terrain and isn't an enemy targetable, we pass through
        return false;
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
        transform.position = originPos;
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
        this.enabled = true; 
        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false;
        
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
        sprite.gameObject.SetActive(false);
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
        
        this.enabled = false; 
    }
}
