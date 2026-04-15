using UnityEngine;

namespace ProjectJS.ScriptableObjects
{
	[CreateAssetMenu(fileName = "BossStat", menuName = "Data/BossStat")]
	public class BossStat : ScriptableObject
	{
		[SerializeField] private float maxHp;
		[SerializeField] private float attackPower;
		[SerializeField] private float defensePower;
		[SerializeField] private float moveSpeed;

		public float MaxHP => maxHp;
		public float AttackPower => attackPower;
		public float DefensePower => defensePower;
		public float MoveSpeed => moveSpeed;
	}
}
