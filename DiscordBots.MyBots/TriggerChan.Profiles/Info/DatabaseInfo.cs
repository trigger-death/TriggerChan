using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	public class DatabaseInfo {
		public DatabaseType DatabaseType { get; set; }
		public string FullName { get; set; }
		public string ShortName { get; set; }
		public string DefaultName => (ShortNameIsDefault ? ShortName : FullName);
		public bool ShortNameIsDefault { get; set; }
		public string UrlName { get; set; }
		public string Url { get; set; }
		public string ImageUrl { get; set; }
		public Color Color { get; set; }
		//public string RepeatedName { get; set; }
		//public Dictionary<ListStatus, string> StatusNames { get; set; }
	}

	public static class DatabaseTypeExtensions {

		private static readonly Dictionary<DatabaseType, DatabaseInfo> Info = new Dictionary<DatabaseType, DatabaseInfo>();

		static DatabaseTypeExtensions() {
			AddDatabaseInfo(new DatabaseInfo {
				DatabaseType = DatabaseType.MyAnimeList,
				FullName = "MyAnimeList",
				ShortName = "MAL",
				ShortNameIsDefault = true,
				UrlName = "MyAnimeList.net",
				Url = @"https://myanimelist.net/",
				ImageUrl = @"https://i.imgur.com/DUikkqZ.png",
				Color = new Color(46, 81, 162),
			});
			AddDatabaseInfo(new DatabaseInfo {
				DatabaseType = DatabaseType.AniList,
				FullName = "AniList",
				ShortName = "Ani",
				ShortNameIsDefault = false,
				UrlName = "AniList.co",
				Url = @"https://anilist.co/",
				ImageUrl = @"https://i.imgur.com/p7K6gAR.png",
				Color = new Color(2, 169, 255),
			});
			AddDatabaseInfo(new DatabaseInfo {
				DatabaseType = DatabaseType.Kitsu,
				FullName = "Kitsu",
				ShortName = "Kitsu",
				ShortNameIsDefault = false,
				UrlName = "Kitsu.io",
				Url = @"https://kitsu.io/",
				ImageUrl = @"https://i.imgur.com/TFxzIX5.png",
				Color = new Color(247, 81, 57),
				//Color = new Color(50, 38, 50),
			});
			AddDatabaseInfo(new DatabaseInfo {
				DatabaseType = DatabaseType.VNDb,
				FullName = "The Visual Novel Database",
				ShortName = "VNDb",
				ShortNameIsDefault = true,
				UrlName = "VNDb.org",
				Url = @"https://vndb.org/",
				ImageUrl = @"https://i.imgur.com/jaXF2bv.png",
				Color = new Color(17, 34, 51),
			});
			AddDatabaseInfo(new DatabaseInfo {
				DatabaseType = DatabaseType.MyFigureCollection,
				FullName = "MyFigureCollection",
				ShortName = "MFC",
				ShortNameIsDefault = true,
				UrlName = "MyFigureCollection.net",
				Url = @"https://myfigurecollection.net/",
				ImageUrl = @"https://i.imgur.com/EjFScAj.png",
				Color = new Color(87, 161, 82),
			});
		}

		private static void AddDatabaseInfo(DatabaseInfo databaseInfo) {
			Info.Add(databaseInfo.DatabaseType, databaseInfo);
		}

		public static DatabaseInfo ToInfo(this DatabaseType type) {
			return Info[type];
		}
	}
}
