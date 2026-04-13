using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Controller
{
	public partial class BossController
	{
		[Header("DEBUG")]
		[SerializeField] protected NetworkVariable<float> currentHP = new();

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
		public void RequestTakeDamageServerRpc(float damage)
		{
			if (!statContainer.TryGet(out HealthStat healthStat))
			{
				Debug.LogError("No Health Stat");
				return;
			}

			healthStat.TakeTrueDamage(damage);
			if (!healthStat.IsDead)
			{
				OnDamaged();
			}
			else
			{
				currentHP.Value = 0;
				OnDead();
			}
		}
		
	}
}
