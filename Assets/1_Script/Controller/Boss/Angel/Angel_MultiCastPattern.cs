using ProjectJS.Entities;
using ProjectJS.Manager;
using ProjectJS.ScriptableObjects;
using ProjectJS.Utils;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class Angel_MultiCastPattern : BossPatternBase
	{
		[SerializeField] private Transform castSocket;

		public override IEnumerator DoPattern(BossAttack boss, float attack)
		{
			boss.GetComponent<BossController>().RequestAnimParam("isAttack3", true);
			for (int i = 0; i < 8; i++)
			{
				yield return new WaitForSeconds(.3f);
				MultiCast();
			}
			boss.GetComponent<BossController>().RequestAnimParam("isAttack3", false);
		}

		public override bool Predict() => true;

		private float angleOffset = 0f;
		private void MultiCast()
		{
			if (!NetworkManager.Singleton.IsHost) return;

			BossController bossController = GetComponent<BossController>();
			for (int i = 0; i < 8; i++)
			{
				float theta = (angleOffset * 22.5f + i * 45f )* Mathf.Deg2Rad;
				Managers.Pool.Get(PoolingType.Projectile_Angel_Spiral)
					.GetComponent<Projectile>()
					.Init(bossController.GetNewProjectileID(), castSocket.position.ToVector2(), new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)));
			}
			angleOffset++;
		}
	}
}