using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using ProjectJS.PStats;

namespace ProjectJS.Skills
{
    public class PlayerSkillManager : NetworkBehaviour
    {
        [Header("Available Data")]
        [SerializeField] private List<ShardData> availableShards = new List<ShardData>();
        private NetworkVariable<int> equippedShardIndex = new NetworkVariable<int>(-1);

        private float lastSkillTime = -100f; // last skill usetime
        private Player player;
        private Animator anim;

        public override void OnNetworkSpawn()
        {
            player = GetComponent<Player>();
            anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            // Input is handled via PlayerController
        }
        
        public void TrySkill()
        {
            Debug.Log("[PlayerSkillManager] TrySkill called!");
            WeaponData currentWeapon = player.CurrentWeapon;
            if (currentWeapon == null)
            {
                Debug.LogWarning("[PlayerSkillManager] CurrentWeapon is null!");
                return;
            }
            if (currentWeapon.WeaponSkill == null)
            {
                Debug.LogWarning($"[PlayerSkillManager] {currentWeapon.WeaponName} has no skill assigned!");
                return;
            }

            SkillData currentSkill = currentWeapon.WeaponSkill;

            int shIdx = equippedShardIndex.Value;
            ShardData equippedShard = (availableShards != null && shIdx >= 0 && shIdx < availableShards.Count) ? availableShards[shIdx] : null;

            if (currentSkill.SkillLogicPrefab == null)
            {
                Debug.LogError($"[PlayerSkillManager] {currentSkill.name} is missing SkillLogicPrefab!");
                return;
            }

            float cooldownMultiplier = (equippedShard != null) ? equippedShard.CooldownMultiplier : 1.0f;
            float finalCooldown = currentSkill.BaseCooldown * cooldownMultiplier;

            if (Time.time >= lastSkillTime + finalCooldown)
            {
                Debug.Log($"[PlayerSkillManager] Triggering Skill: {currentSkill.name}");
                if (anim != null)
                {
                    anim.SetTrigger("Skill");
                }
                
                // use facing direction instead of mouse
                Vector2 direction = player.FacingDirection;

                // cooldown update
                lastSkillTime = Time.time;
                
                PlayLocalSkillEffects(currentSkill, direction);

                // using skill syscall
                UseSkillServerRpc(direction, shIdx);
            }
            else
            {
                Debug.Log($"[PlayerSkillManager] Skill on cooldown. Remaining: {(lastSkillTime + finalCooldown) - Time.time}s");
            }
        }

        private void PlayLocalSkillEffects(SkillData skillData, Vector2 direction)
        {
            if (skillData.VfxPrefab != null)
            {
                // TODO: Run VFX locally
                Instantiate(skillData.VfxPrefab, transform.position, Quaternion.identity);
            }

            if (skillData.SfxClip != null)
            {
                // TODO: Run SFXs locally
                AudioSource.PlayClipAtPoint(skillData.SfxClip, transform.position);
            }
        }

        // NetworkObject with Skillbase
        [Rpc(SendTo.Server)]
        private void UseSkillServerRpc(Vector2 direction, int shardIdx, RpcParams rpcParams = default)
        {
            WeaponData currentWeapon = player.CurrentWeapon;
            if (currentWeapon == null || currentWeapon.WeaponSkill == null) return;
            SkillData currentSkill = currentWeapon.WeaponSkill;

            ShardData equippedShard = (availableShards != null && shardIdx >= 0 && shardIdx < availableShards.Count) ? availableShards[shardIdx] : null;

            SkillBase skillInstance = Instantiate(currentSkill.SkillLogicPrefab, transform.position, Quaternion.identity);

            NetworkObject networkObj = skillInstance.GetComponent<NetworkObject>();
            if (networkObj != null)
            {
                networkObj.Spawn();
                skillInstance.Initialize(player, currentSkill, equippedShard, direction);

                InitializeSkillClientRpc(networkObj.NetworkObjectId, direction, shardIdx);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void InitializeSkillClientRpc(ulong skillNetworkObjectId, Vector2 direction, int shardIdx)
        {
            if (IsServer) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(skillNetworkObjectId, out NetworkObject spawnedObj))
            {
                SkillBase skillInstance = spawnedObj.GetComponent<SkillBase>();
                if (skillInstance != null)
                {
                    WeaponData currentWeapon = player.CurrentWeapon;
                    SkillData currentSkill = (currentWeapon != null) ? currentWeapon.WeaponSkill : null;
                    ShardData equippedShard = (availableShards != null && shardIdx >= 0 && shardIdx < availableShards.Count) ? availableShards[shardIdx] : null;
                    
                    if (currentSkill != null)
                    {
                        skillInstance.Initialize(player, currentSkill, equippedShard, direction);
                    }
                }
            }
        }

        public void EquipShard(int index)
        {
            if (IsOwner) EquipShardServerRpc(index);
        }

        [Rpc(SendTo.Server)]
        private void EquipShardServerRpc(int index)
        {
            equippedShardIndex.Value = index;
        }
    }
}