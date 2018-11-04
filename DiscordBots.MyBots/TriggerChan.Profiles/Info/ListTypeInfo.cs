using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	public class ListInfo {
		public ListType ListType { get; set; }
		public string Name { get; set; }
		public string ListName => $"{Name} List";
		public string RepeatedName { get; set; }
		public string FinishedName { get; set; }
		public Dictionary<ListStatus, string> StatusNames { get; set; }
	}

	public static class ListTypeExtensions {

		private static readonly Dictionary<ListType, ListInfo> Info = new Dictionary<ListType, ListInfo>();

		static ListTypeExtensions() {
			AddListInfo(new ListInfo {
				ListType = ListType.Anime,
				Name = "Anime",
				RepeatedName = "Rewatched",
				FinishedName = "Watched",
				StatusNames = new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Watching" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Watch" },
					{ ListStatus.Repeating, "Rewatching" },
				},
			});
			AddListInfo(new ListInfo {
				ListType = ListType.Manga,
				Name = "Manga",
				RepeatedName = "Reread",
				FinishedName = "Read",
				StatusNames = new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Reading" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Read" },
					{ ListStatus.Repeating, "Rereading" },
				},
			});
			AddListInfo(new ListInfo {
				ListType = ListType.VisualNovels,
				Name = "Visual Novels",
				RepeatedName = "Replayed",
				FinishedName = "Played",
				StatusNames = new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Playing" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Play" },
					{ ListStatus.Repeating, "Replaying" },
				},
			});
			Dictionary<ListStatus, string> merchStatusNames =
				new Dictionary<ListStatus, string> {
				{ ListStatus.Owned, "Owned" },
				{ ListStatus.Ordered, "Ordered" },
				{ ListStatus.Wished, "Wished" },
				{ ListStatus.Favorites, "Favorites" },
			};
			AddListInfo(new ListInfo {
				ListType = ListType.Figures,
				Name = "Figures",
				StatusNames = merchStatusNames,
			});
			AddListInfo(new ListInfo {
				ListType = ListType.Goods,
				Name = "Goods",
				StatusNames = merchStatusNames,
			});
			AddListInfo(new ListInfo {
				ListType = ListType.Media,
				Name = "Media",
				StatusNames = merchStatusNames,
			});
		}

		private static void AddListInfo(ListInfo listInfo) {
			Info.Add(listInfo.ListType, listInfo);
		}

		public static ListInfo ToInfo(this ListType type) {
			return Info[type];
		}
	}
}
