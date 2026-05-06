using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public abstract partial class BossController : NetworkBehaviour
	{
		protected Animator animator;
		protected BossAttack bossAttack;

		protected StatContainer statContainer;


		private void Awake()
		{
			if (!NetworkManager.IsHost) return;
			OnAwake();
		}

		private void Start()
		{
			if (!NetworkManager.IsHost) return;
			OnStart();
		}

		protected override void OnNetworkPostSpawn()
		{
			base.OnNetworkPostSpawn();

			IncreaseSpawnCountServerRPC();
		}

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