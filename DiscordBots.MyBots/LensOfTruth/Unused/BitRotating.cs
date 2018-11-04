using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.SpoilerBot.Utils {
	public static class BitRotating {

		public static int RotateLeft(int value, int count) {
			int bits = sizeof(int) * 8;
			count %= bits;
			if (count == 0)
				return value;
			return (value << count) | (value >> (bits - count));
		}

		public static int RotateRight(int value, int count) {
			int bits = sizeof(int) * 8;
			count %= bits;
			if (count == 0) return value;
			return (value >> count) | (value << (bits - count));
		}
	}
}
