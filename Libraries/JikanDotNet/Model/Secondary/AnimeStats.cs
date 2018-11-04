using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JikanDotNet {
	public class AnimeStats {
		
		[JsonProperty(PropertyName = "days_watched")]
		public float DaysWatched { get; set; }

		[JsonProperty(PropertyName = "mean_score")]
		public float MeanScore { get; set; }

		[JsonProperty(PropertyName = "watching")]
		public int Watching { get; set; }

		[JsonProperty(PropertyName = "completed")]
		public int Completed { get; set; }

		[JsonProperty(PropertyName = "on_hold")]
		public int OnHold { get; set; }

		[JsonProperty(PropertyName = "dropped")]
		public int Dropped { get; set; }

		[JsonProperty(PropertyName = "plan_to_watch")]
		public int PlanToWatch { get; set; }

		[JsonProperty(PropertyName = "total_entries")]
		public int TotalEntries { get; set; }

		[JsonProperty(PropertyName = "rewatched")]
		public int Rewatched { get; set; }

		[JsonProperty(PropertyName = "episodes_watched")]
		public int EpisodesWatched { get; set; }
	}
}
