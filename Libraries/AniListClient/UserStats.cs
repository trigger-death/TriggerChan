using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class UserStats {
		[JsonProperty(PropertyName = "watchedTime")]
		public int WatchedMinutes { get; set; }

		[JsonProperty(PropertyName = "chaptersRead")]
		public int ChaptersRead { get; set; }

		[JsonProperty(PropertyName = "animeListScores")]
		public ListScoreStats AnimeListScores { get; set; }

		[JsonProperty(PropertyName = "mangaListScores")]
		public ListScoreStats MangaListScores { get; set; }

		[JsonIgnore]
		public TimeSpan WatchedTime {
			get => TimeSpan.FromMinutes(WatchedMinutes);
			set => WatchedMinutes = (int) value.TotalMinutes;
		}
	}
}
