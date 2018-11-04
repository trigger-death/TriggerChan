using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList {
	[JsonObject]
	internal class MediaListCollection {
		[JsonProperty(PropertyName = "lists")]
		public List<MediaListGroup> Lists { get; set; }

		[JsonIgnore]
		public int Current {
			get => Lists?.Where(l => l.Status == MediaListStatus.Current).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Completed {
			get => Lists?.Where(l => l.Status == MediaListStatus.Completed).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Paused {
			get => Lists?.Where(l => l.Status == MediaListStatus.Paused).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Dropped {
			get => Lists?.Where(l => l.Status == MediaListStatus.Dropped).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Planning {
			get => Lists?.Where(l => l.Status == MediaListStatus.Planning).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Repeating {
			get => Lists?.Where(l => l.Status == MediaListStatus.Repeating).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}
		[JsonIgnore]
		public int Other {
			get => Lists?.Where(l => !l.Status.HasValue).Sum(l => l.Entries?.Count ?? 0) ?? 0;
		}

		[JsonIgnore]
		public int Episodes {
			get => Lists?.SelectMany(l => l.Entries ?? Enumerable.Empty<MediaList>()).Sum(e => e.Progress ?? 0) ?? 0;
		}

		[JsonIgnore]
		public int Volumes {
			get => Lists?.SelectMany(l => l.Entries ?? Enumerable.Empty<MediaList>()).Sum(e => e.ProgressVolumes ?? 0) ?? 0;
		}
	}
}
