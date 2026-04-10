using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public partial class BossController : NetworkBehaviour
	{
		protected Animator animator;
		protected BossAttack bossAttack;

		private void Awake()
		{
			OnAwake();
		}

		protected virtual void OnAwake()
		{
			animator = GetComponent<Animator>();
			bossAttack = GetComponent<BossAttack>();
		}
		
		public void RequestAnimTrigger(string param)
		{
			animator.SetTrigger(param);
		}
	}
}