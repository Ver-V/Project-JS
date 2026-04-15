using ProjectJS.ScriptableObjects;
using UnityEngine;

namespace ProjectJS.Manager
{
	public class ResourceManager
	{
		private readonly string PATH_BOSSSTAT = "Datas/Boss/";

		private BossStat[] bossStats;

		public void Init()
		{
			bossStats = Resources.LoadAll<BossStat>(PATH_BOSSSTAT);
		}

		public BossStat GetBossStat(/* difficulty, bossIdx .. */)
		{
			return bossStats[0];
		}
	}
}