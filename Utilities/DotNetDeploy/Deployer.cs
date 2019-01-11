using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetDeploy.Utils;
using WinSCP;
using Console = DotNetDeploy.Utils.ColorConsole;

namespace DotNetDeploy {
	public partial class Deployer {
		#region Fields

		public Deployment Deployment { get; }
		public bool ShouldBuild { get; }
		public bool ShouldExecute { get; }
		public bool IsVerbose { get; }
		public IDeployMethod Method { get; private set; }
		public IDeployMethod LocalMethod { get; } = new LocalDeployMethod();
		public string Source { get; }
		public string Destination { get; }
		public string ConfigDestination { get; }

		#endregion

		#region Constructors

		private Deployer(Deployment deployment, bool build, bool execute, bool verbose) {
			Deployment = deployment;
			ShouldBuild = build;
			ShouldExecute = execute;
			IsVerbose = verbose;
			PublishOptions publish = deployment.Publish;
			DeployOptions deploy = deployment.Deploy;
			Source = PathUtils.Combine(publish.Output, "bin", publish.Configuration, publish.Framework, publish.Runtime, "publish").Replace('\\', '/');
			Destination = deploy.Destination.Replace('\\', '/');
			ConfigDestination = deploy.ConfigDestination.Replace('\\', '/');
		}

		#endregion

		#region Run

		public static void Run(Deployment deployment, bool build, bool execute, bool verbose) {
			Deployer deployer = new Deployer(deployment, build, execute, verbose);
			try {
				deployer.Run();
			} finally {
				deployer.Method.Dispose();
			}
		}
		private void Run() {
			ConsoleColor color = ConsoleColor.Cyan;
			Console.WriteLine(color, "Initializing Deployment...");

			Console.WriteLine(color, "Loading Filters...");
			Deployment.LoadFilters();

			Console.WriteLine(color, "Getting Deploy Method...");
			Method = Deployment.Deploy.BuildMethod();

			if (ShouldBuild) {
				Console.WriteLine();
				Console.WriteLine(color, "Building...");
				Publish();
			}

			if (ShouldExecute && Deployment.Deploy.PreCommands.Any()) {
				Console.WriteLine(color, "Executing Pre-Deploy...");
				ExecutePre();
			}

			Console.WriteLine();
			Console.WriteLine(color, "Deploying...");
			Deploy();

			if (ShouldExecute && Deployment.Deploy.PostCommands.Any()) {
				Console.WriteLine();
				Console.WriteLine(color, "Executing Post-Deploy...");
				PostDeploy();
			}

			Console.WriteLine();
			Console.WriteLine(ConsoleColor.DarkMagenta, "Finished!");
			Console.Beep();
		}

		#endregion

		#region Publish

		private void Publish() {
			// Cleanup the publish directory first
			if (Directory.Exists(Source))
				Directory.Delete(Source, true);

			PublishOptions pub = Deployment.Publish;
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "dotnet",
				Arguments = $"publish \"{pub.Project}\" -c \"{pub.Configuration}\" -f \"{pub.Framework}\" -r \"{pub.Runtime}\"",
				UseShellExecute = false,
			};
			if (!IsVerbose) {
				startInfo.UseShellExecute = true;
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			}
			Process process = Process.Start(startInfo);
			process.WaitForExit();
			if (process.ExitCode != 0) {
				throw new Exception($"Publish process exited with error code {process.ExitCode}!");
			}
		}

		#endregion

		#region Execute

		public void ExecutePre() {
			foreach (string command in Deployment.Deploy.PreCommands) {
				Method.Execute(command);
			}
		}

		public void PostDeploy() {
			foreach (string command in Deployment.Deploy.PostCommands) {
				Method.Execute(command);
			}
		}

		#endregion
	}
}
