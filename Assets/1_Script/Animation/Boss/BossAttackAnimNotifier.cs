using System;
using UnityEngine;

namespace ProjectJS.Animation
{
    public class BossAttackAnimNotifier : StateMachineBehaviour
    {
		private Action onEndAction;
		private Action onStartAction;

		public Action OnEndAction { get => onEndAction; set => onEndAction = value; }	
		public Action OnStartAction { get => onStartAction; set => onStartAction = value; }	

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			onEndAction?.Invoke();
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			onStartAction?.Invoke();
		}
	}
}