using System;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan {
	public class Program {
		
		static void Main(string[] args) {
			Startup.RunAsync(args).Wait();
		}
	}
}
