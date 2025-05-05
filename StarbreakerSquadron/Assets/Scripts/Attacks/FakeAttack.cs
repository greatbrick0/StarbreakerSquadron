using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class FakeAttack : MonoBehaviour
{
    private bool used = false;

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
    protected float lifetime;
    protected float age;
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
        if (!used) return;

        rb.linearVelocity = speed * direction;

        age += Time.deltaTime;
        if (age >= lifetime)
        {
            ResetToHidden();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
        ResetToHidden();
    }

    protected virtual void HitTargetable(Targetable targetable)
    {
        if(targetable.team != team)
        {
            //targetable.TakeDamage(primaryPower);
            ResetToHidden();
        }
    }

    public void SetValuesFake(AttackInfo newAttackInfo)
    {
        GetComponent<Collider2D>().enabled = true;
        sprite.gameObject.SetActive(true);
        used = true;

        team = newAttackInfo.team;
        ColorUtility.TryParseHtmlString(newAttackInfo.colour, out Color parsedColour);
        sprite.color = parsedColour;
        sprite.color = Color.red;
        primaryPower = 0;
        secondaryPower = 0;
        lifetime = newAttackInfo.lifetime;
        age = 0;
        speed = newAttackInfo.speed;
        direction = newAttackInfo.direction;
        originPos = newAttackInfo.originPos;
    }

    private void ResetToHidden()
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

        Destroy(gameObject);
    }
}
