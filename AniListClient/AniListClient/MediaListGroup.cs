using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class MediaListGroup {
		[JsonProperty(PropertyName = "entries")]
		public List<MediaList> Entries { get; set; }

		[JsonProperty(PropertyName = "status")]
		public MediaListStatus Status { get; set; }
	}
}
