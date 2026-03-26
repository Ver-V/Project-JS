using System.Collections;
using UnityEngine;

namespace ProjectJS.Controller
{
    public class TestBossPattern1 : MonoBehaviour
		, IBossPattern
	{
		public IEnumerator DoPattern(BossController boss, float attack)
		{
			yield return new WaitForSeconds(1.0f);
			// TODO -player.TakeDamage(attack);
			Debug.Log("DoPattern1");
		}

		public bool Predict() => true;
	}
}