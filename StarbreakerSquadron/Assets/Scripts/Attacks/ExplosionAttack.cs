using UnityEngine;
using Unity.Mathematics;

using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class ExplosionAttack : Attack
{
    [SerializeField]
    private float visualRemainTime = 2.0f;
    [SerializeField]
    private Vector2 soundBounds = Vector2.one * 14;
    [SerializeField, Range(0f, 1f)]
    private float volumeMult = 0.7f;
    [SerializeField]
    private AudioSource audioPlayer;

    private HashSet<Targetable> hitTargets = new HashSet<Targetable>();

    protected override void Update()
    {
        if (!used) return;

        age += Time.deltaTime;
        if (IsServer)
        {
            if (age < lifetime)
            {
                CheckExplosionCollisions();
            }

            if(age >= visualRemainTime)
            {
                ResetToHiddenRpc();
            }
        }
        else
        {
            sprite.color = sprite.color.ChangeAlpha(1 - (age / visualRemainTime));
        }
    }

    private void CheckExplosionCollisions()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeSize, collisionMask);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Targetable targetable))
            {
                if (!hitTargets.Contains(targetable) && targetable.team != team)
                {
                    targetable.TakeDamage(primaryPower);
                    hitTargets.Add(targetable);
                }
            }
        }
    }

    protected override void ValueInitialize()
    {
        base.ValueInitialize();
        hitTargets.Clear();
        sprite.transform.localScale = Vector2.one * aoeSize;
        TryToPlaySound();
        
        if (IsServer) CheckExplosionCollisions();
    }

    private void TryToPlaySound()
    {
        if (audioPlayer == null) return;

        float dist = VecUtils.ModifiedDistance(transform.position, Camera.main.transform.position, 4);
        float volume = Mathf.Clamp01(math.remap(soundBounds.x, soundBounds.y, 1, 0, dist));
        audioPlayer.volume = volume * volumeMult;
        audioPlayer.Play();
    }
}
