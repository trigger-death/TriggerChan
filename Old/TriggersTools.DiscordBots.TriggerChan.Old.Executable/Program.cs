using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Executable {
	class Program {

		const int RestartExitCode = 10;

		static void Main(string[] args) {
			Console.Title = "Trigger-Chan";
			Process process;
			do {
				ProcessStartInfo start = new ProcessStartInfo() {
					FileName = "dotnet",
					Arguments = "TriggersTools.DiscordBots.TriggerChan.dll -wrapper",
					//RedirectStandardOutput = true,
					//RedirectStandardError = true,
					UseShellExecute = false,
				};
				process = Process.Start(start);
				process.WaitForExit();
			} while (process.ExitCode == 10);
		}
	}
}
