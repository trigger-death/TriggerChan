using TriggersTools.DiscordBots;

namespace LensOfTruth.Daemon {
	class Program {
		static void Main(string[] args) {
			const string Path = "LensOfTruth";
#if NETSTANDARD2_0
			DiscordDaemon.RunDotNet($"{Path}.dll");
#else
			DiscordDaemon.RunExecutable($"{Path}.exe");
#endif
		}
	}
}
