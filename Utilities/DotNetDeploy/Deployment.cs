using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotNetDeploy {
	public class Deployment {
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }
		[JsonProperty(PropertyName = "publish")]
		public PublishOptions Publish { get; private set; }
		[JsonProperty(PropertyName = "deploy")]
		public DeployOptions Deploy { get; private set; }
		[JsonProperty(PropertyName = "filters")]
		private string[] FilterPaths { get; set; }

		[JsonIgnore]
		public Filter BuildIgnore { get; private set; } = Filter.EmptyIgnore;
		[JsonIgnore]
		public Filter DeployIgnore { get; private set; } = Filter.EmptyIgnore;
		//[JsonIgnore]
		//public Filter ConfigIgnore { get; private set; } = Filter.EmptyIgnore;
		//[JsonIgnore]
		//public Filter ConfigInclude { get; private set; } = Filter.EmptyInclude;

		public static Deployment[] GetDeployments() {
			var files = Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "deployments"));
			List<Deployment> deployments = new List<Deployment>();
			foreach (string file in files) {
				if (Path.GetExtension(file).ToLower() != ".json")
					continue;
				try {
					string json = File.ReadAllText(file);
					Deployment deployment = JsonConvert.DeserializeObject<Deployment>(json);
					if (deployment.Deploy != null) {
						if (deployment.Name == null)
							deployment.Name = Path.GetFileName(file);
						deployments.Add(deployment);
					}
				} catch { }
			}
			deployments.Sort((a, b) => string.Compare(a.Name, b.Name, true));
			return deployments.ToArray();
		}

		public void LoadFilters() {
			for (int i = 0; i < FilterPaths.Length; i++) {
				string path = FilterPaths[i];
				if (!Path.IsPathRooted(path))
					path = Path.Combine(AppContext.BaseDirectory, "deployments", "filters", path);
				string name = Path.GetFileName(path).ToLower();
				if (name.EndsWith("build.ignore"))
					BuildIgnore.Merge(Filter.FromFile(path, false));
				else if (name.EndsWith("deploy.ignore"))
					DeployIgnore.Merge(Filter.FromFile(path, false));
				//else if (name.EndsWith("config.ignore"))
				//	ConfigIgnore.Merge(Filter.FromFile(path, false));
				//else if (name.EndsWith("config.include"))
				//	ConfigInclude.Merge(Filter.FromFile(path, true));
				else
					throw new Exception("Config file must end with: " +
										"\"build.ignore\" or \"deploy.ignore\"");//, " +
										//"\"config.ignore\", or \"config.include\"!");
										//"\"config.ignore\", or \"config.include\"!");
				FilterPaths[i] = path;
			}
		}
	}
}
