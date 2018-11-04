using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {

	public enum PrimaryAnimeList {
		Unknown = 0,
		MyAnimeList,
		MAL = MyAnimeList,
		AniList,
		Ani = AniList,
		Kitsu,
		Kit = Kitsu,
	}
	public enum MediaType {
		None,
		Anime = (1 << 0),
		Manga = (1 << 1),
		LightNovel = (1 << 2),
		VisualNovel = (1 << 3),
	}


	/// <summary>
	/// User profile information.
	/// </summary>
	public class UserProfile : IDbUser, IDbEndUserData {
		/// <summary>
		/// The 128-bit guild snowflake enity Id as a key.
		/// </summary>
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		/// <summary>
		/// The End User Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserDataId { get => Id; set => Id = value; }

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
		[EncryptedString]
		public string Gender { get; set; }
		/// <summary>
		/// The bio information about the user.
		/// </summary>
		[EncryptedString]
		public string Bio { get; set; }

		#endregion

		#region Profiles

		/// <summary>
		/// Gets the user's primary anime list source.
		/// </summary>
		public PrimaryAnimeList PrimaryAnimeList { get; set; }
		/// <summary>
		/// The user's MyAnimeList profile name.
		/// </summary>
		[EncryptedString]
		public string MALUsername { get; set; }
		/// <summary>
		/// The user's AniList profile name.
		/// </summary>
		[EncryptedString]
		public string AniListUsername { get; set; }
		/// <summary>
		/// The user's Kitsu profile name.
		/// </summary>
		[EncryptedString]
		public string KitsuUsername { get; set; }
		/// <summary>
		/// The user's MyFigureCollection profile name.
		/// </summary>
		[EncryptedString]
		public string MFCUsername { get; set; }

		#endregion

		#region Location

		/// <summary>
		/// The bio information about the user.
		/// </summary>
		[EncryptedDateTimeNullable]
		public DateTime? Birthday { get; set; }
		/// <summary>
		/// The user's time zone.
		/// </summary>
		[EncryptedTimeZone]
		public TimeZoneInfo TimeZone { get; set; }
		/// <summary>
		/// The country the user is from.
		/// </summary>
		[EncryptedString]
		public string Country { get; set; }

		#endregion

		#region Tasts

		// TODO: Make this a list
		/// <summary>
		/// The user's shit tier waifu.
		/// </summary>
		[EncryptedString]
		public string Waifu { get; set; }

		#endregion

		/// <summary>
		/// The end user data container.
		/// </summary>
		[ForeignKey(nameof(EndUserDataId))]
		public User User { get; set; }
	}
}
