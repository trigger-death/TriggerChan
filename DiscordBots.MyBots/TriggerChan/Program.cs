using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan {
	class Program {
		/// <summary>
		/// Run the Discord Bot Program and setup assembly resolution.
		/// </summary>
		/// <param name="args">The program arguments.</param>
		/// <returns>The exit code.</returns>
		static async Task<int> Main(string[] args) {
			// Fix for Ubuntu case-sensitivity not working
			CultureInfo.CurrentCulture = new CultureInfo("en-US");
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			int exitCode = await Run(args).ConfigureAwait(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return exitCode;
		}
		/// <summary>
		/// Run the Discord Bot.
		/// </summary>
		/// <param name="args">The program arguments.</param>
		/// <returns>The exit code.</returns>
		/// <remarks>
		/// We have this function so that we can resolve assemblies that this function requires.
		/// </remarks>
		private static async Task<int> Run(string[] args) {
			//Console.Title = "Trigger-chan - Discord Bot";
			Console.ForegroundColor = ConsoleColor.Cyan;
			FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
			Console.WriteLine($"Trigger-chan Discord Bot : v{fileVersion.ProductVersion}");
			Console.ResetColor();
			return await DiscordStartup.RunAsync(args, () => new TriggerChanBot()).ConfigureAwait(false);
		}
		/// <summary>
		/// Resolves assemblies from the "libraries" folder.
		/// </summary>
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
