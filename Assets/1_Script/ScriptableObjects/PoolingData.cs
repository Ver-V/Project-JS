using System.Collections.Generic;
using UnityEngine;

namespace ProjectJS.ScriptableObjects
{
	public enum PoolingType
    {
        // Dummy
        None = 0,

        // SFX (1)
        SFX = 1,

        // VFX (51 ~ 100)
        VFX_Explode = 51,

        // Projectile (101 ~ 120)
        Projectile_Bullet = 101,
        Projectile_Bullet1,
        Projectile_Bullet2,
    }

    [System.Serializable]
    public class PoolingEntry
    {
        [SerializeField] private PoolingType type;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initCount;

        public PoolingType Type => type;
        public GameObject Prefab => prefab;
        public int InitCount => initCount;
    }

    [CreateAssetMenu(fileName = "PoolingData", menuName = "Data/PoolingData")]
    public class PoolingData : ScriptableObject
    {
        [SerializeField] private List<PoolingEntry> poolingList = new();

        public List<PoolingEntry> PoolingList => poolingList;
	}
}