using ProjectJS.Event;
using UnityEngine;
using Unity.Netcode;

namespace ProjectJS.Manager
{
	public class NetworkDispatcher : NetworkBehaviour
	{
		private void Awake()
		{
			EventBus.Subscribe<EffectEvent>(OnEffectEvent);
		}

		private void OnEffectEvent(EffectEvent evt)
		{
			BroadcastEffectRPC(evt.Type, evt.Position);
		}

		[Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Everyone)]
		private void BroadcastEffectRPC(EffectType type, Vector3 pos)
		{
			EventBus.Publish(new EffectEvent(type, pos));
		}
	}
}