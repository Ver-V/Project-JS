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

        public override void OnNetworkSpawn()
        {
            player = GetComponent<Player>();
        }

        void Update()
        {
            if (!IsOwner) return;
            // Temp Keycode
            if (Input.GetKeyDown(KeyCode.E))
            {
                TrySkill();
            }
        }
        
        public void TrySkill()
        {
            WeaponData currentWeapon = player.CurrentWeapon;
            if (currentWeapon == null || currentWeapon.WeaponSkill == null) return;
            SkillData currentSkill = currentWeapon.WeaponSkill;

            int shIdx = equippedShardIndex.Value;
            ShardData equippedShard = (availableShards != null && shIdx >= 0 && shIdx < availableShards.Count) ? availableShards[shIdx] : null;

            if (currentSkill.SkillLogicPrefab == null) return;

            float cooldownMultiplier = (equippedShard != null) ? equippedShard.CooldownMultiplier : 1.0f;
            float finalCooldown = currentSkill.BaseCooldown * cooldownMultiplier;

            if (Time.time >= lastSkillTime + finalCooldown)
            {
                // calculate mouse position
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (mouseWorldPos - transform.position).normalized;

                // cooldown update
                lastSkillTime = Time.time;
                
                PlayLocalSkillEffects(currentSkill, direction);

                // using skill syscall
                UseSkillServerRpc(direction, shIdx);
            }
        }

        private void PlayLocalSkillEffects(SkillData skillData, Vector2 direction)
        {
            if (skillData.VfxPrefab != null)
            {
                // TODO: Run VFX locally
                // 예: Instantiate(skillData.VfxPrefab, transform.position, Quaternion.identity);
            }

            if (skillData.SfxClip != null)
            {
                // TODO: Run SFXs locally
                // 예: AudioSource.PlayClipAtPoint(skillData.SfxClip, transform.position);
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