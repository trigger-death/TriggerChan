using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class ListScoreStats {
		[JsonProperty(PropertyName = "meanScore")]
		public int? MeanScore { get; set; }

		[JsonProperty(PropertyName = "standardDeviation")]
		public int? StandardDeviation { get; set; }
	}
}
