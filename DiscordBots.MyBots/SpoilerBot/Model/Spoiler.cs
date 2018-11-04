using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {
	/// <summary>
	/// A user-created spoiler message. Reacting sends a DM message with the context.
	/// </summary>
	public class Spoiler : IDbEndUserData {
		/// <summary>
		/// The Id of the spoiler message that the bot created.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong MessageId { get; set; }
		/// <summary>
		/// The Id of the author of this spoiler.
		/// </summary>
		[Required]
		public ulong AuthorId { get; set; }
		/// <summary>
		/// The Id of the channel the spoiler was created in.
		/// </summary>
		[Required]
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The Id of the guild the spoiler was created in.
		/// </summary>
		[Required]
		public ulong GuildId { get; set; }
		/// <summary>
		/// The End User Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserDataId {
			get => AuthorId;
			set => AuthorId = value;
		}

		/// <summary>
		/// The entire content of the message command.<para/>
		/// This is returned to the user when they delete their spoiler.
		/// </summary>
		[Encrypted]
		public string Command { get; set; }
		/// <summary>
		/// The title of this spoiler.
		/// </summary>
		[Encrypted]
		public string Title { get; set; }
		/// <summary>
		/// The content of this spoiler.
		/// </summary>
		[Encrypted]
		public string Content { get; set; }
		/// <summary>
		/// The time when the message was sent.
		/// </summary>
		public DateTime TimeStamp { get; set; }

		/// <summary>
		/// Gets the attachment Url.
		/// </summary>
		[Encrypted]
		public string AttachmentUrl { get; set; }
		/// <summary>
		/// Gets the Urls attached to the spoiler content.
		/// </summary>
		[Encrypted]
		public StringList Urls { get; set; } = new StringList();

		/// <summary>
		/// True if the spoiler is displayed in raw format.
		/// </summary>
		public bool RawDisplay { get; set; }

		/// <summary>
		/// Gets the embed color of the spoiler.
		/// </summary>
		//[ConverterNullableDiscordColor]
		//public Color? EmbedColor { get; set; }


		/// <summary>
		/// Checks if the user has asked this information to be deleted.<para/>
		/// This method is only called once per table. The result should NOT change based on the data.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		bool IDbEndUserDataBase.ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.Contains("spoiler");
		}

		/// <summary>
		/// The list of spoiled users.
		/// </summary>
		[InverseProperty(nameof(SpoiledUser.Spoiler))]
		public List<SpoiledUser> SpoiledUsers { get; set; }
		/*/// <summary>
		/// The user container.
		/// </summary>
		[ForeignKey(nameof(AuthorId))]
		public SpoilerUser User { get; set; }*/
		
		/// <summary>
		/// The original command context. (Only used when creating the spoiler)
		/// </summary>
		[NotMapped]
		public DiscordBotCommandContext Context { get; set; }
	}

	/// <summary>
	/// The database model for a user that has been spoiled to a specific spoiler.
	/// </summary>
	public class SpoiledUser : IDbEndUserData, IDbModelCreating {
		/// <summary>
		/// The Id of the spoiler message that the user was spoiled to.
		/// </summary>
		[Key, Column(Order = 1)]
		public ulong MessageId { get; set; }
		/// <summary>
		/// The Id of the user that was spoiled.
		/// </summary>
		[Key, Column(Order = 2)]
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
		/// The Id of the 1st message that was created in DM when the user was spoiled.
		/// </summary>
		public ulong DMMessageIdA { get; set; }
		/// <summary>
		/// The Id of the 2nd message that was created in DM when the user was spoiled.
		/// </summary>
		public ulong DMMessageIdB { get; set; }
		/// <summary>
		/// The Id of the 3rd message that was created in DM when the user was spoiled.
		/// </summary>
		public ulong DMMessageIdC { get; set; }


		/// <summary>
		/// Checks if the user has asked this information to be deleted.<para/>
		/// This method is only called once per table. The result should NOT change based on the data.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		bool IDbEndUserDataBase.ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.Contains("spoiler");
		}

		/// <summary>
		/// The spoiler this spoiled user relates to.
		/// </summary>
		[ForeignKey(nameof(MessageId))]
		public Spoiler Spoiler { get; set; }
		/*/// <summary>
		/// The user container.
		/// </summary>
		[ForeignKey(nameof(UserId))]
		public SpoilerUser User { get; set; }*/

		public void ModelCreating(ModelBuilder modelBuilder, DbContextEx db) {
			modelBuilder
				.Entity<SpoiledUser>()
				.HasKey(s => new { s.MessageId, s.UserId });
		}
	}
}
