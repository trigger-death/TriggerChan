using System;
using System.Collections.Generic;
using System.Text;
using AniListClient.Internal.Converters;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum MediaListStatus {
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
