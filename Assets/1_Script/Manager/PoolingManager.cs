using ProjectJS.Entities;
using ProjectJS.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectJS.Manager
{
	public class PoolingManager
	{
		private Transform poolRoot = null;
		private Dictionary<PoolingType, PoolingEntry> entryDict = new();
		private Dictionary<PoolingType, Stack<GameObject>> poolDict = new();

		private Dictionary<int, GameObject> projectileDict = new();

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
					Stack<GameObject> stack = new Stack<GameObject>();
					poolDict.Add(entry.Type, stack);

					for (int i = 0; i < entry.InitCount; i++)
					{
						GameObject obj = CreateNewObject(entry);
						obj.SetActive(false);
						stack.Push(obj);
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
				obj = queue.Pop();
			}
			else
			{
				obj = CreateNewObject(entry);
			}

			obj.transform.position = position;
			obj.transform.rotation = rotation;
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
			// obj.transform.SetParent(poolRoot);

			if (type >= PoolingType.Projectile_Angel_Spiral)
			{
				int projId = obj.GetComponent<Projectile>().Id;
				if (projectileDict.ContainsKey(projId))
					projectileDict.Remove(projId);
			}

			poolDict[type].Push(obj);
		}

		public bool RegisterProjectileSync(int id, GameObject gameObject)
		{
			if (projectileDict.ContainsKey(id))
			{
				Debug.LogError($"[PoolingManager] ID exists : {id}");
				return false;
			}

			projectileDict[id] = gameObject;
			return true;
		}
		public bool RemoveProjectile(int id)
        {
            if (!projectileDict.ContainsKey(id))
            {
                Debug.LogError($"[PoolingManager] ID exists : {id}");
                return false;
            }

			GameObject projectileGo = projectileDict[id];
            Return(projectileGo.GetComponent<Poolable>().PoolingType, projectileGo);
			return true;
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
