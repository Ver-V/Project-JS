using ProjectJS.Controller;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Manager
{
	/// <summary>
	/// Synchronize Entities ( Projectile, Boss Pattern ... ) 
	/// </summary>
	public partial class NetworkTransmission
	{
		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
		public void RemoveProjectileServerRPC(int id)
		{
			if(!Managers.Pool.RemoveProjectile(id)) return;

			RemoveProjectileClientRPC(id);
		}

		[Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Server)]
		public void RemoveProjectileClientRPC(int id)
        {
            Managers.Pool.RemoveProjectile(id);
        }
	}
}