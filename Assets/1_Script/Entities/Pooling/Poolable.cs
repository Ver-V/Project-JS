using ProjectJS.ScriptableObjects;
using UnityEngine;
using ProjectJS.Manager;

namespace ProjectJS.Entities
{
	/// <summary>
	/// Pooling 한 객체들의 Auto-Return을 위해 필요합니다.
	/// </summary>
	public abstract class Poolable : MonoBehaviour
	{
		protected bool isUsing = false;

		protected PoolingType poolingType = PoolingType.None;
		public PoolingType PoolingType { get => poolingType; set => poolingType = value; }

		public virtual void OnSpawn()
		{
			isUsing = true;
			gameObject.SetActive(true);
		}

		public virtual void OnDespawn()
		{
			isUsing = false;
			gameObject.SetActive(false);
		}

		protected void Return()
		{
			Managers.Pool.Return(poolingType, gameObject);
		}
	}
}