using Unity.Netcode;
using UnityEngine;
using ProjectJS.Manager;
using Unity.Collections;

namespace ProjectJS.Controller
{
	/// <summary>
	/// 기본적으로 Player를 따라다니는 CameraController입니다.
	/// 
	/// Intro, Outro 등 특정 이벤트가 있는 경우를 대비해서,
	/// Player와 별개의 오브젝트로 움직이도록 하는게 나을 것 같아 따로 만들어 두었습니다.
	/// </summary>
    public class CamerController : MonoBehaviour
    {
		[Header("Camera Settings")]
		[SerializeField] float followSpeed = 10f;

		[Header("DEBUG")]
        [SerializeField, ReadOnly] private Transform targetPlayer;

		private Transform targetTransform = null;

		private void Awake()
		{
			targetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
			targetTransform = targetPlayer;
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
			if (targetTransform == null)
			{
				if (targetPlayer == null)
					targetPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

				targetTransform = targetPlayer;
			}
			else
			{
				Vector3 targetPos = targetTransform.position;

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
					// Find Current Boss ( with tag?.. )
					break;
				case GameEventType.Camera_ToPlayer:
					targetTransform = targetPlayer;
					break;
			}
		}


	}
}