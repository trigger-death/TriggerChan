using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.Utils;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Reactions;
using TriggersTools.DiscordBots.Extensions;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Help")]
	[Summary("Commands for help with using the bot")]
	[AllowBots(false)]
	[IsLockable(false)]
	public class HelpModule : DiscordBotModule {

		private readonly SpoilerService spoilers;
		private readonly ReactionService reactions;
		private readonly HelpService help;

		public HelpModule(DiscordBotServiceContainer services,
						  SpoilerService spoilers,
						  ReactionService reactions,
						  HelpService help)
			: base(services)
		{
			this.spoilers = spoilers;
			this.reactions = reactions;
			this.help = help;
		}

		[Name("help")]
		[Group("help"), Alias("h", "?")]
		[Usage("[commandName|module <moduleName>]")]
		[Summary("Display information about the bot's commands and modules")]
		public class HelpGroup : DiscordBotModule {

			private readonly HelpService help;

			public HelpGroup(DiscordBotServiceContainer services,
							 HelpService help)
				: base(services)
			{
				this.help = help;
			}

			[Name("help")]
			[Command("")]
			[Priority(0)]
			[Example("Show a list of all commands and modules")]
			public async Task HelpList() {
				await ReplyAsync(embed: await help.BuildHelpListAsync(Context, Commands.CommandSet).ConfigureAwait(false)).ConfigureAwait(false);
			}

			[Name("help <commandName>")]
			[Command("")]
			[Priority(0)]
			[Example("spoiler", "Explain the usage of the `spoiler` command")]
			public async Task HelpSearchCommand([Remainder] string commandName) {
				commandName = string.Join(" ", commandName.Trim().Split(' '));
				CommandDetails command = Commands.CommandSet.FindCommand(commandName, Context);
				if (command == null)
					await ReplyAsync($"No command with the name `{commandName}` exists").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildCommandHelpAsync(Context, command).ConfigureAwait(false)).ConfigureAwait(false);
			}

			[Name("help module <moduleName>")]
			[Command("module")]
			[Priority(1)]
			[Example("management", "Summarize the `Management` command module")]
			public async Task HelpSearchGroup([Remainder] string moduleName) {
				moduleName = string.Join(" ", moduleName.Trim().Split(' '));
				ModuleDetails module = Commands.CommandSet.FindModule(moduleName, Context);
				if (module == null)
					await ReplyAsync($"No module with the name `{moduleName}` exists").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildModuleHelpAsync(Context, module, false).ConfigureAwait(false)).ConfigureAwait(false);
			}
		}

		[Name("prefix")]
		[Command("prefix")]
		[Priority(0)]
		[Summary("Gets the bot's command prefix")]
		[Usage("Displays the current bot command prefix")]
		public async Task GetPrefix() {
			string prefix = await Contexting.GetPrefixAsync(Context).ConfigureAwait(false);
			await ReplyAsync($"**Prefix:** `{Format.Sanitize(prefix)}`").ConfigureAwait(false);
		}

		[Name("reactions")]
		[Command("reactions")]
		[Summary("List and explain how all reactions are used by the bot")]
		[Example("Display the bot's reaction list")]
		public Task Reactions() {
			return ReplyAsync(embed: help.BuildReactionList());
		}

		[Name("about")]
		[Command("about")]
		[Summary("Give a detailed explanation of the bot along with general information and statistics")]
		[Example("Display the about message")]
		public async Task About() {
			var embed = new EmbedBuilder {
				Color = ColorUtils.Parse(Config["embed_color"]),
				Title = Config["about_title"] ?? $"{Client.CurrentUser.Username}: About",
			};
			embed.WithThumbnailUrl(Client.CurrentUser.GetAvatarUrl());
			StringBuilder str = new StringBuilder();
			var links = Config.GetSection("about_links");
			List<string> linkList = new List<string>();
			if (links != null) {
				foreach (var child in links.GetChildren()) {
					string name = child["name"];
					string url = child["url"];
					if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(url)) {
						linkList.Add($"[{name}]({url})");
					}
				}
			}
			if (linkList.Count != 0)
				embed.Description = string.Join(" | ", linkList);
			if (Config["about_description"] != null)
				embed.AddField("About", Config["about_description"]);

			string prefix = await Contexting.GetPrefixAsync(Context).ConfigureAwait(false);
			embed.AddField("Prefix", $"The command prefix is `{prefix}`");// is present on **{Client.Guilds.Count}** servers!");

			TimeSpan uptime = DiscordBot.Uptime;
			int d = (int) uptime.TotalDays;
			int h = uptime.Hours;
			int m = uptime.Minutes;
			int s = uptime.Seconds;
			uptime = DiscordBot.TotalUptime;
			int d2 = (int) uptime.TotalDays;
			int h2 = uptime.Hours;
			int m2 = uptime.Minutes;
			int s2 = uptime.Seconds;
			int guilds = Client.Guilds.Count;
			long spoilers = await this.spoilers.GetSpoilerCountAsync().ConfigureAwait(false);
			long spoiledUsers = await this.spoilers.GetSpoiledUserCountAsync().ConfigureAwait(false);
			long members = Client.Guilds.Sum(g => g.Users.Count);

			embed.AddField("Stats", $"**{spoilers}** spoiler{Plural(spoilers)} have been revealed " +
									$"**{spoiledUsers}** time{Plural(spoiledUsers)}");
			embed.AddField("Uptime", $"`Current:` " +
									 $"**{d}** day{Plural(d)}, " +
									 $"**{h}** hour{Plural(h)}, " +
									 $"**{m}** minute{Plural(m)}, and " +
									 $"**{s}** second{Plural(s)}\n" +
									 $"`Total:` " +
									 $"**{d2}** day{Plural(d)}, " +
									 $"**{h2}** hour{Plural(h)}, " +
									 $"**{m2}** minute{Plural(m)}, and " +
									 $"**{s2}** second{Plural(s)}");
			embed.AddField("Servers", $"Active on **{guilds}** server{Plural(guilds)} with " +
									  $"**{members}** member{Plural(members)}");

			await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
		}

		private string Plural(long count) {
			return (count != 1 ? "s" : "");
		}

		/*[Command("ping")]
		[Summary("Ping the bot and see the latency")]
		public async Task Ping() {
			await ReplyAsync($"**Pong:** `{(int) Client.Latency}ms`");
		}*/


		[Name("uptime")]
		[Command("uptime")]
		[Summary("Displays the time the bot has been running for")]
		public Task Uptime() {
			TimeSpan uptime = DiscordBot.Uptime;
			int d = (int) uptime.TotalDays;
			int h = uptime.Hours;
			int m = uptime.Minutes;
			int s = uptime.Seconds;
			uptime = DiscordBot.TotalUptime;
			int d2 = (int) uptime.TotalDays;
			int h2 = uptime.Hours;
			int m2 = uptime.Minutes;
			int s2 = uptime.Seconds;
			
			return ReplyAsync($"`Current:` " +
							  $"**{d}** day{Plural(d)}, " +
							  $"**{h}** hour{Plural(h)}, " +
							  $"**{m}** minute{Plural(m)}, and " +
							  $"**{s}** second{Plural(s)}\n" +
							  $"`Total:` " +
							  $"**{d2}** day{Plural(d)}, " +
							  $"**{h2}** hour{Plural(h)}, " +
							  $"**{m2}** minute{Plural(m)}, and " +
							  $"**{s2}** second{Plural(s)}");
		}

		[Name("ping")]
		[Command("ping")]
		[Summary("Returns the current estimated round-trip latency over WebSocket and REST")]
		public async Task LatencyAsync() {
			/*IUserMessage message = null;
			Stopwatch stopwatch = null;
			int heartbeat = Context.Client.Latency;

			var tcs = new TaskCompletionSource<long>();
			var timeout = Task.Delay(TimeSpan.FromSeconds(30));

			Task TestMessageAsync(SocketMessage arg) {
				if (arg.Id != message?.Id) return Task.CompletedTask;
				tcs.SetResult(stopwatch.ElapsedMilliseconds);
				return Task.CompletedTask;
			}

			Client.MessageReceived += TestMessageAsync;
			stopwatch = Stopwatch.StartNew();
			message = await ReplyAsync($"**Hearbeat:** `{heartbeat}ms`, **init:** `---`, **rtt:** `---`");
			var init = stopwatch.ElapsedMilliseconds;

			//Client.MessageReceived += TestMessageAsync;
			var task = await Task.WhenAny(tcs.Task, timeout);
			Client.MessageReceived -= TestMessageAsync;
			stopwatch.Stop();

			if (task == timeout) {
				await message.ModifyAsync(x => x.Content = $"**Hearbeat:** `{heartbeat}ms`, **init:** `{init}ms`, **rtt:** `timed out`");
			}
			else {
				var rtt = await tcs.Task;
				await message.ModifyAsync(x => x.Content = $"**Hearbeat:** `{heartbeat}ms`, **init:** `{init}ms`, **rtt:** `{rtt}ms`");
			}*/
			/*int latency = Context.Client.Latency;
			Stopwatch watch = Stopwatch.StartNew();
			IUserMessage message = await ReplyAsync($"**Hearbeat:** `{latency}ms`, **init:** `---`, **rtt:** `---`").ConfigureAwait(false);
			long init = watch.ElapsedMilliseconds;
			await message.ModifyAsync(x => x.Content = $"**Hearbeat:** `{latency}ms`, **init:** `{init}ms`, **rtt:** `calculating`").ConfigureAwait(false);
			long rtt = watch.ElapsedMilliseconds;
			watch.Stop();
			await message.ModifyAsync(x => x.Content = $"**Hearbeat:** `{latency}ms`, **init:** `{init}ms`, **rtt:** `{rtt}ms`").ConfigureAwait(false);*/
			await help.PingResultAsync(Context).ConfigureAwait(false);
		}
	}
}
