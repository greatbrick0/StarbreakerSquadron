using UnityEngine;
using Unity.Mathematics;

public class ExplosionAttack : Attack
{
    [SerializeField]
    private float visualRemainTime = 2.0f;
    [SerializeField]
    private Vector2 soundBounds = Vector2.one * 14;
    [SerializeField]
    private AudioSource audioPlayer;

    protected override void Awake()
    {
        
        if (audioPlayer != null)
        {
            Camera cam = Camera.main;
            float dist = VecUtils.ModifiedDistance(transform.position, Camera.main.transform.position, 4);
            float volume = Mathf.Clamp01(math.remap(soundBounds.x, soundBounds.y, 1, 0, dist));
            audioPlayer.volume = volume;
            audioPlayer.Play();
        }
            
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
        return; // override to do nothing 
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
