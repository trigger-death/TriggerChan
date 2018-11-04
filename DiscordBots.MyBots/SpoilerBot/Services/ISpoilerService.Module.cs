using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial interface ISpoilerService {
		/// <summary>
		/// Renames the last posted spoiler by the user in this channel.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="title">The new title of the spoiler.</param>
		/// <returns>The runtime result of success react or normal.</returns>
		Task<RuntimeResult> RenameLastSpoilerAsync(ICommandContext context, string title);
		/// <summary>
		/// Renames the spoiler with the specified Id that was posted by the user.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="id">The message Id of the spoiler.</param>
		/// <param name="title">The new title of the spoiler.</param>
		/// <returns>The runtime result of success react or normal.</returns>
		Task<RuntimeResult> RenameSpoilerAsync(ICommandContext context, ulong id, string title);
		/// <summary>
		/// Removes the last posted spoiler by the user in this channel.
		/// </summary>
		/// <param name="context">The command context.</param>
		Task RemoveLastSpoilerAsync(ICommandContext context);
		/// <summary>
		/// Removes the spoiler with the specified Id that was posted by the user.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="id">The message Id of the spoiler.</param>
		Task RemoveSpoilerAsync(ICommandContext context, ulong id);
	}
}
