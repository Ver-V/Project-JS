using System.Collections;
using UnityEngine;

namespace ProjectJS.Controller
{
	public class TestBossPattern2 : MonoBehaviour
		, IBossPattern
	{
		public IEnumerator DoPattern(BossController boss, float attack)
		{
			yield return new WaitForSeconds(2.0f);
			// TODO -player.TakeDamage(attack);
			Debug.Log("DoPattern2");
		}

		public bool Predict() => true;
	}
}