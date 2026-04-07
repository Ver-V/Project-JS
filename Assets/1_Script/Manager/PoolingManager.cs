using ProjectJS.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;
using ProjectJS.Entities;

namespace ProjectJS.Manager
{
	public class PoolingManager
	{
		private Transform poolRoot = null;
		private Dictionary<PoolingType, PoolingEntry> entryDict = new();
		private Dictionary<PoolingType, Queue<GameObject>> poolDict = new();

		private const string PATH_POOLDATA = "Datas/PoolData/";

		public void Init()
		{
			poolRoot = new GameObject { name = "PoolRoot" }.transform;
			poolRoot.position = Vector3.zero;
			GameObject.DontDestroyOnLoad(poolRoot.gameObject);

			PoolingData[] poolDatas = Resources.LoadAll<PoolingData>(PATH_POOLDATA);
			foreach (PoolingData poolData in poolDatas)
			{
				foreach (PoolingEntry entry in poolData.PoolingList)
				{
					if (entryDict.ContainsKey(entry.Type))
					{
						Debug.LogError($"[PoolingManager] Duplicate PoolingType: {entry.Type}");
						continue;
					}

					entryDict.Add(entry.Type, entry);
					Queue<GameObject> queue = new Queue<GameObject>();
					poolDict.Add(entry.Type, queue);

					for (int i = 0; i < entry.InitCount; i++)
					{
						GameObject obj = CreateNewObject(entry);
						obj.SetActive(false);
						queue.Enqueue(obj);
					}
				}
			}
		}

		public GameObject Get(PoolingType type)
		{
			return Get(type, Vector3.zero, Quaternion.identity);
		}
		public GameObject Get(PoolingType type, Vector3 position)
		{
			return Get(type, position, Quaternion.identity);
		}
		public GameObject Get(PoolingType type, Vector3 position, Quaternion rotation)
		{
			if (!entryDict.TryGetValue(type, out var entry))
			{
				Debug.LogError($"[PoolingManager] No entry for type: {type}");
				return null;
			}

			var queue = poolDict[type];
			GameObject obj;

			if (queue.Count > 0)
			{
				obj = queue.Dequeue();
			}
			else
			{
				obj = CreateNewObject(entry);
			}

			obj.GetComponent<Poolable>().OnSpawn();
			return obj;
		}

		public void Return(PoolingType type, GameObject obj)
		{
			if (!poolDict.ContainsKey(type))
			{
				Debug.LogError($"[PoolingManager] No pool for type: {type}");
				GameObject.Destroy(obj);
				return;
			}

			obj.GetComponent<Poolable>().OnDespawn();
			obj.transform.SetParent(poolRoot);

			poolDict[type].Enqueue(obj);
		}
		
		private GameObject CreateNewObject(PoolingEntry entry)
		{
			GameObject obj = GameObject.Instantiate(entry.Prefab, poolRoot);
			obj.name = $"{entry.Type}_Pooled";
			obj.gameObject.SetActive(false);
			obj.GetComponent<Poolable>().PoolingType = entry.Type;
			return obj;
		}
	}
}
