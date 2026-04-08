using UnityEngine;

namespace ProjectJS.Event
{
	public enum EffectType
	{
		Hit,
	}

	public class EffectEvent
	{
		private EffectType type;
		private Vector3 position;

		public EffectType Type => type;
		public Vector3 Position => position;

		public EffectEvent(EffectType type, Vector3 position)
		{
			this.type = type;
			this.position = position;
		}
	}
}
