using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonConverter(typeof(JsonStringEnumConverter))]
	internal enum MediaListStatus {
		[JsonProperty(PropertyName = "CURRENT")]
		Current,
		[JsonProperty(PropertyName = "PLANNING")]
		Planning,
		[JsonProperty(PropertyName = "COMPLETED")]
		Completed,
		[JsonProperty(PropertyName = "DROPPED")]
		Dropped,
		[JsonProperty(PropertyName = "PAUSED")]
		Paused,
		[JsonProperty(PropertyName = "REPEATING")]
		Repeating,
	}
}
