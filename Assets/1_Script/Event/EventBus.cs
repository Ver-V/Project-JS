using System;
using System.Collections.Generic;

namespace ProjectJS.Event
{
	/// <summary>
	/// 
	/// 이벤트 처리를 위한 EventBus Class
	/// 
	/// </summary>
	public static class EventBus
	{
		private static Dictionary<Type, Action<object>> events = new();

		public static void Subscribe<T>(Action<T> callback)
		{
			if (!events.ContainsKey(typeof(T)))
				events[typeof(T)] = null;

			events[typeof(T)] += (obj) => callback((T)obj);
		}

		public static void Publish<T>(T evt)
		{
			if (events.TryGetValue(typeof(T), out var action))
				action?.Invoke(evt);
		}
	}
}
