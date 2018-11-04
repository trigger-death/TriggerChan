using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {
	/// <summary>
	/// The base database model for a user DM.
	/// </summary>
	public class DM : IDbDM {
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		/// <summary>
		/// The snowflake Id of the user this DM belongs to.
		/// </summary>
		[Required]
		public ulong UserId { get; set; }
		/// <summary>
		/// The End User Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserDataId {
			get => UserId;
			set => UserId = value;
		}

		/// <summary>
		/// The commands that have been locked for this guild.
		/// </summary>
		public StringSet LockedCommands { get; set; } = new StringSet();
		/// <summary>
		/// The modules that have been locked for this guild.
		/// </summary>
		public StringSet LockedModules { get; set; } = new StringSet();
		/// <summary>
		/// The custom prefix assigned to this context.
		/// </summary>
		public string Prefix { get; set; }

		/// <summary>
		/// The user container.
		/// </summary>
		[ForeignKey(nameof(UserId))]
		public User User { get; set; }

		/// <summary>
		/// Checks if the user has asked this information to be deleted.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		public bool ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.EraseAll;
		}
		
		/// <summary>
		/// Gets the entity type of this Discord model.
		/// </summary>
		[NotMapped]
		public EntityType Type => EntityType.DM;
	}
}
