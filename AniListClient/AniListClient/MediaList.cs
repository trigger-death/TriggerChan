using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class MediaList {
		[JsonProperty(PropertyName = "progress")]
		public int Progress;

		[JsonProperty(PropertyName = "progressVolumes")]
		public int ProgressVolumes;
	}
}
