using ProjectJS.ScriptableObjects;
using System;
using UnityEngine;

namespace ProjectJS.Entities
{
	/// <summary>
	/// Pooling 한 객체들의 Auto-Return을 위해 필요합니다.
	/// </summary>
	public class Poolable : MonoBehaviour
	{
		private float timer = 0f;
		private Action<PoolingType, GameObject> onReturnEvent = null;
		private PoolingType poolingType = PoolingType.None;

		public void SetPoolable(PoolingType type, Action<PoolingType, GameObject> evt)
		{
			poolingType = type;
			onReturnEvent = evt;
		}

		public void Return()
		{
			if(onReturnEvent == null)
			{
				Debug.LogError("Return Action is null");
				return;
			}

			onReturnEvent.Invoke(poolingType, gameObject);
		}

		public void Clear()
		{
			timer = 0f;
		}

		private void Update()
		{
			if (timer > 0f)
			{
				timer -= Time.deltaTime;
				if (timer <= 0f)
				{
					
				}
			}	
		}
	}
}