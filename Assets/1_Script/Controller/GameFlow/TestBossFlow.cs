using ProjectJS.Manager;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	/// <summary>
	/// Test용 보스 Flow Controller
	/// 
	/// 보스방 입장, 게임 플레이, 실패 시 UI 및 처리 등..
	/// 게임 한 판의 전체적인 Flow를 관리합니다.
	/// </summary>
	public class TestBossFlow : MonoBehaviour
	{
		[SerializeField] private GameObject bossPrefab;

		private BossController bossController;

		private enum State { Init, Intro, Combat, PhaseTransition, Outro, Exit }
		private StateMachine<State> stateMachine;

		protected void Awake()
		{
			if (!NetworkManager.Singleton.IsHost) return;

			stateMachine = new(this);
			stateMachine.AddState(State.Init, OnStartInit);
			stateMachine.AddState(State.Intro, OnStartIntro, OnEndIntro);
			stateMachine.AddState(State.Combat, OnStartCombat, OnEndCombat);
			stateMachine.AddState(State.PhaseTransition, OnStartPhaseTransition);
			stateMachine.AddState(State.Outro, OnStartOutro);
			stateMachine.AddState(State.Exit, OnStartExit);
		}

		public void Init()
		{
			stateMachine.ChangeState(State.Init);
		}

		private IEnumerator OnStartInit()
		{
			// TODO - Lock players' input

			bossController = Instantiate(bossPrefab, /*HACK*/ Vector3.up * 5f, Quaternion.identity, null)
				.GetComponent<BossController>();
			bossController.GetComponent<NetworkObject>().Spawn();

			yield return new WaitUntil(() => bossController.spawnedCount.Value >= NetworkManager.Singleton.ConnectedClients.Count);
			stateMachine.ChangeState(State.Intro);
		}

		private IEnumerator OnStartIntro()
		{
			bool isDone = false;
			NetworkTransmission.instance.StartEventSync(() => { isDone = true; }, GameEventType.Camera_ToBoss);
			yield return new WaitUntil(() => isDone);

			yield return StartCoroutine(bossController.OnStartIntro());
			stateMachine.ChangeState(State.Combat);
		}
		
		private IEnumerator OnEndIntro()
		{
			bool isDone = false;
			NetworkTransmission.instance.StartEventSync(() => { isDone = true; }, GameEventType.Camera_ToPlayer);
			yield return new WaitUntil(() => isDone);

			yield return StartCoroutine(bossController.OnEndIntro());
		}

		private IEnumerator OnStartCombat()
		{
			// Game Start UI (async)
			// Unlock players' input

			yield return StartCoroutine(bossController.OnStartCombat());
			yield return null;
		}

		private IEnumerator OnEndCombat()
		{
			yield return null;
		}

		private IEnumerator OnStartPhaseTransition()
		{
			yield return null;
		}

		private IEnumerator OnStartOutro()
		{
			yield return null;
		}

		private IEnumerator OnStartExit()
		{
			bossController.GetComponent<NetworkObject>().Despawn(true);
			yield return null;
		}
	}
}
