using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	/// <summary>
	/// The interface for the non-generic spoiler service.
	/// </summary>
	public partial interface ISpoilerService {
		#region Run Spoiler

		/// <summary>
		/// Runs the spoiler command from the <see cref="Modules.SpoilerModule"/>.
		/// </summary>
		/// <param name="context">The context of the spoiler command.</param>
		/// <param name="content">The content of the spoiler command.</param>
		/// <param name="waitForAttachment">True if the spoiler is an upload spoiler.</param>
		/// <param name="raw">True if the spoiler is a raw spoiler by default.</param>
		Task<RuntimeResult> RunSpoilerAsync(DiscordBotCommandContext context, string content, bool waitForAttachment, bool raw);

		#endregion

		#region Public Edit Spoiler

		/// <summary>
		/// Deletes the spoiler with the specified Id. Optionally DMs the spoiler command to the user.
		/// </summary>
		/// <param name="id">The Id of the spoiler.</param>
		/// <param name="dmCommand">True if the executed command should be DMed to the user.</param>
		Task<bool> DeleteSpoilerAsync(ulong id, bool dmCommand = false);

		/// <summary>
		/// Renames the spoiler's title.
		/// </summary>
		/// <param name="db">The database to work with.</param>
		/// <param name="spoiler">The spoiler to rename.</param>
		/// <param name="newTitle">The new title.</param>
		Task RenameSpoilerAsync(DbContextEx db, Spoiler spoiler, string newTitle);

		#endregion

		#region Public Database Access

		/// <summary>
		/// Finds the spoiler with the specified message Id.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <returns>The found spoiler, or null.</returns>
		Task<Spoiler> FindSpoilerAsync(ulong messageId);
		/// <summary>
		/// Finds the spoiler with the specified message Id.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <returns>The found spoiler, or null.</returns>
		Task<Spoiler> FindSpoilerAsync(DbContextEx db, ulong messageId);
		/// <summary>
		/// Finds the spoiled user with the specified message and user Id.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="userId">The user Id of the spoiled user.</param>
		/// <returns>The found spoiled user, or null.</returns>
		Task<SpoiledUser> FindSpoiledUserAsync(ulong messageId, ulong userId);
		/// <summary>
		/// Finds the spoiled user with the specified message and user Id.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="userId">The user Id of the spoiled user.</param>
		/// <returns>The found spoiled user, or null.</returns>
		Task<SpoiledUser> FindSpoiledUserAsync(DbContextEx db, ulong messageId, ulong userId);
		/// <summary>
		/// Gets the last spoiler written by the specified author in the specified channel.
		/// </summary>
		/// <param name="authorId">The Id of the author who posted the spoiler.</param>
		/// <param name="channelId">The channel the spoiler was posted in.</param>
		Task<Spoiler> FindLastSpoilerByUserAsync(ulong authorId, ulong channelId);
		/// <summary>
		/// Gets the last spoiler written by the specified author in the specified channel.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="authorId">The Id of the author who posted the spoiler.</param>
		/// <param name="channelId">The channel the spoiler was posted in.</param>
		Task<Spoiler> FindLastSpoilerByUserAsync(DbContextEx db, ulong authorId, ulong channelId);
		/// <summary>
		/// Gets the count of the number of active spoilers.
		/// </summary>
		/// <returns>The number of active spoilers.</returns>
		Task<long> GetSpoilerCountAsync();
		/// <summary>
		/// Gets the count of the number of spoiled users.
		/// </summary>
		/// <returns>The number of spoiled users.</returns>
		Task<long> GetSpoiledUserCountAsync();

		#endregion
	}
}
