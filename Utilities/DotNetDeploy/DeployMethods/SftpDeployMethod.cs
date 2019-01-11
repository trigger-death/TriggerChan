using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace DotNetDeploy {
	public class SftpDeployMethod : IDeployMethod {
		#region Fields

		public Session Session { get; }
		public TransferOptions Transfer { get; }

		#endregion

		#region Constructors

		public SftpDeployMethod(DeployOptions deploy) {
			//Transfer = deploy.SftpTransfer;
			SessionOptions sessionOptions = deploy.SftpSession;
			string privateKey = sessionOptions.SshPrivateKeyPath;
			if (!Path.IsPathRooted(privateKey))
				sessionOptions.SshPrivateKeyPath = Path.Combine(AppContext.BaseDirectory, "deployments", "keys", privateKey);

			Transfer = new TransferOptions {
				OverwriteMode = OverwriteMode.Overwrite,
				TransferMode = TransferMode.Binary,
				PreserveTimestamp = true,
			};
			Session = new Session();
			Session.Open(deploy.SftpSession);
		}

		#endregion

		#region IDeployMethod Implementation

		public string Combine(string path1, string path2) {
			return RemotePath.Combine(path1, path2);
		}
		public string Combine(params string[] paths) {
			string path = paths[0];
			for (int i = 1; i < paths.Length; i++) {
				path = RemotePath.Combine(path, paths[i]);
			}
			return path;
		}
		public IEnumerable<FileData> Enumerate(string dir) {
			EnumerationOptions options = EnumerationOptions.AllDirectories |
										 EnumerationOptions.EnumerateDirectories;
			return Session.EnumerateRemoteFiles(dir, "*", options)
						  .Select(f => new FileData(f, dir));
		}
		public IEnumerable<FileData> EnumerateDirectories(string dir) {
			EnumerationOptions options = EnumerationOptions.AllDirectories |
										 EnumerationOptions.EnumerateDirectories;
			return Session.EnumerateRemoteFiles(dir, "*", options)
						  .Where(f => f.IsDirectory)
						  .Select(f => new FileData(f, dir));
		}
		public IEnumerable<FileData> EnumerateFiles(string dir) {
			EnumerationOptions options = EnumerationOptions.AllDirectories;
			return Session.EnumerateRemoteFiles(dir, "*", options)
						  .Where(f => !f.IsDirectory)
						  .Select(f => new FileData(f, dir));
		}
		public DateTime GetLastWriteTimeUtc(string file) {
			if (Session.FileExists(file))
				return Session.GetFileInfo(file).LastWriteTime.ToUniversalTime();
			return DateTime.MinValue;
		}
		public void DeleteFile(string file) {
			var info = Session.GetFileInfo(file);
			if (info.IsDirectory) throw new FileNotFoundException();
			var result = Session.RemoveFiles(file);
			result.Check();
		}
		public void DeleteDirectory(string dir) {
			var info = Session.GetFileInfo(dir);
			if (!info.IsDirectory) throw new DirectoryNotFoundException();
			var result = Session.RemoveFiles(dir);
			result.Check();
		}
		public void CopyFile(string srcFile, string dstFile) {
			var result = Session.PutFiles(srcFile.Replace('/', '\\'), dstFile, options: Transfer);
			result.Check();
		}
		public bool Exists(string path) {
			return Session.FileExists(path);
		}
		public bool FileExists(string file) {
			if (Session.FileExists(file))
				return !Session.GetFileInfo(file).IsDirectory;
			return false;
		}
		public bool DirectoryExists(string dir) {
			if (Session.FileExists(dir))
				return Session.GetFileInfo(dir).IsDirectory;
			return false;
		}
		public void CreateDirectory(string dir) {
			Session.CreateDirectory(dir);
		}
		public void Execute(string command) {
			var result = Session.ExecuteCommand(command);
			result.Check();
		}

		#endregion

		#region IDisposable Implementation

		public void Dispose() {
			Session.Dispose();
		}

		#endregion
	}
}
