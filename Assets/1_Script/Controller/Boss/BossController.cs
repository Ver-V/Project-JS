using ProjectJS.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public abstract partial class BossController : NetworkBehaviour
	{
		protected Animator animator;
		protected BossAttack bossAttack;

		protected Material spriteMaterial;
		protected StatContainer statContainer;

		private int bossID = 1;
		private int projectileIdx = 0;
		public int GetNewProjectileID() => IdUtil.GetProjectileID(bossID, projectileIdx++);

		private void Awake()
		{
			spriteMaterial = GetComponent<SpriteRenderer>().material;

			if (!NetworkManager.IsHost) return;
			OnAwake();
		}

		private void Start()
		{
			if (!NetworkManager.IsHost) return;
			OnStart();
		}

		protected void Update()
		{
			if (!NetworkManager.IsHost) return;

			if (remainedFlashTime > 0f)
				remainedFlashTime -= Time.deltaTime;

			isFlashing.Value = remainedFlashTime > 0f;

			if (Input.GetKeyDown(KeyCode.R))
			{
				RequestResetFlashTimeServerRPC();
			}
		}

		protected override void OnNetworkPostSpawn()
		{
			base.OnNetworkPostSpawn();

			IncreaseSpawnCountServerRPC();

			isFlashing.OnValueChanged += ((prev, cur) => {
				if (prev != cur)
					spriteMaterial.SetFloat("_EdgeStrength", cur ? 2f : 0f);
				});
		}

		public abstract float GetAttackPower();

		public abstract IEnumerator OnStartIntro();
		public abstract IEnumerator OnEndIntro();
		public abstract IEnumerator OnStartCombat();

		protected abstract void OnDamaged();
		protected abstract void OnDead();

		protected virtual void OnAwake()
		{
			animator = GetComponent<Animator>();
			bossAttack = GetComponent<BossAttack>();

			statContainer = new();

			// TODO - HP/UI 연결
			// currentHP.OnValueChanged((value) => { UIManager.SetHPBar(value); });
		}
		protected virtual void OnStart()
		{
			if (!statContainer.TryGet(out HealthStat healthStat))
			{
				Debug.LogError("No Health Stat");
				return;
			}

			healthStat.OnCurrentHPChanged += ((value) => { currentHP.Value = value; });
			healthStat.OnCurrentHPChanged.Invoke(healthStat.CurrentHP);
		}

		public void RequestAnimParam(string param, bool isOn)
		{
			animator.SetBool(param, isOn);
		}
		public void RequestAnimParam(string param)
		{
			animator.SetTrigger(param);
		}
	}
}