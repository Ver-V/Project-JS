using System.Collections;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class Angel_CastPattern : BossPatternBase
	{
		public override IEnumerator DoPattern(BossAttack boss, float attack)
		{
			yield return new WaitForSeconds(2.0f);
			// TODO -player.TakeDamage(attack);
			Debug.Log("DoPattern2");
		}

		public override bool Predict() => true;
	}
}