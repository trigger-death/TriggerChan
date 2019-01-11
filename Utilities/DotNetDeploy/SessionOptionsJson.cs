using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WinSCP;

namespace DotNetDeploy {
	public static class SessionOptionsJson {
		
		public static SessionOptions Read(string filePath) {
			string json = File.ReadAllText(filePath);
			SessionOptions options = JsonConvert.DeserializeObject<SessionOptions>(json);
			options.Protocol = Protocol.Sftp;
			if (options.SshPrivateKeyPath != null)
				options.SshPrivateKeyPath = Path.GetFullPath(options.SshPrivateKeyPath);
			if (options.TlsClientCertificatePath != null)
				options.TlsClientCertificatePath = Path.GetFullPath(options.TlsClientCertificatePath);
			return options;
		}
	}
}
