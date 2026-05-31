using ProjectJS.Entities;
using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;
using ProjectJS.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class Angel_CastPattern : BossPatternBase
	{
		[SerializeField] private Transform castSocket;

		public override IEnumerator DoPattern(BossAttack boss, float attack)
		{
			boss.GetComponent<BossController>().RequestAnimParam("isAttack2");
			yield return null;
			yield return new WaitUntil(() => !boss.IsAttackAnimPlaying);
		}

		public override bool Predict() => true;

		private void OnCast()
		{
			BossController bossController = GetComponent<BossController>();
			for (int i = 0; i < 8; i++)
			{
				float theta = i * 45f * Mathf.Deg2Rad;
				Managers.Pool.Get(PoolingType.Projectile_Angel_Spiral)
					.GetComponent<Projectile>()
					.Init(bossController.GetNewProjectileID(), castSocket.position.ToVector2(), new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)));
			}
		}

	}
}