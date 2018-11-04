using System;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.SpoilerBot {
	class Program {
		static async Task<int> Main(string[] args) {
			Console.Title = "Lens of Truth - Discord Bot";
			return await DiscordStartup.RunAsync(args, () => new LensOfTruthBot()).ConfigureAwait(false);
		}
	}
}
