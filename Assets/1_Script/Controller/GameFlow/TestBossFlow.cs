using ProjectJS.Manager;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;

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

		private enum State { Init, Intro, Combat, Outro, Exit, GameOver }
		private StateMachine<State> stateMachine;

		protected void Awake()
		{
			if (!NetworkManager.Singleton.IsHost) return;

			stateMachine = new(this);
			stateMachine.AddState(State.Init, OnStartInit);
			stateMachine.AddState(State.Intro, OnStartIntro, OnEndIntro);
			stateMachine.AddState(State.Combat, OnStartCombat, OnEndCombat);
			stateMachine.AddState(State.Outro, OnStartOutro);
			stateMachine.AddState(State.Exit, OnStartExit);
			stateMachine.AddState(State.GameOver, OnStartGameOver);
		}

		public void Init()
		{
			stateMachine.ChangeState(State.Init);
		}

		private IEnumerator OnStartInit()
		{
			// TODO - Lock players' input
			if (ProjectJS.UI.GameScene.GameSceneUI.Instance != null)
				ProjectJS.UI.GameScene.GameSceneUI.Instance.ShowGameOverUI(false);

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
			//ProjectJS.UI.GameScene.GameSceneUI.Instance.RegisterBoss(bossController);
			// Unlock players' input

			yield return StartCoroutine(bossController.OnStartCombat());
			
			// Wait until boss is dead or all players are dead
			while (bossController.CurrentPhase != BossPhaseType.None)
			{
				if (AreAllPlayersDead())
				{
					stateMachine.ChangeState(State.GameOver);
					yield break;
				}
				yield return null;
			}

			stateMachine.ChangeState(State.Outro);
		}

		private bool AreAllPlayersDead()
		{
			var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
			if (players.Length == 0) return false;

			foreach (var player in players)
			{
				if (!player.IsDead) return false;
			}
			return true;
		}

		private IEnumerator OnStartGameOver()
		{
			Debug.LogWarning("GAME OVER!!!");
			if (ProjectJS.UI.GameScene.GameSceneUI.Instance != null)
				ProjectJS.UI.GameScene.GameSceneUI.Instance.ShowGameOverUI(true);

			while (true)
			{
				if (Input.GetKeyDown(KeyCode.F5))
				{
					RestartBoss();
					yield break;
				}
				if (Input.GetKeyDown(KeyCode.F6))
				{
					ReturnToLobby();
					yield break;
				}
				yield return null;
			}
		}

		private void RestartBoss()
		{
			if (bossController != null)
			{
				bossController.GetComponent<NetworkObject>().Despawn(true);
			}

			var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
			foreach (var player in players)
			{
				player.RequestRetryServerRpc();
			}

			stateMachine.ChangeState(State.Init);
		}

		private void ReturnToLobby()
		{
			if (ProjectJS.UI.GameScene.GameSceneUI.Instance != null)
				ProjectJS.UI.GameScene.GameSceneUI.Instance.ShowGameOverUI(false);
			
			GameNetworkManager.Instance.ReturnToLobbyFromGame();
		}

		private void OnGUI()
		{
			if (stateMachine == null || stateMachine.CurrentState != State.GameOver) return;
			if (ProjectJS.UI.GameScene.GameSceneUI.Instance != null && ProjectJS.UI.GameScene.GameSceneUI.Instance.HasGameOverPanel) return;

			GUI.Box(new Rect(Screen.width / 2 - 125, Screen.height / 2 - 60, 250, 120), "GAME OVER (HOST ONLY)");
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 25), "F5: Restart Boss Fight");
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 25), "F6: Return to Lobby");
			GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 30, 200, 25), "(Assign GameOverPanel in GameSceneUI)");
		}

		private IEnumerator OnEndCombat()
		{
			yield return null;
		}

		private IEnumerator OnStartOutro()
		{
			Debug.LogWarning("OUTRO!!!");
			yield return null;
			// TOOD - UI 띄워야함 
		}

		private IEnumerator OnStartExit()
		{
			bossController.GetComponent<NetworkObject>().Despawn(true);
			yield return null;
		}
	}
}
