using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class ConcurrentDictionaryExtensions {

		public static bool TryAddOrGet<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> dictionary,
			TKey key, ref TValue value)
		{
			TValue result;
			do {
				if (dictionary.TryAdd(key, value))
					return true;
			}
			while (!dictionary.TryGetValue(key, out result));
			value = result;
			return false;
		}

	}
}
