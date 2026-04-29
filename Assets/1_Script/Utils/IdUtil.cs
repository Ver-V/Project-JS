namespace ProjectJS.Utils
{
	public static class IdUtil
	{
		public static readonly int idMult = 1000;

		public static int GetProjectileID(int bossId, int projectileIdx)
		{
			return bossId * idMult + (projectileIdx % idMult);
		}
	}
}
