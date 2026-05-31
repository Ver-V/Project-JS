using ProjectJS.UI.GameScene;
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

        public event System.Action<float, float, float> OnHealthChangedEvent;

        private void Awake()
        {
        	spriteMaterial = GetComponent<SpriteRenderer>().material;
        	gameObject.layer = 13;

        	if (!NetworkManager.Singleton.IsHost) return;
        	OnAwake();
        }

        private void Start()
        {
        	if (!NetworkManager.Singleton.IsHost) return;
        	OnStart();
        }

        protected void Update()
        {
        	if (!NetworkManager.Singleton.IsHost) return;

        	if (remainedFlashTime > 0f)
        		remainedFlashTime -= Time.deltaTime;

			isFlashing.Value = remainedFlashTime > 0f;

			OnUpdate();
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
			currentHP.OnValueChanged += OnCurHealthChanged;
        }
		protected virtual void OnStart()
		{
			if (!statContainer.TryGet(out HealthStat healthStat))
			{
				Debug.LogError("No Health Stat");
				return;
			}

            maxHP.Value = healthStat.MaxHP;

            healthStat.OnCurrentHPChanged += ((value) => { currentHP.Value = value; });
			healthStat.OnCurrentHPChanged.Invoke(healthStat.CurrentHP);
        }
		protected virtual void OnUpdate() { }


		public void RequestAnimParam(string param, bool isOn)
		{
			animator.SetBool(param, isOn);
		}
		public void RequestAnimParam(string param)
		{
			animator.SetTrigger(param);
		}

        private void OnCurHealthChanged(float previousValue, float newValue)
        {
            OnHealthChangedEvent?.Invoke(previousValue, newValue, maxHP.Value);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            currentHP.OnValueChanged += OnCurHealthChanged;

            if (GameSceneUI.Instance != null)
                GameSceneUI.Instance.RegisterBoss(this);

            OnCurHealthChanged(currentHP.Value, currentHP.Value);
        }

        public override void OnNetworkDespawn()
        {
            currentHP.OnValueChanged -= OnCurHealthChanged;

            if (GameSceneUI.Instance != null)
                GameSceneUI.Instance.UnregisterBoss(this);

            base.OnNetworkDespawn();
        }

    }
}