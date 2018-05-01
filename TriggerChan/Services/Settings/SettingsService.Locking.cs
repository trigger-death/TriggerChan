using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class SettingsService : BotServiceBase {

		public async Task<bool> IsCommandLocked(SocketCommandContext context, CommandInfo cmd) {
			// Check if the command is locked
			return (await GetSettings(context)).IsCommandLocked(cmd);
		}

		public async Task<bool> LockCommand(SocketCommandContext context, CommandInfo cmd) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);
				string alias = cmd.Aliases.First();

				if (!cmd.IsLockable()) {
					await context.Channel.SendMessageAsync($"The command **{alias}** cannot be locked");
					return false;
				}
				else if (settings.LockCommand(alias)) {
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The command **{alias}** is already locked");
					return false;
				}
			}
		}

		public async Task<bool> UnlockCommand(SocketCommandContext context, CommandInfo cmd) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);
				string alias = cmd.Aliases.First();

				if (settings.UnlockCommand(alias)) {
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The command **{alias}** is already unlocked");
					return false;
				}
			}
		}

		public async Task<bool> LockModule(SocketCommandContext context, ModuleInfo mod) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);
				string name = mod.RootModule().Name;

				if (!mod.IsLockable()) {
					await context.Channel.SendMessageAsync($"The command group **{name}** cannot be locked");
					return false;
				}
				else if (settings.LockModule(name)) {
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The command group **{name}** is already locked");
					return false;
				}
			}
		}

		public async Task<bool> UnlockModule(SocketCommandContext context, ModuleInfo mod) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);
				string name = mod.RootModule().Name;

				if (settings.UnlockModule(name)) {
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The command **{name}** is already unlocked");
					return false;
				}
			}
		}

		public async Task<bool> UnlockAll(SocketCommandContext context) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);

				if (settings.LockedCommandsStr != "|" || settings.LockedModulesStr != "|") {
					settings.LockedCommandsStr = "|";
					settings.LockedModulesStr = "|";
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"No commands or command groups are locked");
					return false;
				}
			}
		}

		public async Task<bool> IsRolePublic(SocketCommandContext context, IRole role) {
			return (await GetGuild(context.Guild.Id)).IsRolePublic(role.Id);
		}

		public async Task<bool> SetRolePublic(SocketCommandContext context, IRole role) {
			using (var database = new BotDatabaseContext()) {
				Guild guild = await Settings.GetGuild(database, context.Guild.Id, true);
				string name = role.Name;

				if (guild.SetRolePublic(role.Id)) {
					database.Guilds.Update(guild);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The role **{name}** is already public");
					return false;
				}
			}
		}

		public async Task<bool> SetRolePrivate(SocketCommandContext context, IRole role) {
			using (var database = new BotDatabaseContext()) {
				Guild guild = await Settings.GetGuild(database, context.Guild.Id, true);
				string name = role.Name;

				if (guild.SetRolePrivate(role.Id)) {
					database.Guilds.Update(guild);
					await database.SaveChangesAsync();
					return true;
				}
				else {
					await context.Channel.SendMessageAsync($"The role **{name}** is already private");
					return false;
				}
			}
		}
	}
}
