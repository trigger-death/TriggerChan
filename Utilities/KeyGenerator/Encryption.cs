using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordKeyGenerator {
	public static class Encryption {

		public static string GenerateBase64(int length) {
			return Convert.ToBase64String(GenerateBytes(length));
		}

		public static byte[] GenerateBytes(int length) {
			Random random = new Random();
			byte[] bytes = new byte[length];
			random.NextBytes(bytes);
			return bytes;
		}
	}
}
