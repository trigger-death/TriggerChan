using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class UserAvatar {
		[JsonProperty(PropertyName = "medium")]
		public string Medium { get; set; }

		[JsonProperty(PropertyName = "large")]
		public string Large { get; set; }
	}
}
