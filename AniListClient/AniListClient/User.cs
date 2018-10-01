using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class User {
		[JsonProperty(PropertyName = "avatar")]
		public UserAvatar Avatar { get; set; }

		[JsonProperty(PropertyName = "stats")]
		public UserStats Stats { get; set; }

		[JsonProperty(PropertyName = "siteUrl")]
		public string SiteUrl { get; set; }
	}
}
