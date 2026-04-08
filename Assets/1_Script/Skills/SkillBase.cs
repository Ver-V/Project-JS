using UnityEngine;
using Unity.Netcode;
using System;

namespace ProjectJS.Skills
{
    public abstract class SkillBase : NetworkBehaviour
    {
        private Player caster;
private SkillData baseSkillData;
private ShardData equippedShard;
private Vector2 skillDirection;

private float finalDamage;
private float finalRange;
private float finalCooldown;
private StatusEffect finalStatusEffect;

public Player Caster { get => caster; set => caster = value; }
public SkillData BaseSkillData { get => baseSkillData; set => baseSkillData = value; }
public ShardData EquippedShard { get => equippedShard; set => equippedShard = value; }
public Vector2 SkillDirection { get => skillDirection; set => skillDirection = value; }

public float FinalDamage { get => finalDamage; set => finalDamage = value; }
public float FinalRange { get => finalRange; set => finalRange = value; }
public float FinalCooldown { get => finalCooldown; set => finalCooldown = value; }
public StatusEffect FinalStatusEffect { get => finalStatusEffect; set => finalStatusEffect = value; }

        public virtual void Initialize(Player caster, SkillData data, ShardData shard, Vector2 direction)
        {
            Caster = caster;
            BaseSkillData = data;
            EquippedShard = shard;
            SkillDirection = direction.normalized;

            CalculateFinalStats();
            PlayEffects();
            if (IsOwner)
            {
                Execute();
            }
        }

        private void CalculateFinalStats()
        {
            float damageMultiplier = (EquippedShard != null) ? EquippedShard.DamageMultiplier : 1.0f;
            float rangeMultiplier = (EquippedShard != null) ? EquippedShard.RangeMultiplier : 1.0f;
            float cooldownMultiplier = (EquippedShard != null) ? EquippedShard.CooldownMultiplier : 1.0f;
            FinalDamage = BaseSkillData.BaseDamage* damageMultiplier;
            FinalRange = BaseSkillData.BaseRange* rangeMultiplier;
            FinalStatusEffect = (EquippedShard != null) ? EquippedShard.GrantedEffect : StatusEffect.None;
        }

protected void PlayEffects()
    {
            
        if (BaseSkillData.VfxPrefab != null)
            {
                // TODO: Run VFX
            }

        if (BaseSkillData.SfxClip != null)
            {
                // TODO: Run SFXs
            } 
    }
protected abstract void Execute();

protected void ApplyDamageAndEffect(Collider2D enemyCollider)
    {
        if (!IsOwner) return;

         /* TODO: fix the script later 
          var enemy = enemyCollider.GetComponent<Enemy>();
          if (enemy != null) {
              enemy.TakeDamage(FinalDamage);
              if (FinalStatusEffect != StatusEffect.None) {
                  enemy.ApplyStatusEffect(FinalStatusEffect);
              } 
          } */

        }
    }     
}