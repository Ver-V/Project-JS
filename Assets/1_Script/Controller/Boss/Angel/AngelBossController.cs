using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectJS.Controller
{
	/// <summary>
	/// Test 및 시연용. 수정 필요
	/// 
	/// - Roaming -> 물리 연산으로 처리 필요
	/// </summary>
	public class AngelBossController : BossController
	{
		[Header("Move")]
		[SerializeField] private float moveDistance = 1f;
		[SerializeField] private float moveSpeed = .5f;
		[SerializeField] private float roamDuration = 1f;

		private Rigidbody2D rigidbody;

        private enum State { Init, Detect, Roam, Pattern, Dead, Phasing }
		private BossPhaseType currentPhase = BossPhaseType.Phase1;
		private StateMachine<State> stateMachine;
		private BossStat bossStat = null;

		protected override void OnUpdate()
		{
			// TEST
			if (Input.GetKeyDown(KeyCode.R))
			{
				currentHP.Value -= bossStat.MaxHP * .7f;
			}
		}

		protected override void OnAwake()
		{
			base.OnAwake();

			rigidbody = GetComponent<Rigidbody2D>();

            bossStat = Managers.Resource.GetBossStat();
			statContainer.AddStat(new HealthStat(bossStat.MaxHP));
			statContainer.AddStat(new AttackStat(DamageType.Physics, bossStat.AttackPower));
			statContainer.AddStat(new DefenseStat(bossStat.DefensePower));
			statContainer.AddStat(new MovementStat(bossStat.MoveSpeed));

			stateMachine = new(this);
			stateMachine.AddState(State.Init, OnStartInit);
			stateMachine.AddState(State.Roam, OnStartRoam, OnEndRoam);
			stateMachine.AddState(State.Detect, OnStartDetect);
			stateMachine.AddState(State.Pattern, OnStartPattern, OnEndPattern);
			stateMachine.AddState(State.Dead, OnStartDead, OnEndDead);
			stateMachine.AddState(State.Phasing, OnStartPhasing, OnEndPhasing);
			stateMachine.ChangeState(State.Init);
		}

		protected override void OnDamaged()
		{
			RequestResetFlashTimeServerRPC();
		}

		protected override void OnDead()
		{
			stateMachine.ChangeState(State.Dead);
		}

		public override IEnumerator OnStartIntro()
		{
			RequestAnimParam("isIntro", true);
			yield return new WaitForSeconds(3f);
		}

		public override IEnumerator OnEndIntro()
		{
			RequestAnimParam("isIntro", false);
			yield return null;
		}

		public override IEnumerator OnStartCombat()
		{
			stateMachine.ChangeState(State.Detect);
			yield return null;
		}

		private IEnumerator OnStartInit()
		{
			yield return null;
		}

		private IEnumerator OnStartDetect()
		{
			// ex. 근거리에 있으면 근거리 패턴
			// 원거리에 있으면 그 방향으로 Roam or 원거리 패턴

			if (currentPhase == BossPhaseType.Phase1 && currentHP.Value <= bossStat.MaxHP * .5f)
			{
				currentPhase = BossPhaseType.Phase2;
				stateMachine.ChangeState(State.Phasing);
				yield break;
			}

			yield return null;
			//stateMachine.ChangeState(State.Roam);
			stateMachine.ChangeState(State.Pattern);
		}

		private IEnumerator OnStartRoam()
		{
			if (currentPhase == BossPhaseType.Phase1 && currentHP.Value <= bossStat.MaxHP * .5f)
			{
				currentPhase = BossPhaseType.Phase2;
				stateMachine.ChangeState(State.Phasing);
				yield break;
			}

			animator.SetBool("isWalk", true);
			yield return Move();
			stateMachine.ChangeState(State.Detect);
		}

		private IEnumerator OnEndRoam()
		{
			animator.SetBool("isWalk", false);
			yield return null;
		}

		private IEnumerator OnStartPattern()
		{
			yield return bossAttack.GetRandomPattern(currentPhase);
			stateMachine.ChangeState(State.Roam);
		}

		private IEnumerator OnEndPattern()
		{
			yield return new WaitForSeconds(.5f);
		}

		private IEnumerator OnStartDead()
		{
			yield return null;
		}

		private IEnumerator OnEndDead()
		{
			yield return null;
		}

		private IEnumerator OnStartPhasing()
		{
			bool isDone = false;
			NetworkTransmission.instance.StartEventSync(() => { isDone = true; }, GameEventType.Camera_ToBoss);
			yield return new WaitUntil(() => isDone);

			yield return StartCoroutine(OnStartIntro());
			stateMachine.ChangeState(State.Detect);
		}

		private IEnumerator OnEndPhasing()
		{
			bool isDone = false;
			NetworkTransmission.instance.StartEventSync(() => { isDone = true; }, GameEventType.Camera_ToPlayer);
			yield return new WaitUntil(() => isDone);

			yield return StartCoroutine(OnEndIntro());
		}

		private IEnumerator Move()
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            transform.localScale = new Vector3(
                randomDir.x < 0 ? -1f : 1f,
                1f,
                1f
            );

            rigidbody.linearVelocity = randomDir * moveSpeed;

            float elapsed = 0f;
            while (elapsed < roamDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            rigidbody.linearVelocity = Vector2.zero;
        }

        public override float GetAttackPower()
		{
			return statContainer.Get<AttackStat>().CurrentAttack.Value;
		}
	}
}
