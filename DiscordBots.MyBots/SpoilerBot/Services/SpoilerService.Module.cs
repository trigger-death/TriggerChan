using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService<TDbContext> {
		/// <summary>
		/// Renames the last posted spoiler by the user in this channel.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="title">The new title of the spoiler.</param>
		/// <returns>The runtime result of success react or normal.</returns>
		public async Task<RuntimeResult> RenameLastSpoilerAsync(ICommandContext context, string title) {
			using (var db = GetDb<TDbContext>()) {
				Spoiler spoiler = await FindLastSpoilerByUserAsync(db, context.User.Id, context.Channel.Id).ConfigureAwait(false);
				if (spoiler == null)
					await context.Channel.SendMessageAsync($"**Remove Spoiler:** You have not posted any spoilers in this channel").ConfigureAwait(false);
				else {
					await RenameSpoilerAsync(db, spoiler, title).ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}
			return NormalResult.FromSuccess();
		}
		/// <summary>
		/// Renames the spoiler with the specified Id that was posted by the user.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="id">The message Id of the spoiler.</param>
		/// <param name="title">The new title of the spoiler.</param>
		/// <returns>The runtime result of success react or normal.</returns>
		public async Task<RuntimeResult> RenameSpoilerAsync(ICommandContext context, ulong id, string title) {
			using (var db = GetDb<TDbContext>()) {
				Spoiler spoiler = await FindSpoilerAsync(db, id).ConfigureAwait(false);
				if (spoiler == null)
					await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler with Id of {id} was either not found").ConfigureAwait(false);
				else if (spoiler.AuthorId != context.User.Id)
					await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler with Id of {id} is not owned by you").ConfigureAwait(false);
				else {
					await RenameSpoilerAsync(db, spoiler, title).ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}
			return NormalResult.FromSuccess();
		}
		/// <summary>
		/// Removes the last posted spoiler by the user in this channel.
		/// </summary>
		/// <param name="context">The command context.</param>
		public async Task RemoveLastSpoilerAsync(ICommandContext context) {
			Spoiler spoiler = await FindLastSpoilerByUserAsync(context.User.Id, context.Channel.Id).ConfigureAwait(false);
			if (spoiler == null)
				await context.Channel.SendMessageAsync($"**Remove Spoiler:** You have not posted any spoilers in this channel").ConfigureAwait(false);
			else {
				await DeleteSpoilerAsync(spoiler.MessageId, true).ConfigureAwait(false);
				await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler posted at {spoiler.TimeStamp:dd MMMM yyyy HH:mm:ss} UTC was deleted").ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Removes the spoiler with the specified Id that was posted by the user.
		/// </summary>
		/// <param name="context">The command context.</param>
		/// <param name="id">The message Id of the spoiler.</param>
		public async Task RemoveSpoilerAsync(ICommandContext context, ulong id) {
			Spoiler spoiler = await FindSpoilerAsync(id).ConfigureAwait(false);
			if (spoiler == null)
				await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler with Id of {id} was either not found").ConfigureAwait(false);
			else if (spoiler.AuthorId != context.User.Id)
				await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler with Id of {id} is not owned by you").ConfigureAwait(false);
			else {
				await DeleteSpoilerAsync(id, true).ConfigureAwait(false);
				await context.Channel.SendMessageAsync($"**Remove Spoiler:** Spoiler with Id of {id} was deleted").ConfigureAwait(false);
			}
		}
	}
}
