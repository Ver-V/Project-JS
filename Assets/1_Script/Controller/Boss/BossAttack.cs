using ProjectJS.Animation;
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

		protected BossAttackAnimNotifier[] attackAnimNotifiers;
		protected bool isAttackAnimPlaying = false;

		private IBossPattern[] enablePattern = null;
		
		public bool IsAttackAnimPlaying => isAttackAnimPlaying;
		
		private void Awake()
		{
			enablePattern = GetComponents<IBossPattern>();
			enablePattern.Shuffle();
			enablePattern = enablePattern.Take(enablePatternCount).ToArray();

			attackAnimNotifiers = GetComponent<Animator>().GetBehaviours<BossAttackAnimNotifier>();
			foreach (var notifiers in attackAnimNotifiers)
			{
				notifiers.OnStartAction += OnAttackStart;
				notifiers.OnEndAction += OnAttackEnd;
			}
		}

		public IEnumerator GetRandomPattern()
		{
			enablePattern.Shuffle();
			return enablePattern.First().DoPattern(this, 0f);
		}

		protected virtual void OnAttackStart()
		{
			isAttackAnimPlaying = true;
		}

		protected virtual void OnAttackEnd()
		{
			isAttackAnimPlaying = false;
		}
	}
}