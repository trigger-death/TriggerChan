using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class User {
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "avatar")]
		public UserAvatar Avatar { get; set; }

		[JsonProperty(PropertyName = "stats")]
		public UserStats Stats { get; set; }

		[JsonProperty(PropertyName = "siteUrl")]
		public string ProfileUrl { get; set; }
	}
}
