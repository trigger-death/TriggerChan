using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class UserAvatar {
		[JsonProperty(PropertyName = "medium")]
		public string Medium { get; set; }

		[JsonProperty(PropertyName = "large")]
		public string Large { get; set; }

		[JsonIgnore]
		public string Largest => Large ?? Medium;
	}
}
