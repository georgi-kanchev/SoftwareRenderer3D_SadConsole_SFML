using System;
using System.Collections.Generic;

namespace SMPL
{
	public static class Event
	{
		public abstract class Parameters { }
		internal static Dictionary<string, SortedDictionary<uint, List<Action<Parameters>>>> eventListeners = new();

		public static void Subscribe(string eventName, Action<Parameters> method, uint order = uint.MaxValue)
		{
			if (eventListeners.ContainsKey(eventName) == false)
				eventListeners[eventName] = new();
			if (eventListeners[eventName].ContainsKey(order) == false)
				eventListeners[eventName][order] = new();
			eventListeners[eventName][order].Add(method);
		}
		public static void Unsubscribe(string eventName, Action<Parameters> method)
		{
			if (eventListeners.ContainsKey(eventName) == false)
				return;
			foreach (var kvp in eventListeners[eventName])
				if (kvp.Value.Contains(method))
				{
					kvp.Value.Remove(method);
					return;
				}
		}
		public static void Unsubscribe(string eventName)
		{
			if (eventListeners.ContainsKey(eventName))
				eventListeners[eventName].Clear();
		}
		public static void Unsubscribe(object obj)
		{
			foreach (var kvp in eventListeners)
				foreach (var kvp2 in kvp.Value)
					for (int i = 0; i < kvp2.Value.Count; i++)
						if (kvp2.Value[i].Target == obj)
							kvp2.Value.Remove(kvp2.Value[i]);
		}
		public static void Trigger(string eventName, Parameters parameters = default)
		{
			if (eventListeners.ContainsKey(eventName) == false)
				return;
			var actions = new SortedDictionary<uint, List<Action<Parameters>>>(eventListeners[eventName]);
			foreach (var kvp in actions)
				for (int i = 0; i < kvp.Value.Count; i++)
					kvp.Value[i].Invoke(parameters);
		}
	}
}
