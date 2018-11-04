using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class MediaList {
		[JsonProperty(PropertyName = "progress")]
		public int? Progress { get; set; }

		[JsonProperty(PropertyName = "progressVolumes")]
		public int? ProgressVolumes { get; set; }
	}
}
