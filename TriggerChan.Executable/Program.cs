using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Executable {
	class Program {
		static void Main(string[] args) {
			Console.Title = "Trigger-Chan";
			ProcessStartInfo start = new ProcessStartInfo() {
				FileName = "dotnet",
				Arguments = "TriggersTools.DiscordBots.TriggerChan.dll",
				//RedirectStandardOutput = true,
				//RedirectStandardError = true,
				UseShellExecute = false,
			};
			Process.Start(start).WaitForExit();
		}
	}
}
