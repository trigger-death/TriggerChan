using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Management")]
	[IsLockable(false)]
	public class ManagementModule : BotModuleBase {
		
		[Group("prefix")]
		public class Prefix : BotModuleBase {

			[Command("", RunMode = RunMode.Async)]
			[Summary("Get the bot's prefix for this channel")]
			public async Task GetPrefix() {
				string prefix = await Settings.GetPrefix(Context);
				await ReplyAsync($"**Prefix:** `{prefix}`");
			}

			[Command("guild", RunMode = RunMode.Async)]
			[Summary("Get the bot's prefix for the entire guild. Note: This channel may use a different prefix")]
			[RequireContext(ContextType.Guild)]
			public async Task GetGuildPrefix() {
				string prefix = await Settings.GetGuildPrefix(Context);
				await ReplyAsync($"**Prefix:** `{prefix}`");
			}

			[Command("set", RunMode = RunMode.Async)]
			[Summary("Assign the bot's prefix")]
			[RequireUserPermissionOrBotOwner(GuildPermission.ManageChannels)]
			[Parameters("<newPrefix>")]
			//[IsDuplicateName]
			public async Task SetPrefix(string newPrefix) {
				newPrefix = newPrefix.Trim();
				if (!newPrefix.Contains(" ") && !newPrefix.Contains("\t") &&
					!newPrefix.Contains("`") && !newPrefix.Contains("\\")) {
					await Settings.SetPrefix(Context, newPrefix);
					await ReplyAsync($"**New Prefix:** `{newPrefix}`");
				}
				else {
					Context.CustomError = CustomCommandError.InvalidArgument;
					Context.ErrorReason = "Invalid characters in prefix";
					Context.IsSuccess = false;
				}
			}

			[Command("set")]
			[RequireOwner]
			[Browsable(false)]
			[IsDuplicate(true)]
			public async Task SetPrefixOwner(string newPrefix) {
				await SetPrefix(newPrefix);
			}
			
			[Group("channel")]
			[RequireContext(ContextType.Guild)]
			public class Channel : BotModuleBase {

				[Command("", RunMode = RunMode.Async)]
				[Summary("Gets the bot's prefix for this guild channel only")]
				[RequireUserPermissionOrBotOwner(GuildPermission.ManageChannels)]
				[IsLockable]
				public async Task GetChannelPrefix() {
					string prefix = await Settings.GetChannelPrefix(Context);
					if (prefix == null) {
						prefix = await Settings.GetPrefix(Context);
						await ReplyAsync($"Channel prefix is unset\n**Default Prefix:** `{prefix}`");
					}
					else {
						await ReplyAsync($"**Channel Prefix:** `{prefix}`");
					}
				}

				[Command("set", RunMode = RunMode.Async)]
				[Summary("Assign the bot's prefix for this guild channel only")]
				[Parameters("<newPrefix>")]
				[RequireUserPermissionOrBotOwner(GuildPermission.ManageChannels)]
				[IsLockable]
				public async Task SetChannelPrefix(string newPrefix) {
					newPrefix = newPrefix.Trim();
					if (!newPrefix.Contains(" ") && !newPrefix.Contains("\t") &&
						!newPrefix.Contains("`") && !newPrefix.Contains("\\")) {
						await Settings.SetChannelPrefix(Context, newPrefix);
						await ReplyAsync($"**New Prefix:** `{newPrefix}`");
					}
					else {
						Context.CustomError = CustomCommandError.InvalidArgument;
						Context.ErrorReason = "Invalid characters in prefix";
						Context.IsSuccess = false;
					}
				}

				[Command("unset", RunMode = RunMode.Async)]
				[Summary("Removes the bot prefix override for this channel")]
				[RequireUserPermissionOrBotOwner(GuildPermission.ManageChannels)]
				[IsLockable]
				public async Task SetChannelPrefix() {
					await Settings.SetChannelPrefix(Context, null);
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
		}

		[Group("lock")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		public class Lock : BotModuleBase {
			[Command("cmd", RunMode = RunMode.Async)]
			[Parameters("<command>")]
			[Summary("Locks all commands with this name")]
			public async Task LockCommand([Remainder]string command) {
				CommandInfo cmd = Help.GetCommand(ref command);
				if (cmd == null) {
					Context.IsSuccess = false;
					await ReplyAsync($"No command with the name **{command}**");
					return;
				}
				if (!(await Settings.LockCommand(Context, cmd))) {
					Context.IsSuccess = false;
				}
				else {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}

			[Command("group", RunMode = RunMode.Async)]
			[Parameters("<group>")]
			[Summary("Locks all commands within the group")]
			public async Task LockCommandGroup([Remainder]string module) {
				ModuleInfo mod = Help.GetModule(ref module);
				if (mod == null || mod.Parent != null) {
					Context.IsSuccess = false;
					await ReplyAsync($"No group with the name **{module}**");
					return;
				}
				if (!(await Settings.LockModule(Context, mod))) {
					Context.IsSuccess = false;
				}
				else {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
		}


		[Group("unlock")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		public class Unlock : BotModuleBase {
			[Command("cmd", RunMode = RunMode.Async)]
			[Parameters("<command>")]
			[Summary("Unlocks all commands with this name")]
			public async Task UnlockCommand([Remainder]string command) {
				CommandInfo cmd = Help.GetCommand(ref command);
				if (cmd == null) {
					Context.IsSuccess = false;
					await ReplyAsync($"No command with the name **{command}**");
					return;
				}
				if (!(await Settings.UnlockCommand(Context, cmd))) {
					Context.IsSuccess = false;
				}
				else {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}

			[Command("group", RunMode = RunMode.Async)]
			[Parameters("<group>")]
			[Summary("Unlocks all commands within the group")]
			public async Task UnlockCommandGroup([Remainder]string module) {
				ModuleInfo mod = Help.GetModule(ref module);
				if (mod == null || mod.Parent != null) {
					Context.IsSuccess = false;
					await ReplyAsync($"No group with the name **{module}**");
					return;
				}
				if (!(await Settings.UnlockModule(Context, mod))) {
					Context.IsSuccess = false;
				}
				else {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}

			[Command("all", RunMode = RunMode.Async)]
			[Parameters("<group>")]
			[Summary("Unlocks all commands and groups")]
			public async Task UnlockAll() {
				if (!(await Settings.UnlockAll(Context))) {
					Context.IsSuccess = false;
				}
				else {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
		}

		[Command("botowners")]
		[Summary("Gets the list of owners for this bot.")]
		[RequireContext(ContextType.Guild)]
		public async Task GetBotOwners() {
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				var embed = new EmbedBuilder() {
					Color = new Color(114, 137, 218),
					Title = $"Bot Owners",
				};
				string description = "";
				foreach (GuildUser gUser in gUsers) {
					if (!gUser.IsBotOwner)
						continue;
					IGuildUser user = Context.Guild.GetUser(gUser.UserId);
					if (user != null)
						description += $"{user.GetName(Context.Guild)}\n";
				}
				if (string.IsNullOrEmpty(description)) {
					await ReplyAsync("No users are bot owners");
				}
				else {
					embed.Description = description;
					await ReplyAsync("", false, embed.Build());
				}
			}
		}

		[Command("botowner assign")]
		[Summary("Gives the user full access to the bot's commands.")]
		[Parameters("<user>")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task AssignBotOwner(IGuildUser user) {
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(database, user, true);
				gUser.IsBotOwner = true;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("botowner unassign")]
		[Summary("Gives the user full access to the bot's commands.")]
		[Parameters("<user>")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task UnassignBotOwner(IGuildUser user) {
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(database, user, true);
				gUser.IsBotOwner = false;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("purge", RunMode = RunMode.Async)]
		[Summary("Clears all or a designated amount of messages from the channel")]
		[RequireOwner]
		[IsLockable]
		public async Task Purge([Remainder]int count = -1) {
			if (count != -1 && count < 0) {
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Purge count cannot be less than 0";
				Context.IsSuccess = false;
			}
			IEnumerable<IMessage> messages;
			do {
				var channel = (SocketTextChannel) Context.Channel;
				messages = await Context.Channel.GetMessagesAsync(200).FlattenAsync();
				List<IMessage> messagesToDelete = new List<IMessage>();
				foreach (IMessage message in messages) {
					if (count == 0)
						break;
					messagesToDelete.Add(message);
					count--;
				}
				await channel.DeleteMessagesAsync(messagesToDelete);
			} while (messages.Any() && count != 0);
		}
	}
}
