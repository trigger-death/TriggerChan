using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class UserStats {
		[JsonProperty(PropertyName = "watchedTime")]
		public int? WatchedMinutes { get; set; }

		[JsonProperty(PropertyName = "chaptersRead")]
		public int? ChaptersRead { get; set; }

		[JsonProperty(PropertyName = "animeListScores")]
		public ListScoreStats AnimeScores { get; set; }

		[JsonProperty(PropertyName = "mangaListScores")]
		public ListScoreStats MangaScores { get; set; }
	}
}
