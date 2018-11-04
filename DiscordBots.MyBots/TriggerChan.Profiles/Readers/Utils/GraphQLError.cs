using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Utils {
	[JsonObject]
	internal class GraphQLError {
		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }
		[JsonProperty(PropertyName = "status")]
		public int Status { get; set; }
		[JsonProperty(PropertyName = "locations")]
		public List<GraphQLErrorLocation> Locations { get; set; }
	}
	internal class GraphQLErrorLocation {
		[JsonProperty(PropertyName = "line")]
		public int Line { get; }
		[JsonProperty(PropertyName = "column")]
		public int Column { get; }
	}
}
