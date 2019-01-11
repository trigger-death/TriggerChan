using System;
using System.IO;
using System.Reflection;
using TriggersTools.DiscordBots;

namespace TriggerChan.Daemon {
	class Program {
		static void Main(string[] args) {
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			Run(args);
		}

		private static void Run(string[] args) {
			//Console.Title = "Discord Bot Deploy";
			const string path = "TriggerChan";
#if NETCOREAPP2_1
			DiscordDaemon.RunDotNet(Path.Combine(AppContext.BaseDirectory, $"{path}.dll"), args);
#else
			DiscordDaemon.RunExecutable(Path.Combine(AppContext.BaseDirectory, $"{path}.exe"), args);
#endif
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
			AssemblyName assemblyName = new AssemblyName(args.Name);
			string path = Path.Combine(AppContext.BaseDirectory, "libraries", assemblyName.Name + ".dll");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			path = Path.Combine(AppContext.BaseDirectory, "libraries", assemblyName.Name + ".exe");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			path = Path.ChangeExtension(path, ".so");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			return null;
		}
	}
}
