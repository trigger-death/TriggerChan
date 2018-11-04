using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class ListScoreStats {

		[JsonProperty(PropertyName = "meanScore")]
		public int? MeanScore { get; set; }

		[JsonIgnore]
		public int MeanScore100 => MeanScore ?? 0;

		[JsonIgnore]
		public double MeanScore10 => MeanScore100 / 10d;
	}
}
