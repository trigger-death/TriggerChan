using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace DotNetDeploy.Utils {
	public static class PathUtils {
		#region Combine

		/*public static string Combine(string path1, string path2) {
			return Path.Combine(path1, path2).Replace('\\', '/');
		}
		public static string Combine(params string[] paths) {
			return Path.Combine(paths).Replace('\\', '/');
		}*/
		public static string Combine(string path1, string path2) {
			return RemotePath.Combine(path1, path2);
		}
		public static string Combine(params string[] paths) {
			string path = paths[0];
			for (int i = 1; i < paths.Length; i++) {
				path = RemotePath.Combine(path, paths[i]);
			}
			return path;
		}

		public static string GetExtension(string file) {
			return Path.GetExtension(RemotePath.GetFileName(file));
		}

		#endregion
	}
}
