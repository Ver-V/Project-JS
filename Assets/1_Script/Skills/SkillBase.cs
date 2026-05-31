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

public Player Caster => caster;
public SkillData BaseSkillData => baseSkillData;
public ShardData EquippedShard => equippedShard;
public Vector2 SkillDirection => skillDirection;

public float FinalDamage => finalDamage;
public float FinalRange => finalRange;
public float FinalCooldown => finalCooldown;
public StatusEffect FinalStatusEffect => finalStatusEffect;

        public virtual void Initialize(Player caster, SkillData data, ShardData shard, Vector2 direction)
        {
            this.caster = caster;
            this.baseSkillData = data;
            this.equippedShard = shard;
            this.skillDirection = direction.normalized;

            CalculateFinalStats();
            PlayEffects();
            if (IsOwner)
            {
                Execute();
            }
        }

        private void CalculateFinalStats()
        {
            float damageMultiplier = (equippedShard != null) ? equippedShard.DamageMultiplier : 1.0f;
            float rangeMultiplier = (equippedShard != null) ? equippedShard.RangeMultiplier : 1.0f;
            float cooldownMultiplier = (equippedShard != null) ? equippedShard.CooldownMultiplier : 1.0f;
            finalDamage = baseSkillData.BaseDamage* damageMultiplier;
            finalRange = baseSkillData.BaseRange* rangeMultiplier;
            finalCooldown = baseSkillData.BaseCooldown * cooldownMultiplier;
            finalStatusEffect = (equippedShard != null) ? equippedShard.GrantedEffect : StatusEffect.None;
        }

        protected void PlayEffects()
        {
            if (BaseSkillData.VfxPrefab != null)
            {
                Instantiate(BaseSkillData.VfxPrefab, transform.position, Quaternion.identity);
            }

            if (BaseSkillData.SfxClip != null)
            {
                AudioSource.PlayClipAtPoint(BaseSkillData.SfxClip, transform.position);
            }
        }
protected abstract void Execute();

        protected void ApplyDamageAndEffect(Collider2D enemyCollider)
        {
            Debug.Log($"[SkillBase] ApplyDamageAndEffect called. IsServer: {IsServer}");
            if (!IsServer) return;

            var boss = enemyCollider.GetComponentInParent<ProjectJS.Controller.BossController>();
            if (boss != null)
            {
                Debug.Log($"[SkillBase] BossController found! Requesting {FinalDamage} damage.");
                boss.RequestTakeDamageServerRpc(FinalDamage);
                
                if (TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned)
                {
                    netObj.Despawn();
                }
            }
            else
            {
                Debug.LogWarning("[SkillBase] BossController NOT found on the hit collider or its parents.");
            }
        }
    }     
}