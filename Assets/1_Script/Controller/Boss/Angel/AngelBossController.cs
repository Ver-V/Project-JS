using ProjectJS.Manager;
using System.Collections;
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

		private enum State { Init, Detect, Roam, Pattern, Dead }
		private StateMachine<State> stateMachine;

		protected override void OnAwake()
		{
			base.OnAwake();

			var bossStat = Managers.Resource.GetBossStat();
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
			stateMachine.ChangeState(State.Init);
		}

		protected override void OnDamaged()
		{

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
			yield return null;
			//stateMachine.ChangeState(State.Roam);
			stateMachine.ChangeState(State.Pattern);
		}

		private IEnumerator OnStartRoam()
		{
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
			yield return bossAttack.GetRandomPattern();
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

		private IEnumerator Move()
		{
			Vector2 startPos = transform.position;
			Vector2 randomDir = Random.insideUnitCircle.normalized;
			Vector2 targetPos = startPos + randomDir * moveDistance;

			transform.localScale = new Vector3(randomDir.x < 0 ? -1f : 1f, 1f, 1f);

			float totalDistance = Vector2.Distance(startPos, targetPos);
			float moved = 0f;

			while (moved < totalDistance)
			{
				float step = moveSpeed * Time.deltaTime;
				moved += step;

				transform.position = Vector2.MoveTowards(transform.position, targetPos, step);

				yield return null;
			}

			transform.position = targetPos;
		}

		public override float GetAttackPower()
		{
			return statContainer.Get<AttackStat>().CurrentAttack.Value;
		}
	}
}
