using ProjectJS.Manager;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	/// <summary>
	/// 기본적으로 Player를 따라다니는 CameraController입니다.
	/// 
	/// Intro, Outro 등 특정 이벤트가 있는 경우를 대비해서,
	/// Player와 별개의 오브젝트로 움직이도록 하는게 나을 것 같아 따로 만들어 두었습니다.
	/// </summary>
	public class CameraController : MonoBehaviour
    {
		[Header("Camera Settings")]
		[SerializeField] float followSpeed = 10f;

		[Header("DEBUG")]
        [SerializeField, ReadOnly] private Transform targetPlayer;

		private Transform targetTransform = null;
		private bool isFollowPlayer = true;

		private void Awake()
		{
			// targetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
			// targetTransform = targetPlayer;
		}

		private void OnEnable()
		{
			NetworkTransmission.instance.OnGameEvent += OnGameEvent;
		}

		private void OnDisable()
		{
			NetworkTransmission.instance.OnGameEvent -= OnGameEvent;
		}

		private void Update()
		{
			if (!isFollowPlayer) return;

			if (targetTransform == null)
			{
				if (targetPlayer == null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
					targetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

				targetTransform = targetPlayer;
			}
			else
			{
				Vector3 targetPos = targetTransform.position;
				targetPos.z = -10f;

				transform.position = Vector3.Lerp(
					transform.position,
					targetPos,
					followSpeed * Time.deltaTime
				);
			}
		}

		private void OnGameEvent(GameEventType eventType, int eventId)
		{
			switch (eventType)
			{
				case GameEventType.Camera_ToBoss:
					isFollowPlayer = false;
					StartCoroutine(MoveCameraToBoss(eventId));
					break;
				case GameEventType.Camera_ToPlayer:
					isFollowPlayer = true;
					NetworkTransmission.instance.ReportEventFinishServerRPC(eventId,
						NetworkManager.Singleton.LocalClientId);
					break;
			}
		}

		private IEnumerator MoveCameraToBoss(int eventId)
		{
			Vector3 startPos = transform.position;
			BossController bossController = FindAnyObjectByType<BossController>();

			if (bossController == null)
			{
				Debug.LogError("Boss doesn't exists!");
				yield break;
			}

			Vector3 targetPos = bossController.transform.position;
			targetPos.z = -10f;

			float duration = 3f;
			float elapsedTime = 0f;

			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				float t = elapsedTime / duration;

				transform.position = Vector3.Lerp(startPos, targetPos, t);

				yield return null;
			}

			transform.position = targetPos;

			NetworkTransmission.instance.ReportEventFinishServerRPC(
				eventId,
				NetworkManager.Singleton.LocalClientId
			);
		}

	}
}