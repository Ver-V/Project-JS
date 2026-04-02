using UnityEngine;

namespace ProjectJS.Utils
{
	public static class Extensions
	{
		// Fisher-Yates shuffle
		public static void Shuffle<T>(this T[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				int rand = UnityEngine.Random.Range(i, array.Length);
				(array[i], array[rand]) = (array[rand], array[i]);
			}
		}

		public static Vector3 ToVector3(this Vector2 v)
		{
			return new Vector3(v.x, 0, v.y);
		}

		public static Vector3 ToVector2(this Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}
	}
}
