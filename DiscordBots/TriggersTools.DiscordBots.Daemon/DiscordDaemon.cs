using System;
using System.Diagnostics;
using System.Linq;

namespace TriggersTools.DiscordBots {
	public static class DiscordDaemon {
		public static void RunDotNet(string dll, params string[] args) {
			bool singleRun = GetSingleRun(ref args);
			Process process;
			do {
				Console.ResetColor();
				Console.WriteLine("================================================");
				ProcessStartInfo start = new ProcessStartInfo() {
					FileName = "dotnet",
					Arguments = $"\"{dll}\"{(args.Any() ? " " : "")}{FormatArgs(args)}",
					//Arguments = $"{dll} \"{FormatArgs(args).Replace("\"", "\\\"")}\"",
					UseShellExecute = false,
				};
				process = Process.Start(start);
				process.WaitForExit();
			} while (!singleRun && ShouldRestart(process));
		}
		public static void RunExecutable(string exe, params string[] args) {
			bool singleRun = GetSingleRun(ref args);
			Process process;
			do {
				Console.ResetColor();
				Console.WriteLine("================================================");
				ProcessStartInfo start = new ProcessStartInfo() {
					FileName = exe,
					Arguments = FormatArgs(args),
					UseShellExecute = false,
				};
				process = Process.Start(start);
				process.WaitForExit();
			} while (!singleRun && ShouldRestart(process));
		}
		private static bool ShouldRestart(Process process) {
			return process.ExitCode != 0;
		}
		private static bool GetSingleRun(ref string[] args) {
			bool singleRun = args.Length != 0 && args[0] == "-debug";
			if (singleRun)
				args = args.Skip(1).ToArray();
			return singleRun;
		}
		private static string FormatArgs(string[] args) {
			if (args.Length != 0)
				return $"{string.Join(" ", args.Select(a => $"\"{a}\""))}";
			return string.Empty;
			/*if (args.Any())
				return $"-daemon {string.Join(" ", args.Select(a => $"\"{a}\""))}";
			return "-daemon";*/
		}
	}
}
