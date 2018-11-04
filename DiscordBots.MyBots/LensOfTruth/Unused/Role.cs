using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {
	/// <summary>
	/// The base database model for a guild role.
	/// </summary>
	public class Role : IDbRole {
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		/// <summary>
		/// The End User (guild) Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserGuildDataId { get; set; }

		/// <summary>
		/// The guild snowflake Id.
		/// </summary>
		[NotMapped]
		public ulong GuildId {
			get => EndUserGuildDataId;
			set => EndUserGuildDataId = value;
		}

		/// <summary>
		/// Checks if the user has asked this information to be deleted.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		public bool ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.Contains("role");
		}


		/// <summary>
		/// Gets the entity type of this Discord model.
		/// </summary>
		[NotMapped]
		public EntityType Type => EntityType.Role;

		/// <summary>
		/// True if the role can be assigned by other users.
		/// </summary>
		public bool IsPublicRole { get; set; }

		/// <summary>
		/// The guild assocaited with this role.
		/// </summary>
		[ForeignKey(nameof(GuildId))]
		public Guild Guild { get; set; }
	}
}
