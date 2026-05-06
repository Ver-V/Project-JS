using System.Collections;
using UnityEngine;

namespace ProjectJS.Controller
{
	public abstract class BossPatternBase : MonoBehaviour
		, IBossPattern
	{
		[Header("Patttern Base Settings")]
		[SerializeField] BossPhaseType phaseType;

		public BossPhaseType EnablePhase() => phaseType;

		public abstract IEnumerator DoPattern(BossAttack boss, float attack);
		public abstract bool Predict();
	}
}
