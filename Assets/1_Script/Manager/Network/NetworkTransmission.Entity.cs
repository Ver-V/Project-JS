using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProjectJS.Manager
{
	public enum GameEventType
	{
		Camera_ToBoss,
		Camera_ToPlayer,
	}

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

		[Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
		public void RemoveProjectileClientRPC(int id)
        {
            Managers.Pool.RemoveProjectile(id);
        }


		/// <summary>
		/// Synchronize clients event
		/// 
		/// ex) Wait until all clients' cameras finish moving, then trigger the next event.
		/// </summary>

		private int currentEventId = 0;
		private Action onAllFinishedEvent = null;
		private HashSet<ulong> finishedClients = new();

		public Action<GameEventType, int> OnGameEvent { get; set; }

		public void StartEventSync(Action finishEvent, GameEventType evtType)
		{
			if (!NetworkManager.IsHost) return;

			onAllFinishedEvent = finishEvent;
			finishedClients.Clear();

			StartEventSyncClientRPC(evtType, ++currentEventId);
			OnGameEvent.Invoke(evtType, currentEventId);	// OnHost
		}

		[Rpc(SendTo.NotServer, InvokePermission = RpcInvokePermission.Server)]
		public void StartEventSyncClientRPC(GameEventType evtType, int eventId)
		{
			currentEventId = eventId;
			OnGameEvent.Invoke(evtType, eventId);	// OnClient
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
		public void ReportEventFinishServerRPC(int eventId, ulong clinetId)
		{
			if (currentEventId != eventId) return;
			
			if (finishedClients.Add(clinetId))
			{
				if(finishedClients.Count == NetworkManager.ConnectedClientsList.Count)
				{
					onAllFinishedEvent?.Invoke();
				}
			}
		}


	}
}