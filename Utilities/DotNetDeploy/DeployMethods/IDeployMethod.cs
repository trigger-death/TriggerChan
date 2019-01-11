using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDeploy {
	public interface IDeployMethod : IDisposable {
		string Combine(string path1, string path2);
		string Combine(params string[] paths);
		IEnumerable<FileData> Enumerate(string dir);
		IEnumerable<FileData> EnumerateDirectories(string dir);
		IEnumerable<FileData> EnumerateFiles(string dir);
		DateTime GetLastWriteTimeUtc(string file);
		void DeleteFile(string file);
		void DeleteDirectory(string dir);
		void CopyFile(string srcFile, string dstFile);
		bool Exists(string path);
		bool FileExists(string file);
		bool DirectoryExists(string dir);
		void CreateDirectory(string dir);
		void Execute(string command);
	}
}
