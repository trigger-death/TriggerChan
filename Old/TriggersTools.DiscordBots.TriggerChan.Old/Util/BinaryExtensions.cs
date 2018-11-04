using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class BinaryExtensions {
		
		public static byte[] ReadRemaining(this Stream stream) {
			using (MemoryStream memory = new MemoryStream()) {
				byte[] buffer = new byte[1024];
				int read;

				while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
					;
				return memory.GetBuffer();
			}
		}
	}
}
