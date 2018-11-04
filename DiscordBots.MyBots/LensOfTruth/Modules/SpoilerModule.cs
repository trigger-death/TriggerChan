using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Contexts;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Commands;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Spoilers")]
	[Summary("All commands related to spoilers")]
	[RequiresContext(ContextType.Guild)]
	public class SpoilerModule : DiscordBotModule {
		
		#region Constructors
		
		public SpoilerModule(DiscordBotServiceContainer services) : base(services) { }

		#endregion

		[Group("spoiler")]//, Alias("spoilers", "spoil", "spoiled")]
		[Usage("[raw] [<up|upload>|remove <last|messageId>|rename <last|messageId> <title...>] [{title}] [content...]")]
		[Summary("Hide a spoiler behind a message that can be viewed by reacting with 🔍")]
		[Remarks("`content...` must be included when the first argument is not present\nIt's common courtesy to include a `{title}` with all spoilers")]
		public class SpoilerGroup : DiscordBotModule {

			#region Fields

			private readonly SpoilerService spoilers;
			private readonly HelpService help;

			#endregion

			#region Constructors

			public SpoilerGroup(DiscordBotServiceContainer services,
								SpoilerService spoilers,
								HelpService help)
				: base(services)
			{
				this.spoilers = spoilers;
				this.help = help;
			}

			#endregion
			
			[Name("spoiler <content>")]
			[Command("")]
			[Priority(0)]
			[Example("{Steins;Gate} 1.048596", "Hide *1.048596* behind a spoiler")]
			public Task<RuntimeResult> Spoiler([Remainder] string content) {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), false, false);
			}

			[Name("spoiler raw <content>")]
			[Command("raw")]
			[Priority(1)]
			[Example("{What!?} Navy Seals Copypasta", "Post a raw spoiler that has room for the entire Navy Seals copypasta")]
			public Task<RuntimeResult> RawSpoiler([Remainder] string content) {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), false, true);
			}

			[Name("spoiler upload [content]")]
			[Command("upload"), Alias("up")]
			[Priority(2)]
			[Example("{Incoming Attachment}", "Send an attachment to your DM to complete the spoiler")]
			[AllowBots(false)]
			public Task<RuntimeResult> AttachmentSpoiler([Remainder] string content = "") {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), true, false);
			}

			[Name("spoiler raw upload [content]")]
			[Command("raw upload"), Alias("raw up", "upload raw", "up raw")]
			[Priority(3)]
			[AllowBots(false)]
			public Task<RuntimeResult> RawAttachmentSpoiler([Remainder] string content = "") {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), true, true);
			}

			[Name("spoiler rename last")]
			[Command("rename last")]
			[Priority(2)]
			[Example("New Title Bois!", "Rename the title of your last posted spoiler in this channel to *New Title Bois!*")]
			public async Task<RuntimeResult> RenameLastSpoiler([Remainder] string title) {
				using (var db = GetDb<SpoilerDbContext>()) {
					Spoiler spoiler = await spoilers.FindLastSpoilerByUserAsync(db, Context.User.Id, Context.Channel.Id).ConfigureAwait(false);
					if (spoiler == null)
						await ReplyAsync($"**Remove Spoiler:** You have not posted any spoilers in this channel").ConfigureAwait(false);
					else {
						await spoilers.RenameSpoilerAsync(db, spoiler, title).ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
				return NormalResult.FromSuccess();
			}

			[Name("spoiler rename <messageId>")]
			[Command("rename")]
			[Priority(1)]
			[Example("499990259434782720 New Title Bois!", "Rename the title of your spoiler with this Id to *New Title Bois!*")]
			public async Task<RuntimeResult> RenameSpoiler(ulong id, [Remainder] string title) {
				using (var db = GetDb<SpoilerDbContext>()) {
					Spoiler spoiler = await spoilers.FindSpoilerAsync(db, id).ConfigureAwait(false);
					if (spoiler == null)
						await ReplyAsync($"**Remove Spoiler:** Spoiler with Id of {id} was either not found").ConfigureAwait(false);
					else if (spoiler.AuthorId != Context.User.Id)
						await ReplyAsync($"**Remove Spoiler:** Spoiler with Id of {id} is not owned by you").ConfigureAwait(false);
					else {
						await spoilers.RenameSpoilerAsync(db, spoiler, title).ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
				return NormalResult.FromSuccess();
			}

			[Name("spoiler remove last")]
			[Command("remove last")]
			[Priority(2)]
			[Example("Remove your last posted spoiler in this channel")]
			public async Task RemoveLastSpoiler() {
				Spoiler spoiler = await spoilers.FindLastSpoilerByUserAsync(Context.User.Id, Context.Channel.Id).ConfigureAwait(false);
				if (spoiler == null)
					await ReplyAsync($"**Remove Spoiler:** You have not posted any spoilers in this channel").ConfigureAwait(false);
				else {
					await ReplyAsync($"**Remove Spoiler:** Spoiler posted at {spoiler.TimeStamp:dd MMMM yyyy HH:mm:ss} UTC was deleted").ConfigureAwait(false);
					await spoilers.DeleteSpoilerAsync(spoiler.MessageId, true).ConfigureAwait(false);
				}
			}

			[Name("spoiler remove <messageId>")]
			[Command("remove")]
			[Priority(1)]
			[Example("499990259434782720", "Remove your spoiler with this Id")]
			public async Task RemoveSpoiler(ulong id) {
				Spoiler spoiler = await spoilers.FindSpoilerAsync(id).ConfigureAwait(false);
				if (spoiler == null)
					await ReplyAsync($"**Remove Spoiler:** Spoiler with Id of {id} was either not found").ConfigureAwait(false);
				else if (spoiler.AuthorId != Context.User.Id)
					await ReplyAsync($"**Remove Spoiler:** Spoiler with Id of {id} is not owned by you").ConfigureAwait(false);
				else {
					await spoilers.DeleteSpoilerAsync(id, true).ConfigureAwait(false);
					await ReplyAsync($"**Remove Spoiler:** Spoiler with Id of {id} was deleted").ConfigureAwait(false);
				}
			}

			/// <summary>
			/// For those poor fools who typed `s;command help` instead of `s;help command`.
			/// </summary>
			[Name("spoiler help")]
			[Command("help")]
			[Priority(3)]
			public async Task SpoilerHelp() {
				CommandDetails command = Commands.CommandSet.FindCommand("spoiler", Context);
				if (command == null)
					await ReplyAsync($"No command with the name `spoiler` exists").ConfigureAwait(false);
				else if (!command.IsUsable(Context))
					await ReplyAsync($"The command `{command.Alias}` cannot be used").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildCommandHelpAsync(Context, command).ConfigureAwait(false)).ConfigureAwait(false);
			}

			private string ResolveContent(string content) {
				if (string.IsNullOrWhiteSpace(content))
					return string.Empty;
				int index = Context.Message.Content.LastIndexOf(content);
				return Context.Message.Resolve(index, everyoneHandling: TagHandling.Name, emojiHandling: TagHandling.Ignore);
			}
		}

		/*[Command("eraseme")]
		[Summary("Erases all information on you from the database.")]
		public async Task EraseMe() {
			var wait = new EraseEndUserDataWaitContext(Context, EndUserDataType.User, Context.User.Id, true);
			await wait.Start().ConfigureAwait(false);
		}*/
	}
}
