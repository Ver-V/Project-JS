using System.Collections;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class TestBossPattern1 : MonoBehaviour
		, IBossPattern
	{
		public IEnumerator DoPattern(BossAttack boss, float attack)
		{
			boss.GetComponent<BossController>().RequestAnimTrigger("isAttack");
			yield return null;
			yield return new WaitUntil(() => !boss.IsAttackAnimPlaying);
		}

		public bool Predict() => true;
	}
}