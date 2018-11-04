using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JikanDotNet {
	public class AnimeListItem {
		
		[JsonProperty(PropertyName = "mal_id")]
		public int MALId { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "video_url")]
		public string VideoUrl { get; set; }

		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		[JsonProperty(PropertyName = "image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "watching_status")]
		public int WatchingStatus { get; set; }

		[JsonProperty(PropertyName = "score")]
		public float Score { get; set; }

		[JsonProperty(PropertyName = "watched_episodes")]
		public int WatchedEpisodes { get; set; }

		[JsonProperty(PropertyName = "total_episodes")]
		public int TotalEpisodes { get; set; }

		[JsonProperty(PropertyName = "airing_status")]
		public int AiringStatus { get; set; }

		[JsonProperty(PropertyName = "season_name")]
		public string SeasonName { get; set; }

		[JsonProperty(PropertyName = "season_year")]
		public string SeasonYear { get; set; }

		[JsonProperty(PropertyName = "has_episode_video")]
		public bool HasEpisodeVideo { get; set; }

		[JsonProperty(PropertyName = "has_promo_video")]
		public bool HasPromoVideo { get; set; }

		[JsonProperty(PropertyName = "has_video")]
		public bool HasVideo { get; set; }

		[JsonProperty(PropertyName = "is_rewatching")]
		public bool IsRewatching { get; set; }

		[JsonProperty(PropertyName = "tags")]
		public string Tags { get; set; }

		[JsonProperty(PropertyName = "rating")]
		public string Rating { get; set; }

		[JsonProperty(PropertyName = "start_date")]
		public DateTime StartDate { get; set; }

		[JsonProperty(PropertyName = "end_date")]
		public DateTime EndDate { get; set; }

		[JsonProperty(PropertyName = "watch_start_date")]
		public DateTime WatchStartDate { get; set; }

		[JsonProperty(PropertyName = "watch_end_date")]
		public DateTime WatchEndDate { get; set; }

		//[JsonProperty(PropertyName = "days")]
		//public string Days { get; set; }
	}
}
