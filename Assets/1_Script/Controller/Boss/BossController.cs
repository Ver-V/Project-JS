using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public interface IBossPattern
	{
		public IEnumerator DoPattern(BossController boss, float attack);
		// 체력 상태, 플레이어 위치 등을 탐지하여 사용 가능한 패턴인지 판단
		public bool Predict();	
	}

	public class BossController : NetworkBehaviour
    {
		private bool isPatterning = false;
		private Coroutine patternCoroutine = null;

		private void Awake()
		{
			isPatterning = false;
		}

		private void Update()
		{
			if (!isPatterning)
			{
				try
				{
					var patternQuery = from pattern in GetComponents<IBossPattern>() where pattern.Predict() orderby Guid.NewGuid() select pattern;
					patternCoroutine = StartCoroutine(RunAttack(patternQuery.First(), 0f /** TODO - Boss attack power needed **/));
				} 
				catch { }
			}
		}

		public void StopAttack()
		{
			if (isPatterning) StopCoroutine(patternCoroutine);
		}

		private IEnumerator RunAttack(IBossPattern pattern, float attackPower)
		{
			isPatterning = true;
			yield return pattern.DoPattern(this, attackPower);
			isPatterning = false;
		}
	}
}