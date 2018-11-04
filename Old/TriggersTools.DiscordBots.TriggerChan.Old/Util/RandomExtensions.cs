using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class RandomExtensions {

		public static T Choose<T>(this Random random, T[] list) {
			if (list.Length == 0)
				return default;
			else
				return list[random.Next(list.Length)];
		}

		public static T Choose<T>(this Random random, List<T> list) {
			if (list.Count == 0)
				return default;
			else
				return list[random.Next(list.Count)];
		}
	}
}
