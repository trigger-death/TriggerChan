using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Model {
	/// <summary>
	/// The primary anime list used by the user.
	/// </summary>
	/*public enum AnimeDatabase {
		MyAnimeList,
		AniList,
		Kitsu,
	}*/
	public enum DatabaseService {
		MyAnimeList,
		AniList,
		Kitsu,
		VNDb,
		MyFigureCollection,
	}
	/*public enum MediaType {
		None = 0,
		Anime = (1 << 0),
		Manga = (1 << 1),
		LightNovel = (1 << 2),
		VisualNovel = (1 << 3),
	}
	public enum AnimeListType {
		Anime,
		Manga,
	}
	public enum MerchListType {
		Figures,
		Goods,
		Media,
	}
	public enum ListType {
		Anime,
		Manga,
		VisualNovels,
		Figures,
		Goods,
		Media,
	}*/


	/// <summary>
	/// User profile information.
	/// </summary>
	public class UserProfile : IDbUser, IDbEndUserData {
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }

		/// <summary>
		/// The End User Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserDataId {
			get => Id;
			set => Id = value;
		}

		/// <summary>
		/// Checks if the user has asked this information to be deleted.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		public bool ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.Contains("profile");
		}


		/// <summary>
		/// Gets the entity type of this Discord model.
		/// </summary>
		[NotMapped]
		public EntityType Type => EntityType.User;

		#region Personal

		/// <summary>
		/// The user's gender.
		/// </summary>
		[Encrypted]
		public string Gender { get; set; }
		/// <summary>
		/// The bio information about the user.
		/// </summary>
		[Encrypted]
		public string Bio { get; set; }

		#endregion

		#region Profiles

		/// <summary>
		/// Gets the user's primary anime list source.
		/// </summary>
		public DatabaseService? PrimaryAnimeList { get; set; }
		/// <summary>
		/// The user's MyAnimeList profile name.
		/// </summary>
		[Encrypted]
		public string MALUsername { get; set; }
		/// <summary>
		/// The user's AniList profile name.
		/// </summary>
		[Encrypted]
		public string AniListUsername { get; set; }
		/// <summary>
		/// The user's Kitsu profile name.
		/// </summary>
		[Encrypted]
		public string KitsuUsername { get; set; }
		/// <summary>
		/// The user's VNdb profile name.
		/// </summary>
		[Encrypted]
		public string VNdbUsername { get; set; }
		/// <summary>
		/// The user's MyFigureCollection profile name.
		/// </summary>
		[Encrypted]
		public string MFCUsername { get; set; }

		#endregion

		#region Location

		/// <summary>
		/// The bio information about the user.
		/// </summary>
		[Encrypted]
		public DateTime? Birthday { get; set; }
		/// <summary>
		/// The user's time zone.
		/// </summary>
		[Encrypted]
		public TimeZoneInfo TimeZone { get; set; }
		/// <summary>
		/// The country the user is from.
		/// </summary>
		[Encrypted]
		public string Country { get; set; }

		#endregion

		// This mispelling is an inside joke
		#region Tasts

		// TODO: Make this a list
		/// <summary>
		/// The user's shit-tier waifu list.
		/// </summary>
		[Encrypted]
		public StringSet Waifu { get; set; } = new StringSet();

		#endregion

		/// <summary>
		/// The end user data container.
		/// </summary>
		//[ForeignKey(nameof(EndUserDataId))]
		//public User User { get; set; }
	}
}
