using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Help")]
	[IsLockable(false)]
	public class HelpModule : BotModuleBase {


		[Command("help", RunMode = RunMode.Async)]
		[Summary("List all commands usable by you")]
		public async Task HelpAsycn() {
			await HelpBase();
		}

		[Command("helpg", RunMode = RunMode.Async)]
		[Parameters("<groupName>")]
		[Summary("List all commands usable by you in the group")]
		public async Task HelpGroup([Remainder] string groupName) {
			string prefix = await Settings.GetPrefix(Context);
			SettingsBase settings = await Settings.GetSettings(Context);
			List<CommandInfo> results = await Help.GetUsableCommands(Context,
				c => !c.IsDuplicateFunctionality());
			CommandGroup group = Help.GetAllCommandGroups().FirstOrDefault(
				g => string.Compare(g.Name, groupName, true) == 0);

			if (group == null) {
				await ReplyAsync($"No group found matching **{groupName}**");
				return;
			}

			var builder = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				//Description = $"Found {group.Count} commands in group **{group.Name}**",
			};

			string description = null;
			foreach (CommandInfo cmd in group) {
				string alias = cmd.Aliases.First();
				/*if (!all) {
					if (!string.IsNullOrEmpty(description))
						description += " **|** ";
					description += $"{alias.Split(' ')[0]}";
				}
				else if (all) {
					if (args) {*/
						description += $"{prefix}{alias}";
						if (cmd.HasParameters())
							description += $" `{cmd.GetParameters()}`";
						description += "\n";
					/*}
					else {
						if (!string.IsNullOrEmpty(description))
							description += " **|** ";
						description += $"{alias}";
					}
				}*/
			}

			if (!string.IsNullOrWhiteSpace(description)) {
				builder.AddField(x => {
					x.Name = group.Name;
					x.Value = description;
					x.IsInline = false;
				});
			}

			/*foreach (CommandInfo cmd in results) {

				List<string> items = new List<string>();
				//if (cmd.Parameters.Any())
				//	items.Add($"__Parameters:__ {string.Join(", ", cmd.Parameters.Select(p => p.Name))}");
				if (cmd.HasParameters())
					items.Add($"__Parameters:__ `{cmd.GetParameters()}`");//{prefix}{cmd.Aliases.First()} 
				if (cmd.Summary != null)
					items.Add($"__Summary:__ {cmd.Summary}");
				//if (cmd.HasUsage())
				//	items.Add($"__Usage:__ {cmd.GetUsage()}");
				if (cmd.Remarks != null)
					items.Add($"__Remarks:__ {cmd.Remarks}");
				string value = string.Join('\n', items.ToArray());
				if (string.IsNullOrWhiteSpace(value))
					value = "No information available";
				string name = string.Join(", ", cmd.Aliases);
				if (cmd.IsLocked(settings))
					name = $"`🔒` {name}";
				builder.AddField(name, value);
			}*/

			await ReplyAsync("", false, builder.Build());
		}

		public async Task HelpBase(bool all = false, bool args = false) {
			SettingsBase settings = await Settings.GetSettings(Context);
			var groups = await Help.GetUsableCommandGroups(
				Context, c => (!c.IsDuplicate() ||
				(args && !c.IsDuplicateFunctionality()))
				&& !c.IsLocked(settings));

			if (!all) {
				List<CommandGroup> groupsFinal = new List<CommandGroup>();
				foreach (var group in groups) {
					List<CommandInfo> commandsFinal = new List<CommandInfo>();
					HashSet<string> baseCommands = new HashSet<string>();
					foreach (var cmd in group) {
						if (baseCommands.Add(cmd.Aliases.First().Split(' ')[0])) {
							commandsFinal.Add(cmd);
						}
					}
					group.Clear();
					group.AddRange(commandsFinal);
				}
			}

			string prefix = await Settings.GetPrefix(Context);
			var builder = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = $"These are the commands you can use\nThe prefix is: `{prefix}`",
			};

			foreach (CommandGroup group in groups) {
				string description = null;
				foreach (CommandInfo cmd in group) {
					string alias = cmd.Aliases.First();
					if (!all) {
						if (!string.IsNullOrEmpty(description))
							description += " **|** ";
						description += $"{alias.Split(' ')[0]}";
					}
					else if (all) {
						if (args) {
							description += $"{prefix}{alias}";
							if (cmd.HasParameters())
								description += $" `{cmd.GetParameters()}`";
							description += "\n";
						}
						else {
							if (!string.IsNullOrEmpty(description))
								description += " **|** ";
							description += $"{alias}";
						}
					}
				}

				if (!string.IsNullOrWhiteSpace(description)) {
					builder.AddField(x => {
						x.Name = group.Name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}

			ITextChannel channel = Context.Channel as ITextChannel;
			if (args && !(Context.Channel is IDMChannel)) {
				IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
				await dm.SendMessageAsync("", false, builder.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
			else {
				await Context.Channel.SendMessageAsync("", false, builder.Build());
			}
			
		}

		/*[Command("help all", RunMode = RunMode.Async)]
		[Summary("List every command in its entirety.")]
		public async Task HelpAllAsync() {
			await HelpBase(true);
		}

		[Command("help allargs", RunMode = RunMode.Async)]
		[Summary("List every command in its entirety, with parameters.")]
		public async Task HelpAllArgsAsync() {
			await HelpBase(true, true);
		}*/

		[Command("help", RunMode = RunMode.Async)]
		[Summary("Get help for any commands that start with the search term")]
		[Parameters("<command...>")]
		[Remarks("Entering `all` as a search term will list every command in its entirety.\n" +
			"Entering `allargs` will list every command as well as the parameters for the commands.\n" +
			"Either of these options will DM the list to you.")]
		[IsDuplicate(false)]
		public async Task HelpSearch([Remainder]string searchTerm) {
			string prefix = await Settings.GetPrefix(Context);

			if (string.Compare(searchTerm, "all", true) == 0) {
				await HelpBase(true);
				return;
			}
			else if (string.Compare(searchTerm, "allargs", true) == 0) {
				await HelpBase(true, true);
				return;
			}
			/*var result = Commands.Search(Context, command);

			List<CommandInfo> results = new List<CommandInfo>();
			if (result.IsSuccess) {
				foreach (var match in result.Commands) {
					var cmd = match.Command;
					if (cmd.IsDuplicateFunctionality())
						continue;
					results.Add(cmd);
				}
			}*/
			SettingsBase settings = await Settings.GetSettings(Context);
			List<CommandInfo> results = await Help.GetUsableCommands(Context,
				c => !c.IsDuplicateFunctionality());
			Help.SearchCommands(results, searchTerm);

			if (!results.Any()) {
				await ReplyAsync($"No commands found matching **{searchTerm}**");
				return;
			}

			var builder = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = $"Found {results.Count} commands matching **{searchTerm}**"
			};

			foreach (CommandInfo cmd in results) {

				List<string> items = new List<string>();
				//if (cmd.Parameters.Any())
				//	items.Add($"__Parameters:__ {string.Join(", ", cmd.Parameters.Select(p => p.Name))}");
				if (cmd.HasParameters())
					items.Add($"__Parameters:__ `{cmd.GetParameters()}`");//{prefix}{cmd.Aliases.First()} 
				if (cmd.Summary != null)
					items.Add($"__Summary:__ {cmd.Summary}");
				//if (cmd.HasUsage())
				//	items.Add($"__Usage:__ {cmd.GetUsage()}");
				if (cmd.Remarks != null)
					items.Add($"__Remarks:__ {cmd.Remarks}");
				string value = string.Join('\n', items.ToArray());
				if (string.IsNullOrWhiteSpace(value))
					value = "No information available";
				string name = string.Join(", ", cmd.Aliases);
				if (cmd.IsLocked(settings))
					name = $"`🔒` {name}";
				builder.AddField(name, value);
			}

			await ReplyAsync("", false, builder.Build());
		}

		[Command("locked")]
		[Summary("Lists all locked commands")]
		public async Task Locked() {
			List<CommandGroup> groups = Help.GetAllCommandGroups();
			SettingsBase settings = await Settings.GetSettings(Context);
			Help.FilterCommandGroups(groups, c => !c.IsDuplicate() && c.IsLocked(settings));


			string prefix = await Settings.GetPrefix(Context);
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = "The following commands are locked. Commands prefixed with `🔒` are locked directly.",
			};

			bool any = false;

			foreach (CommandGroup group in groups) {
				string name = group.Name;
				if (group.IsGroupLocked(settings))
					name = $"`🔒` {name}";
				string description = null;
				foreach (CommandInfo cmd in group) {
					if (!cmd.IsLocked(settings))
						continue;
					if (!any) any = true;
					if (cmd.IsLockedDirectly(settings))
						description += "`🔒` ";
					description += $"{prefix}{cmd.Aliases.First()}\n";
				}

				if (!string.IsNullOrWhiteSpace(description)) {
					embed.AddField(x => {
						x.Name = name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}
			if (!any)
				embed.Description = "There are no locked commands";

			await ReplyAsync("", false, embed.Build());

			//await Help.ListCommands(Context, groups);
		}

		[Command("reactions", RunMode = RunMode.Async)]
		[Summary("List and explain how all reactions are used by the bot")]
		public async Task Reactions() {
			//StringBuilder text = new StringBuilder();

			string prefix = await Settings.GetPrefix(Context);
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = "The following reactions are used to interact with the bot",
			};
			
			foreach (ReactionCategory category in BotReactions.Categories.Values) {
				string name = category.Category;
				string description = "";
				foreach (ReactionInfo reaction in category.Reactions) {
					description += $"{reaction.Emoji.ToString()} = {reaction.Description}\n";
				}
				
				embed.AddField(x => {
					x.Name = category.Category;
					x.Value = description;
					x.IsInline = false;
				});
			}

			/*foreach (ReactionCategory category in BotReactions.Categories.Values) {
				text.AppendLine($"**{category.Category}:**");
				foreach (ReactionInfo reaction in category.Reactions) {
					text.Append(reaction.Emoji.ToString());
					text.Append(" = ");
					text.Append(reaction.Description);
					text.AppendLine();
				}
				text.AppendLine();
			}*/

			await ReplyAsync("", false, embed.Build());
		}

		[Command("usercount")]
		[Summary("Counts the total number of users in the server, including those that are offline")]
		public async Task GetUserCount() {
			await Context.Guild.DownloadUsersAsync();
			int total = Context.Guild.Users.Count;
			int offline = 0;
			int bots = 0;
			foreach (IUser user in Context.Guild.Users) {
				if (user.IsBot)
					bots++;
				if (user.Status == UserStatus.Offline)
					offline++;
			}
			int online = total - offline - bots;
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				//Description = $"Users in {Context.Guild.Name}",
				Title = $"Users in {Context.Guild.Name}",
				Description = //$"Users in {Context.Guild.Name}\n\n" +
							$"Online: {online}\n" +
							$"Offline: {offline}\n" +
							$"Bots: {bots}\n" +
							$"Total: {total}\n",
			};
			/*embed.AddField("*Online:*", online, true);
			embed.AddField("*Offline:*", offline, true);
			embed.AddField("*Bots:*", bots, true);
			embed.AddField("*Total:*", total, true);*/

			await ReplyAsync("", false, embed.Build());

			/*await ReplyAsync($"**Users in:** {Context.Guild.Name}\n" +
							$"*Online:* {online}\n" +
							$"*Offline:* {offline}\n" +
							$"*Bots:* {bots}\n" +
							$"*Total:* {total}\n");*/
		}

		[Command("about")]
		[Summary("Gives some info on the bot")]
		public async Task About() {
			string name = Client.CurrentUser.GetName(Context.Guild);
			string prefix = await Settings.GetPrefix(Context);
			ulong creatorId = ulong.Parse(Config["creator_client_id"]);
			SocketUser creator = Client.GetUser(creatorId);
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Title = $"About {name}",
				Description =
					$"This bot is made by trigger_death for use in a small selection " +
					$"of Discord servers. " +
					$"It's selling features are spoilers that do not rely on gifs " +
					$"and a command which inserts claps between words. \n\n" +
					$"{name} is my first Discord bot as well as my first dive into " +
					$"heavy use of asynchonous functions and databases so things " +
					$"might be a little wonkey. \n\n" +
					$"Please be nice to {name}.  (ﾉ´ヮ`)ﾉ*: ･ﾟ\n",
			};
			embed.WithThumbnailUrl(@"https://i.imgur.com/Jep2ni2.png");
			embed.WithAuthor(creator);
			//embed.WithImageUrl(@"https://i.imgur.com/Jep2ni2.png");

			await ReplyAsync("", false, embed.Build());
		}

		/*[Command("msgcount cancel"), Alias("messagecount cancel")]
		[Summary("Cancels the current message count operation")]
		[RequireOwner]
		[RequireContext(ContextType.DM | ContextType.Group)]
		public async Task CancelMessageCount() {
			Stopwatch watch = Stopwatch.StartNew();
			IMessage message = await ReplyAsync("Getting channel message count. This may take awhile...");
			int count = 0;
			var enumerable = await Context.Channel.GetMessagesAsync().FlattenAsync();
			while (enumerable.Any()) {
				count += enumerable.Count();
				IMessage last = enumerable.Last();
				enumerable = await Context.Channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
			}
			await message.DeleteAsync();
			await ReplyAsync($"**Channel Message Count:** {count}\n\n*Took {watch.Elapsed.ToDHMSString()} to finish*");
		}

		[Command("msgcount"), Alias("messagecount")]
		[Summary("Gets the total message count for the current channel")]
		[RequireOwner]
		[RequireContext(ContextType.DM | ContextType.Group)]
		public async Task GetMessageCountPrivate() {
			Stopwatch watch = Stopwatch.StartNew();
			IMessage message = await ReplyAsync("Getting channel message count. This may take awhile...");
			int count = 0;
			var enumerable = await Context.Channel.GetMessagesAsync().FlattenAsync();
			while (enumerable.Any()) {
				count += enumerable.Count();
				IMessage last = enumerable.Last();
				enumerable = await Context.Channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
			}
			await message.DeleteAsync();
			await ReplyAsync($"**Channel Message Count:** {count}\n\n*Took {watch.Elapsed.ToDHMSString()} to finish*");
		}

		[Command("msgcount"), Alias("messagecount")]
		[Summary("Gets the total message count for the current channel")]
		[RequireOwner]
		[RequireContext(ContextType.DM | ContextType.Group)]
		public async Task GetMessageCountPrivateUser(IUser user) {
			Stopwatch watch = Stopwatch.StartNew();
			IMessage message = await ReplyAsync("Getting channel message count. This may take awhile...");
			int count = 0;
			string id = user.Id;
			var enumerable = await Context.Channel.GetMessagesAsync().FlattenAsync();
			while (enumerable.Any()) {
				IMessage last = null;
				foreach (IMessage msg in enumerable) {
					if (msg.Author.Id == id) {

					}
					count++;
					last = msg;
				}
				count += enumerable.Count();
				IMessage last = enumerable.Last();
				enumerable = await Context.Channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
			}
			await message.DeleteAsync();
			await ReplyAsync($"**Channel Message Count:** {count}\n\n*Took {watch.Elapsed.ToDHMSString()} to finish*");
		}

		[Command("msgcount channel"), Alias("messagecount channel")]
		[Summary("Gets the total message count for the current channel")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		[RequireContext(ContextType.Guild)]
		public async Task GetMessageCount() {
			Stopwatch watch = Stopwatch.StartNew();
			await ReplyAsync("Getting channel message count. This may take awhile...");
			int count = 0;
			var enumerable = await Context.Channel.GetMessagesAsync().FlattenAsync();
			while (enumerable.Any()) {
				count += enumerable.Count();
				IMessage last = enumerable.Last();
				enumerable = await Context.Channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
			}
			await ReplyAsync($"**Channel Message Count:** {count}");
		}

		[Command("msgcount guild"), Alias("messagecount guild")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		[Summary("Gets the total message count for the entire guild")]
		[RequireContext(ContextType.Guild)]
		public async Task GetGuildMessageCount() {
			Stopwatch watch = Stopwatch.StartNew();
			IMessage message = await ReplyAsync("Getting guild message count. This may take awhile...");
			int count = 0;
			foreach (var baseChannel in Context.Guild.Channels) {
				if (baseChannel is ITextChannel channel) {
					var enumerable = await channel.GetMessagesAsync().FlattenAsync();
					while (enumerable.Any()) {
						count += enumerable.Count();
						IMessage last = enumerable.Last();
						enumerable = await channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
					}
				}
			}
			await message.DeleteAsync();
			await ReplyAsync($"**Total Message Count:** {count}\n\n*Took {watch.Elapsed.ToDHMSString()} to finish*");
		}*/
	}
}
