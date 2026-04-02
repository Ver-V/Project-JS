using ProjectJS.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectJS.Controller
{
	public interface IBossPattern
	{
		public IEnumerator DoPattern(BossAttack boss, float attack);
		// 체력 상태, 플레이어 위치 등을 탐지하여 사용 가능한 패턴인지 판단
		public bool Predict();	
	}

	public class BossAttack : MonoBehaviour
    {
		[SerializeField] private int enablePatternCount;

		private bool isPatterning = false;
		private Coroutine patternCoroutine = null;
		private IBossPattern[] enablePattern = null;
		
		private void Awake()
		{
			isPatterning = false;
			enablePattern = GetComponents<IBossPattern>();
			enablePattern.Shuffle();
			enablePattern = enablePattern.Take(enablePatternCount).ToArray();
		}

		public void StartAttack()
		{
			if (isPatterning)
			{
				Debug.LogWarning("BossAttack - Pattern in progress.");
				return; 
			}

			try
			{
				enablePattern.Shuffle();
				patternCoroutine = StartCoroutine(AttackCoroutine(enablePattern.First(), 0f /** TODO - Boss attack power needed **/));
			}
			catch { }
		}

		public void StopAttack()
		{
			if (!isPatterning)
			{
				Debug.LogWarning("BossAttack - Pattern is not in progress.");
				return;
			}

			StopCoroutine(patternCoroutine);
		}

		private IEnumerator AttackCoroutine(IBossPattern pattern, float attackPower)
		{
			isPatterning = true;
			yield return pattern.DoPattern(this, attackPower);
			isPatterning = false;
		}
	}
}