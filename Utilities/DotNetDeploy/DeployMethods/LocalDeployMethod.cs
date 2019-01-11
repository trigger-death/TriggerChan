using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetDeploy {
	public class LocalDeployMethod : IDeployMethod {
		#region Fields

		#endregion

		#region Constructors

		public LocalDeployMethod() { }
		public LocalDeployMethod(DeployOptions deploy) { }

		#endregion

		#region IDeployMethod Implementation

		public string Combine(string path1, string path2) {
			return Path.Combine(path1, path2);
		}
		public string Combine(params string[] paths) {
			return Path.Combine(paths);
		}
		public IEnumerable<FileData> Enumerate(string dir) {
			dir = dir.Replace('\\', '/');
			return new DirectoryInfo(dir).EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
										 .Select(f => new FileData(f, dir));
		}
		public IEnumerable<FileData> EnumerateDirectories(string dir) {
			dir = dir.Replace('\\', '/');
			return new DirectoryInfo(dir).EnumerateDirectories("*", SearchOption.AllDirectories)
										 .Select(f => new FileData(f, dir));
		}
		public IEnumerable<FileData> EnumerateFiles(string dir) {
			dir = dir.Replace('\\', '/');
			return new DirectoryInfo(dir).EnumerateFiles("*", SearchOption.AllDirectories)
										 .Select(f => new FileData(f, dir));
		}
		public DateTime GetLastWriteTimeUtc(string file) {
			return File.GetLastWriteTimeUtc(file);
		}
		public void DeleteFile(string file) {
			File.Delete(file);
		}
		public void DeleteDirectory(string dir) {
			Directory.Delete(dir);
		}
		public void CopyFile(string srcFile, string dstFile) {
			File.Copy(srcFile, dstFile, true);
		}
		public bool Exists(string path) {
			return File.Exists(path) || Directory.Exists(path);
		}
		public bool FileExists(string file) {
			return File.Exists(file);
		}
		public bool DirectoryExists(string dir) {
			return Directory.Exists(dir);
		}
		public void CreateDirectory(string dir) {
			Directory.CreateDirectory(dir);
		}
		private const string CommandPattern = @"^\s*(?'name'""(?:[^""\\]|\\.)*""|(?:[^""\\\s]|\\.)+)(?:\s+(?'args'.*))?$";
		private static readonly Regex CommandMatch = new Regex(CommandPattern);
		public void Execute(string command) {
			Match match = CommandMatch.Match(command);
			if (!match.Success)
				throw new ArgumentException("Invalid command!");
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = match.Groups["name"].Value,
				Arguments = match.Groups["args"].Value,
				UseShellExecute = true,
			};
			Process.Start(startInfo).Dispose();
		}

		#endregion

		#region IDisposable Implementation

		public void Dispose() { }

		#endregion
	}
}
