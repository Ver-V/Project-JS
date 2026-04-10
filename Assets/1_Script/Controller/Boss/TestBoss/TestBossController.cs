using ProjectJS.Entities;
using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;
using ProjectJS.Utils;
using System.Collections;
using UnityEngine;

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

		protected override void OnAwake()
		{
			base.OnAwake();

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
			yield return bossAttack.GetRandomPattern();
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

		private void OnAttack()
		{
			Managers.Pool.Get(PoolingType.Projectile_Bullet)
				.GetComponent<Projectile>()
				.Init(0, pattern1Socket.position.ToVector2(), new Vector2(transform.localScale.x > 0 ? 1 : -1, 0));
			Managers.Pool.Get(PoolingType.Projectile_Bullet1)
				.GetComponent<Projectile>()
				.Init(0, pattern1Socket.position.ToVector2(), new Vector2(transform.localScale.x > 0 ? 1 : -1, 0));
			Managers.Pool.Get(PoolingType.Projectile_Bullet2)
				.GetComponent<Projectile>()
				.Init(0, pattern1Socket.position.ToVector2(), new Vector2(transform.localScale.x > 0 ? 1 : -1, 0));
		}
	}
}
