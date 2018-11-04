using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan {
	class Program {
		static async Task<int> Main(string[] args) {
			//Process.Start("java", "-jar Lavalink.jar").Dispose();
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			return await Run(args).ConfigureAwait(false);
		}

		static async Task<int> Run(string[] args) {
			Console.Title = "Trigger-Chan - Discord Bot";
			return await DiscordStartup.RunAsync(args, () => new TriggerChanBot()).ConfigureAwait(false);
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
			AssemblyName assemblyName = new AssemblyName(args.Name);
			string path = Path.Combine(AppContext.BaseDirectory, "libraries", assemblyName.Name + ".dll");
			return (File.Exists(path) ? Assembly.LoadFile(path) : null);
		}

	}
}
