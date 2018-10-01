using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class ListScoreStats {

		[JsonProperty(PropertyName = "meanScore")]
		public int MeanScore100 { get; set; }

		[JsonIgnore]
		public double MeanScore10 {
			get => MeanScore100 / 10d;
			set => MeanScore100 = (int) (value * 10);
		}
	}
}
