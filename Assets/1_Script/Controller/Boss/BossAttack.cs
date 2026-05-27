using ProjectJS.Animation;
using ProjectJS.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ProjectJS.Controller
{
	[System.Flags]
	public enum BossPhaseType
	{
		None = 0,
		Phase1 = 1 << 0,
		Phase2 = 1 << 1,
		Phase3 = 1 << 2,
	}

	public interface IBossPattern
	{
		public IEnumerator DoPattern(BossAttack boss, float attack);
		// 체력 상태, 플레이어 위치 등을 탐지하여 사용 가능한 패턴인지 판단
		public bool Predict();
		public BossPhaseType EnablePhase();
	}

	public class BossAttack : MonoBehaviour
    {
		[SerializeField] private int enablePatternCount;

		protected BossAttackAnimNotifier[] attackAnimNotifiers;
		protected bool isAttackAnimPlaying = false;

		private BossPatternBase[] enablePattern = null;
		
		public bool IsAttackAnimPlaying => isAttackAnimPlaying;
		
		private void Awake()
		{
			enablePattern = GetComponents<BossPatternBase>();
			enablePattern.Shuffle();
			enablePattern = enablePattern
				.Where(x => x.enabled)
				.Take(enablePatternCount)
				.ToArray();

			attackAnimNotifiers = GetComponent<Animator>().GetBehaviours<BossAttackAnimNotifier>();
			foreach (var notifiers in attackAnimNotifiers)
			{
				notifiers.OnStartAction += OnAttackStart;
				notifiers.OnEndAction += OnAttackEnd;
			}
		}

		public IEnumerator GetRandomPattern(BossPhaseType currentPhase)
		{
			var validPatterns = enablePattern
				.Where(pattern => (pattern.EnablePhase() & currentPhase) != 0)
				.OrderBy(_ => UnityEngine.Random.value)
				.ToList();

			if (validPatterns.Count == 0)
				return null;

			return validPatterns.First().DoPattern(this, 0f);
		}

		public void OnReset()
		{
			isAttackAnimPlaying = false;
		}

		// HACK - 테스트용 함수. 어그로 로직 개발 필요
		public Transform GetTarget()
		{
			return FindObjectOfType<PlayerController>().transform;
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