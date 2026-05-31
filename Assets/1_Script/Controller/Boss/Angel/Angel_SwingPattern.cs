using ProjectJS.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	/// <summary>
	/// 일반적인 낫 Swing 패턴
	/// 
	/// - 1 페이즈에서 사용 가능
	/// - 낫을 휘두르며, 범위 내의 플레이어에게 피해를 입힘
	/// </summary>
	public class Angel_SwingPattern : BossPatternBase
	{

		private Collider2D[] colliders = new Collider2D[Constants.MAX_PLAYERS + 1];
		private int playerCount = 0;
		private ContactFilter2D filter;

		[Header("Swing Pattern")]
		[SerializeField] private Transform swingPatternSocket = null;
		[SerializeField] private Vector2 swingSize;
		[SerializeField] private float damageMult = 1f;

		private void Awake()
		{
			filter = new ContactFilter2D();
			filter.SetLayerMask(Constants.LAYER_PLAYER);
			filter.useTriggers = true;
		}

		public override IEnumerator DoPattern(BossAttack boss, float attack)
		{
			boss.GetComponent<BossController>().RequestAnimParam("isAttack");
			yield return null;
			yield return new WaitUntil(() => !boss.IsAttackAnimPlaying);
		}
		
		public override bool Predict() => true;

		private void OnSwingSickle()
		{
			playerCount = Physics2D.OverlapBox(swingPatternSocket.position, swingSize, 0f, filter, colliders);
			
			for (int i = 0; i < playerCount; i++)
			{
				if (!colliders[i].GetComponent<NetworkObject>().IsOwner) return;
				colliders[i].GetComponent<Player>()
					.TakeDamage(GetComponent<BossController>().GetAttackPower() * damageMult, transform.position.ToVector2());
			}
		}

		private void OnDrawGizmosSelected()
		{
			return;

			Color backupColor = Gizmos.color;
			Gizmos.color = Color.red;

			Vector2 center = (swingPatternSocket ?? transform).position.ToVector2();
			Vector2 size = swingSize;
			float angle = 0f;

			Matrix4x4 matrix = Matrix4x4.TRS(
				center,
				Quaternion.Euler(0, 0, angle),
				Vector3.one
			);

			Gizmos.matrix = matrix;

			Gizmos.DrawWireCube(Vector3.zero, size);
			Gizmos.color = backupColor;
		}
	}
}