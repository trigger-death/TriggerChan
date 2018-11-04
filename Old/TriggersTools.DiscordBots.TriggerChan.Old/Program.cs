using System;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan {
	public class Program {
		
		static void Main(string[] args) {
			//APIDownTimeRenderer.CreateImage();
			//return;
			Startup.RunAsync(args).Wait();
		}
	}
}
