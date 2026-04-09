using ProjectJS.Animation;
using ProjectJS.Manager;
using System.Collections;
using UnityEngine;
using ProjectJS.ScriptableObjects;
using ProjectJS.Entities;
using ProjectJS.Utils;

namespace ProjectJS.Controller
{
	/// <summary>
	/// Test 및 시연용. 수정 필요
	/// 
	/// - Roaming -> 물리 연산으로 처리 필요
	/// </summary>
	public class TestBossController : BossController
	{
		[Header("Move")]
		[SerializeField] private float moveDistance = 1f;
		[SerializeField] private float moveSpeed = .5f;

		[Header("Sockets")]
		[SerializeField] private Transform pattern1Socket;

		private enum State { Init, Roam, Detect, Pattern }
		private StateMachine<State> stateMachine;

		protected Animator animator;
		protected BossAttackAnimNotifier[] attackAnimNotifiers;
		protected bool isAttackAnimPlaying = false;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			attackAnimNotifiers = animator.GetBehaviours<BossAttackAnimNotifier>();
			foreach (var notifiers in attackAnimNotifiers)
			{
				notifiers.OnStartAction += OnAttackStart;
				notifiers.OnEndAction += OnAttackEnd;
			}

			stateMachine = new(this);
			stateMachine.AddState(State.Init, OnStartInit);
			stateMachine.AddState(State.Roam, OnStartRoam, OnEndRoam);
			stateMachine.AddState(State.Detect, OnStartDetect);
			stateMachine.AddState(State.Pattern, OnStartPattern, OnEndPattern);
			stateMachine.ChangeState(State.Init);
		}

		private IEnumerator OnStartInit()
		{
			yield return null;
			stateMachine.ChangeState(State.Roam);
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

		private IEnumerator OnStartDetect()
		{
			// TODO - 플레이어 중 타겟 지정 필요
			yield return null;
			stateMachine.ChangeState(State.Pattern);
		}

		private IEnumerator OnStartPattern()
		{
			yield return Pattern1();
			stateMachine.ChangeState(State.Roam);
		}

		private IEnumerator OnEndPattern()
		{
			yield return new WaitForSeconds(1.0f);
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

		// HACK - 테스트용 코루틴입니다.
		//	실사용시 IBossPattern 인터페이스 구현하여 BossAttack 스크립트를 통해 실행되어야 합니다.
		private IEnumerator Pattern1()
		{
			animator.SetTrigger("isAttack");
			yield return null;
			yield return new WaitUntil(() => !isAttackAnimPlaying);
		}

		protected virtual void OnAttackStart()
		{
			isAttackAnimPlaying = true;
		}
		protected virtual void OnAttackEnd()
		{
			isAttackAnimPlaying = false;
		}

		// Animation Event
		private void OnAttack()
		{
			Managers.Pool.Get(PoolingType.Projectile_Bullet)
				.GetComponent<Projectile>()
				.Init(0, pattern1Socket.position.ToVector2(), new Vector2(transform.localScale.x > 0 ? 1 : -1, 0));
		}
	}
}
