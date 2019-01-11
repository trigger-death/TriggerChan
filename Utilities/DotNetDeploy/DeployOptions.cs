using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WinSCP;

namespace DotNetDeploy {
	public enum DeployMode {
		Local,
		//LocalWSL,
		Sftp,
	}
	public class DeployOptions {
		[JsonProperty(PropertyName = "mode")]
		public DeployMode Mode { get; private set; }
		[JsonProperty(PropertyName = "config_destination")]
		public string ConfigDestination { get; private set; }
		[JsonProperty(PropertyName = "destination")]
		public string Destination { get; private set; }
		//[JsonProperty(PropertyName = "move_libs")]
		//public bool MoveLibraries { get; private set; }

		[JsonProperty(PropertyName = "sftp_session")]
		public SessionOptions SftpSession { get; private set; }

		[JsonProperty(PropertyName = "pre_deploy")]
		public string[] PreCommands { get; private set; } = new string[0];
		[JsonProperty(PropertyName = "post_deploy")]
		public string[] PostCommands { get; private set; } = new string[0];

		public IDeployMethod BuildMethod() {
			switch (Mode) {
			case DeployMode.Local:
				return new LocalDeployMethod(this);
			//case DeployMode.LocalWSL:
			//	return new LocalWSLDeployMethod(this);
			case DeployMode.Sftp:
				return new SftpDeployMethod(this);
			}
			throw new ArgumentException();
		}
	}
}
