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
        private NetworkVariable<ShardSpecies> equippedShardSpecies = new NetworkVariable<ShardSpecies>(ShardSpecies.None);

        public IReadOnlyList<ShardData> AvailableShards => availableShards;
        public ShardSpecies EquippedShardSpecies => equippedShardSpecies.Value;

        public IReadOnlyList<ShardData> GetAvailableShards()
        {
            return availableShards;
        }

        public ShardData GetEquippedShard()
        {
            return GetShardBySpecies(equippedShardSpecies.Value);
        }

        public ShardData GetShardBySpecies(ShardSpecies species)
        {
            if (availableShards == null)
            {
                return null;
            }

            foreach (ShardData shard in availableShards)
            {
                if (shard != null && shard.Species == species)
                {
                    return shard;
                }
            }

            return null;
        }

        private float lastSkillTime = -100f; // last skill usetime
        private Player player;
        private Animator anim;

        public override void OnNetworkSpawn()
        {
            player = GetComponent<Player>();
            anim = GetComponentInChildren<Animator>();
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

            ShardSpecies equippedSpecies = equippedShardSpecies.Value;
            ShardData equippedShard = GetShardBySpecies(equippedSpecies);

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
                UseSkillServerRpc(direction, equippedSpecies);
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
        private void UseSkillServerRpc(Vector2 direction, ShardSpecies shardSpecies, RpcParams rpcParams = default)
        {
            WeaponData currentWeapon = player.CurrentWeapon;
            if (currentWeapon == null || currentWeapon.WeaponSkill == null) return;
            SkillData currentSkill = currentWeapon.WeaponSkill;

            ShardData equippedShard = GetShardBySpecies(shardSpecies);

            SkillBase skillInstance = Instantiate(currentSkill.SkillLogicPrefab, transform.position, Quaternion.identity);

            NetworkObject networkObj = skillInstance.GetComponent<NetworkObject>();
            if (networkObj != null)
            {
                networkObj.Spawn();
                skillInstance.Initialize(player, currentSkill, equippedShard, direction);

                InitializeSkillClientRpc(networkObj.NetworkObjectId, direction, shardSpecies);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void InitializeSkillClientRpc(ulong skillNetworkObjectId, Vector2 direction, ShardSpecies shardSpecies)
        {
            if (IsServer) return;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(skillNetworkObjectId, out NetworkObject spawnedObj))
            {
                SkillBase skillInstance = spawnedObj.GetComponent<SkillBase>();
                if (skillInstance != null)
                {
                    WeaponData currentWeapon = player.CurrentWeapon;
                    SkillData currentSkill = (currentWeapon != null) ? currentWeapon.WeaponSkill : null;
                    ShardData equippedShard = GetShardBySpecies(shardSpecies);
                    
                    if (currentSkill != null)
                    {
                        skillInstance.Initialize(player, currentSkill, equippedShard, direction);
                    }
                }
            }
        }

        public void EquipShard(ShardSpecies species)
        {
            if (IsOwner) EquipShardServerRpc(species);
        }

        [Rpc(SendTo.Server)]
        private void EquipShardServerRpc(ShardSpecies species)
        {
            if (species != ShardSpecies.None && GetShardBySpecies(species) == null)
            {
                return;
            }

            equippedShardSpecies.Value = species;
        }
    }
}
