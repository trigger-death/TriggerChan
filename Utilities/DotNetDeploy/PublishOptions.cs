using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetDeploy {
	public class PublishOptions {
		[JsonProperty(PropertyName = "output")]
		public string Output { get; private set; }
		[JsonProperty(PropertyName = "project")]
		public string Project { get; private set; }
		[JsonProperty(PropertyName = "configuration")]
		public string Configuration { get; private set; }
		[JsonProperty(PropertyName = "framework")]
		public string Framework { get; private set; }
		[JsonProperty(PropertyName = "runtime")]
		public string Runtime { get; private set; }
	}
}
