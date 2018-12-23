using System;
using System.IO;
using System.Reflection;
using TriggersTools.DiscordBots;

namespace TriggerChan.Daemon {
	class Program {
		static void Main(string[] args) {
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			Run();
		}

		static void Run() {
			Console.Title = "Discord Bot Deploy";
			const string Path = "TriggerChan";
#if NETCOREAPP2_1
			DiscordDaemon.RunDotNet($"{Path}.dll");
#else
			DiscordDaemon.RunExecutable($"{Path}.exe");
#endif
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
			AssemblyName assemblyName = new AssemblyName(args.Name);
			string path = Path.Combine(AppContext.BaseDirectory, "libraries", assemblyName.Name + ".dll");
			return (File.Exists(path) ? Assembly.LoadFile(path) : null);
		}
	}
}
