using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The primary anime list used by the user.
	/// </summary>
	/*public enum AnimeDatabase {
		MyAnimeList,
		AniList,
		Kitsu,
	}*/
	public enum DatabaseType {
		MyAnimeList,
		AniList,
		Kitsu,
		VNDb,
		MyFigureCollection,
	}
	public enum MediaType {
		None = 0,
		Anime = (1 << 0),
		Manga = (1 << 1),
		LightNovel = (1 << 2),
		VisualNovel = (1 << 3),
	}
	/*public enum AnimeListType {
		Anime,
		Manga,
	}
	public enum MerchListType {
		Figures,
		Goods,
		Media,
	}*/
	public enum ListType {
		Anime,
		Manga,
		VisualNovels,
		Figures,
		Goods,
		Media,
	}

	[Flags]
	public enum InfoLevel {
		Compact = 0,
		All,
	}

	public enum ListStatus {
		// Anime
		Current,
		Completed,
		OnHold,
		Dropped,
		Planning,
		Repeating,

		// Merch
		Owned,
		Ordered,
		Wished,
		Favorites,
	}

	/*public static class ProfileNamingExtensions {

		private static readonly Dictionary<ListType, string> TypeNames =
			new Dictionary<ListType, string> {
			{ ListType.Anime, "Anime" },
			{ ListType.Manga, "Manga" },
			{ ListType.VisualNovels, "Visual Novels" },
			{ ListType.Figures, "Figures" },
			{ ListType.Goods, "Goods" },
			{ ListType.Media, "Media" },
		};
		private static readonly Dictionary<ListType, Dictionary<ListStatus, string>> StatusNames =
			new Dictionary<ListType, Dictionary<ListStatus, string>> {
			{
				ListType.Anime,
				new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Watching" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Watch" },
					{ ListStatus.Repeating, "Rewatching" },
				}
			},
			{
				ListType.Manga,
				new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Reading" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Read" },
					{ ListStatus.Repeating, "Rereading" },
				}
			},
			{
				ListType.VisualNovels,
				new Dictionary<ListStatus, string> {
					{ ListStatus.Current, "Playing" },
					{ ListStatus.Completed, "Completed" },
					{ ListStatus.OnHold, "On-Hold" },
					{ ListStatus.Dropped, "Dropped" },
					{ ListStatus.Planning, "Plan to Play" },
					{ ListStatus.Repeating, "Replaying" },
				}
			},
			{ ListType.Figures, MerchStatusNames },
			{ ListType.Goods, MerchStatusNames },
			{ ListType.Media, MerchStatusNames },
		};
		private static readonly Dictionary<ListStatus, string> MerchStatusNames =
			new Dictionary<ListStatus, string> {
			{ ListStatus.Owned, "Owned" },
			{ ListStatus.Ordered, "Ordered" },
			{ ListStatus.Wished, "Wished" },
			{ ListStatus.Favorites, "Favorites" },
		};
		private static readonly Dictionary<ListType, string> RepeatedNames =
			new Dictionary<ListType, string> {
			{ ListType.Anime, "Rewatched" },
			{ ListType.Manga, "Reread" },
			{ ListType.VisualNovels, "Replayed" },
		};

		public static string ToName(this ListType type) {
			return TypeNames[type];
		}
		public static string ToStatusName(this ListType type, ListStatus status) {
			return StatusNames[type][status];
		}
		public static string ToRepeatedName(this ListType type) {
			return RepeatedNames[type];
		}


	}*/
}
