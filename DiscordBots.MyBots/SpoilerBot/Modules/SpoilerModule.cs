using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Contexts;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.SpoilerBot.Commands;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Spoilers")]
	[Summary("All commands related to spoilers")]
	[RequiresContext(ContextType.Guild)]
	[AllowBots(true)]
	public class SpoilerModule : DiscordBotModule {
		
		#region Constructors
		
		public SpoilerModule(DiscordBotServiceContainer services) : base(services) { }

		#endregion

		[Group("spoiler")]//, Alias("spoilers", "spoil", "spoiled")]
		[Usage("[raw] [up|upload|remove <last|messageId>|rename <last|messageId> <title...>] [<{title}>] [<content...>]")]
		[Summary("Hide a spoiler behind a message that can be viewed by reacting with 🔍")]
		[Remarks("`content...` must be included when the first argument is not present\nIt's common courtesy to include a `{title}` with all spoilers")]
		[RequiresContext(ContextType.Guild)]
		public class SpoilerGroup : DiscordBotModule {

			#region Fields

			private readonly ISpoilerService spoilers;
			private readonly HelpService help;

			#endregion

			#region Constructors

			public SpoilerGroup(DiscordBotServiceContainer services,
								ISpoilerService spoilers,
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
			public Task<RuntimeResult> RenameLastSpoiler([Remainder] string title) {
				return spoilers.RenameLastSpoilerAsync(Context, title);
			}

			[Name("spoiler rename <messageId>")]
			[Command("rename")]
			[Priority(1)]
			[Example("499990259434782720 New Title Bois!", "Rename the title of your spoiler with this Id to *New Title Bois!*")]
			public Task<RuntimeResult> RenameSpoiler(ulong id, [Remainder] string title) {
				return spoilers.RenameSpoilerAsync(Context, id, title);
			}

			[Name("spoiler remove last")]
			[Command("remove last")]
			[Priority(2)]
			[Example("Remove your last posted spoiler in this channel")]
			public Task RemoveLastSpoiler() {
				return spoilers.RemoveLastSpoilerAsync(Context);
			}

			[Name("spoiler remove <messageId>")]
			[Command("remove")]
			[Priority(1)]
			[Example("499990259434782720", "Remove your spoiler with this Id")]
			public Task RemoveSpoiler(ulong id) {
				return spoilers.RemoveSpoilerAsync(Context, id);
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
