using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TriggerChanDeploy {
	public static class Program {

		static string Project { get; set; }
		static string Configuration { get; set; }
		static string Framework { get; set; }
		static string Runtime { get; set; }
		static string ExecutableName { get; set; }
		static string PublishLocation { get; set; }
		static string DeployLocation { get; set; }

		static string ProcessName => Path.GetFileNameWithoutExtension(ExecutableName);
		static string ExecutablePath => Path.Combine(DeployLocation, ExecutableName);
		static string LibrariesPath => Path.Combine(DeployLocation, "libraries");
		static bool ConfigFiles = false;

		public static int Main(string[] args) {
			Console.WriteLine("==== Discord Bot Deploy ====");
			if (args.Length < 7 || args.Length > 8) {
				Console.WriteLine("Usage: DiscordBotDeploy.exe <project> <configuration> <framework> <runtime> <exeName> <publishLocation> <deployLocation> [config]");
				Console.ReadLine();
				return -1;
			}

			try {
				Project = args[0];
				Configuration = args[1];
				Framework = args[2];
				Runtime = args[3];
				ExecutableName = args[4];
				PublishLocation = Path.GetFullPath(args[5]);
				DeployLocation = Path.GetFullPath(args[6]);
				ConfigFiles = args.Length == 8;

				if (!ConfigFiles)
					Publish();
				//KillBot();
				CopyPublished();
				if (!ConfigFiles)
					RunBot();

			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
				Console.ReadLine();
				return -1;
			}
			Console.ReadLine();
			return 0;
		}

		static void Publish() {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "dotnet",
				Arguments = $"publish \"{Project}\" -c \"{Configuration}\" -f \"{Framework}\" -r \"{Runtime}\"",
				UseShellExecute = false,
			};
			Process process = Process.Start(startInfo);
			process.WaitForExit();
			if (process.ExitCode != 0) {
				throw new Exception($"Publish process exited with error code {process.ExitCode}!");
			}
		}
		
		/*static void KillBot() {
			foreach (var process in Process.GetProcessesByName(ProcessName))
				process.Kill();
		}*/

		static void RunBot() {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = ExecutablePath,
				WorkingDirectory = DeployLocation,
			};
			Process.Start(startInfo).Dispose();
		}

		static void CopyPublished() {
			if (!Directory.Exists(DeployLocation))
				Directory.CreateDirectory(DeployLocation);
			if (!Directory.Exists(LibrariesPath))
				Directory.CreateDirectory(LibrariesPath);

			/*FileInfo[] publishFiles = ReadFiles(PublishLocation);
			DirectoryInfo[] publishDirs = ReadDirectories(PublishLocation);
			FileInfo[] deployFiles = ReadFiles(DeployLocation);
			DirectoryInfo[] deployDirs = ReadDirectories(DeployLocation);*/
			string[] publishFiles = ReadFiles(PublishLocation);
			string[] publishDirs = ReadDirectories(PublishLocation);
			string[] deployFiles = ReadFiles(DeployLocation);
			string[] deployDirs = ReadDirectories(DeployLocation);

			/*HashSet<string> publishFilesSet = new HashSet<string>(
				publishFiles.Select(f => f.FullName),
				StringComparer.InvariantCultureIgnoreCase);
			HashSet<string> publishDirsSet = new HashSet<string>(
				publishDirs.Select(d => d.FullName),
				StringComparer.InvariantCultureIgnoreCase);

			HashSet<string> deployFilesSet = new HashSet<string>(
				deployFiles.Select(f => f.FullName),
				StringComparer.InvariantCultureIgnoreCase);
			HashSet<string> deployDirsSet = new HashSet<string>(
				deployDirs.Select(d => d.FullName),
				StringComparer.InvariantCultureIgnoreCase);*/

			HashSet<string> publishFilesSet = new HashSet<string>(
				publishFiles,
				StringComparer.InvariantCultureIgnoreCase);
			HashSet<string> publishDirsSet = new HashSet<string>(
				publishDirs,
				StringComparer.InvariantCultureIgnoreCase);

			HashSet<string> deployFilesSet = new HashSet<string>(
				deployFiles,
				StringComparer.InvariantCultureIgnoreCase);
			HashSet<string> deployDirsSet = new HashSet<string>(
				deployDirs,
				StringComparer.InvariantCultureIgnoreCase);

			deployFilesSet.ExceptWith(publishFiles);
			deployDirsSet.ExceptWith(publishDirs);
			publishFilesSet.ExceptWith(deployFiles);
			publishDirsSet.ExceptWith(deployDirs);
			var deleteFiles = deployFilesSet
				//.Except(publishFilesSet)
				.Where(f => Path.GetDirectoryName(f) != "");
			var deleteDirs = deployDirsSet
				//.Except(publishDirsSet)
				.Reverse(); // Reverse to delete highest directories first.

			var copyFiles = publishFilesSet;
			var createDirs = publishDirsSet;

			var replaceFiles = publishFiles.Intersect(deployFiles)
				.Where(f => File.GetLastWriteTimeUtc(GetPublish(f)) != File.GetLastWriteTimeUtc(GetDeploy(f)));

			var noDeleteFiles = deployFilesSet
				.Where(f => Path.GetDirectoryName(GetDeploy(f)) == DeployLocation);
			var noReplaceFiles = publishFilesSet
				.Where(f => File.GetLastWriteTimeUtc(GetPublish(f)) == File.GetLastWriteTimeUtc(GetDeploy(f)));

			Console.ForegroundColor = ConsoleColor.Red;
			if (!ConfigFiles) {
				foreach (string file in deleteFiles) {
					Console.WriteLine($"Deleted {file}");
					File.Delete(GetDeploy(file));
				}
				foreach (string dir in deleteDirs) {
					Console.WriteLine($"Deleted {dir}");
					Directory.Delete(GetDeploy(dir));
				}
			}
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach (string file in replaceFiles) {
				Console.WriteLine($"Replaced {file}");
				File.Copy(GetPublish(file), GetDeploy(file), true);
			}
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (string dir in createDirs) {
				Console.WriteLine($"Created {dir}");
				Directory.CreateDirectory(GetDeploy(dir));
			}
			foreach (string file in copyFiles) {
				Console.WriteLine($"Copied {file}");
				File.Copy(GetPublish(file), GetDeploy(file), true);
			}
			Console.ResetColor();

			foreach (string file in noDeleteFiles)
				Console.WriteLine($"Deploy: Did not delete {file}");
			foreach (string file in replaceFiles)
				Console.WriteLine($"Deploy: Did not replace {file}");

			var ignoredDeploy = Directory.EnumerateFileSystemEntries(DeployLocation)
				.Except(deployFiles).Except(deployDirs);
			var ignoredPublish = Directory.EnumerateFileSystemEntries(PublishLocation)
				.Except(publishFiles).Except(publishDirs);

			foreach (string file in ignoredDeploy)
				Console.WriteLine($"Deploy: Ignored {file}");
			foreach (string file in ignoredPublish)
				Console.WriteLine($"Publish: Ignored {file}");
		}

		static string GetDeploy(string path) {
			return Path.Combine(DeployLocation, path);
		}
		static string GetPublish(string path) {
			if (Path.GetDirectoryName(path) == "libraries")
				path = path.Substring("libraries".Length + 1);
			return Path.Combine(PublishLocation, path);
		}

		/*static FileInfo[] ReadFiles(string location) {
			return new DirectoryInfo(location)
				.EnumerateFiles("*", SearchOption.AllDirectories)
				.Where(f => !f.FullName.StartsWith(Path.Combine(location, "Logs"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(f => !f.FullName.StartsWith(Path.Combine(location, "Temp"),
					StringComparison.InvariantCultureIgnoreCase))
				//.Where(f => f.DirectoryName != location)
				.ToArray();
		}

		static DirectoryInfo[] ReadDirectories(string location) {
			return new DirectoryInfo(location)
				.EnumerateDirectories("*", SearchOption.AllDirectories)
				.Where(d => !d.FullName.StartsWith(Path.Combine(location, "Logs"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(d => !d.FullName.StartsWith(Path.Combine(location, "Temp"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(d => !d.FullName.StartsWith(Path.Combine(location, "libraries"),
					StringComparison.InvariantCultureIgnoreCase))
				.ToArray();
		}*/

		static string[] ReadFiles(string location) {
			return Directory
				.EnumerateFiles(location, "*", SearchOption.AllDirectories)
				.Where(f => !f.StartsWith(Path.Combine(location, "Logs"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(f => !f.StartsWith(Path.Combine(location, "Temp"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(f => string.Compare(Path.GetExtension(f), ".sqlite") != 0)
				.Where(f => string.Compare(Path.GetFileName(f), "DiscordBot.TotalUptime.txt") != 0)
				.Where(f => !ConfigFiles || string.Compare(Path.GetExtension(f), ".json") == 0 ||
											string.Compare(Path.GetExtension(f), ".yml")  == 0 ||
											string.Compare(Path.GetExtension(f), ".txt")  == 0)
				//.Where(f => f.DirectoryName != location)
				.Select(f => MoveLibrary(f.Substring(location.Length + 1)))
				.ToArray();
		}

		static string[] ReadDirectories(string location) {
			return Directory
				.EnumerateDirectories(location, "*", SearchOption.AllDirectories)
				.Where(d => !d.StartsWith(Path.Combine(location, "Logs"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(d => !d.StartsWith(Path.Combine(location, "Temp"),
					StringComparison.InvariantCultureIgnoreCase))
				.Where(d => !d.StartsWith(Path.Combine(location, "libraries"),
					StringComparison.InvariantCultureIgnoreCase))
				.Select(d => d.Substring(location.Length + 1))
				.ToArray();
		}

		static bool IsLibrary(string file) {
			string ext = Path.GetExtension(file).ToLower();
			return ext == ".dll" || ext == ".pdb";
		}
		static string MoveLibrary(string file) {
			string dir = Path.GetDirectoryName(file);
			if (dir == "" && IsLibrary(file)) {
				return Path.Combine("libraries", file);
			}
			return file;
		}
	}
}
