using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class MediaListGroup {
		[JsonProperty(PropertyName = "entries")]
		public List<MediaList> Entries { get; set; }

		/*[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "isCustomList")]
		public bool? IsCustomList { get; set; }

		[JsonProperty(PropertyName = "isSplitCompletedList")]
		public bool? IsSplitCompletedList { get; set; }*/

		[JsonProperty(PropertyName = "status")]
		public MediaListStatus? Status { get; set; }
	}
}
