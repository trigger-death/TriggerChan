using System;
using System.IO;
using System.Reflection;
using Console = DotNetDeploy.Utils.ColorConsole;

namespace DotNetDeploy {
	class Program {
		static int Main(string[] args) {
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			return Run(args);
		}
		static int Run(string[] args) {
			System.Console.Title = ".Net Deployer";
			//Console.WriteLine(ConsoleColor.Cyan, "==== Triggy Deploy ====");
			//Console.WriteLine();


			/*AsciiImage dotNetDeployLogo;
			using (var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Resources", "dotnet.asc")))
				dotNetDeployLogo = AsciiImage.FromStream(stream);

			dotNetDeployLogo.Draw(10);
			Console.WriteLine();*/

			DrawLogo();

			Console.WriteLine("Deployments: ");
			Deployment[] deployments = Deployment.GetDeployments();
			for (int i = 0; i < deployments.Length; i++) {
				Console.WriteLine($"{(i + 1).ToString().PadLeft(2)}) {deployments[i].Name}");
			}

			Console.WriteLine();
			//Console.Write("Use Deployment: ");
			//Deployment choice = deployments[int.Parse(Console.ReadLine()) - 1];
			int index = ReadRange("Use Deployment", 1, deployments.Length) - 1;
			Deployment deployment = deployments[index];

			//Console.Write("Rebuild? (y/n): ");
			//string yesNoStr = Console.ReadLine().ToLower();
			//bool build = yesNoStr == "y" || yesNoStr == "yes";
			bool build = ReadYesNo("Rebuild?", true);

			//Console.Write("Execute? (y/n): ");
			//yesNoStr = Console.ReadLine().ToLower();
			//bool execute = yesNoStr == "y" || yesNoStr == "yes";
			bool execute = ReadYesNo("Execute?", true);

			//Console.Write("Verbose? (y/n): ");
			//yesNoStr = Console.ReadLine().ToLower();
			//bool verbose = yesNoStr == "y" || yesNoStr == "yes";
			bool verbose = ReadYesNo("Verbose?", false);

			Console.WriteLine();

			int returnCode = 0;
			try {
				Deployer.Run(deployment, build, execute, verbose);
			} catch (Exception ex) {
				Console.Error("Error: ");
				Console.ErrorMessageLine(ex);
				returnCode = 1;
			}

			Console.Write("Press enter to exit...");
			Console.ReadLine();
			return returnCode;
		}
		static int ReadRange(string label, int min, int max, int? defaultValue = null) {
			string input = null;
			int result;
			do {
				if (input != null)
					Console.ErrorLine($"Invalid input! Must be between {min} and {max}");
				Console.Write($"{label}: ");
				if (defaultValue.HasValue)
					Console.Watermark(defaultValue.Value);
				input = Console.ReadLine().ToLower();
				if (string.IsNullOrEmpty(input) && defaultValue.HasValue)
					return defaultValue.Value;
			} while (!int.TryParse(input, out result) || (result < min || result > max));
			return result;
		}
		static bool ReadYesNo(string label, bool? defaultValue = null) {
			string input = null;
			do {
				if (input != null)
					Console.ErrorLine("Invalid input! Must be yes/y/no/n");
				Console.Write($"{label} (y/n): ");
				if (defaultValue.HasValue)
					Console.Watermark((defaultValue.Value ? "y" : "n"));
				input = Console.ReadLine().ToLower();
				if (string.IsNullOrEmpty(input) && defaultValue.HasValue)
					return defaultValue.Value;
			} while (input != "yes" && input != "y" && input != "no" && input != "n");
			return input == "yes" || input == "y";
		}
		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
			AssemblyName assemblyName = new AssemblyName(args.Name);
			string baseDir = AppContext.BaseDirectory;
			//return Assembly.LoadFile(Path.Combine(baseDir, "libs", assemblyName.Name + ".dll"));
			string path = Path.Combine(baseDir, "libraries", assemblyName.Name + ".dll");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			path = Path.Combine(baseDir, "libs", assemblyName.Name + ".dll");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			path = Path.Combine(baseDir, "libraries", assemblyName.Name + ".exe");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			path = Path.Combine(baseDir, "libs", assemblyName.Name + ".exe");
			if (File.Exists(path)) return Assembly.LoadFile(path);
			return null;
		}
		private static void DrawLogo() {
			Assembly assembly = typeof(AsciiImage).Assembly;
			using (Stream stream = assembly.GetManifestResourceStream(
				$"DotNetDeploy.Resources.dotnet.asc")) {
				AsciiImage logo = AsciiImage.FromStream(stream);
				logo.Draw(10);
				Console.WriteLine();
			}
		}
	}
}
