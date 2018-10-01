using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient {
	[JsonObject]
	public class MediaListCollection {
		[JsonProperty(PropertyName = "lists")]
		public List<MediaListGroup> Lists { get; set; }

		[JsonIgnore]
		public int Completed {
			get {
				foreach (MediaListGroup list in Lists) {
					if (list.Status == MediaListStatus.Completed)
						return list.Entries.Count;
				}
				return 0;
			}
		}

		[JsonIgnore]
		public int Episodes {
			get {
				int count = 0;
				foreach (MediaListGroup list in Lists) {
					count += list.Entries.Sum(ml => ml.Progress);
				}
				return count;
			}
		}

		[JsonIgnore]
		public int Volumes {
			get {
				int count = 0;
				foreach (MediaListGroup list in Lists) {
					count += list.Entries.Sum(ml => ml.ProgressVolumes);
				}
				return count;
			}
		}
	}
}
