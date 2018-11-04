using Discord;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Model {
	/// <summary>
	/// The base database model for a guild.
	/// </summary>
	public class Guild : IDbGuild, IDbCommandContext {
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		/// <summary>
		/// The End User (guild) Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserGuildDataId {
			get => Id;
			set => Id = value;
		}

		/// <summary>
		/// Checks if the user has asked this information to be deleted.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		public bool ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.EraseAll;
		}


		/*/// <summary>
		/// Gets the color used for embeds.
		/// </summary>
		[ConverterNullableDiscordColor]
		public Color? EmbedColor { get; set; }*/

		/// <summary>
		/// True if spoiler spell check is disabled.
		/// </summary>
		public bool SpellCheckSpoilers { get; set; }
		/// <summary>
		/// True if other bots are allowed to use this bot's commands.
		/// </summary>
		public bool AllowBotSpoilers { get; set; }
		/// <summary>
		/// The cooldown allowing the bot to respond to user messages talking about it.
		/// </summary>
		public TimeSpan TalkBackCooldown { get; set; }
		/// <summary>
		/// True if talkback is enabled.
		/// </summary>
		public bool TalkBackEnabled { get; set; }
		/// <summary>
		/// The number of "📌" reacts before a message is pinned.
		/// </summary>
		public int PinReactCount { get; set; }

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
		/// The snowflake Id for the role that allows users administrative access to the bot.
		/// </summary>
		public ulong ManagerRoleId { get; set; }

		public SnowflakeSet PublicRoles { get; set; } = new SnowflakeSet();

		/*/// <summary>
		/// Gets the role information in the guild.
		/// </summary>
		[InverseProperty(nameof(Role.Guild))]
		public List<Role> Roles { get; set; }*/
		/// <summary>
		/// Gets the guild emotes in the guild.
		/// </summary>
		[InverseProperty(nameof(GuildEmote.Guild))]
		public List<GuildEmote> GuildEmotes { get; set; }

		
		/// <summary>
		/// Gets the entity type of this Discord model.
		/// </summary>
		[NotMapped]
		public EntityType Type => EntityType.Guild;
	}
}
