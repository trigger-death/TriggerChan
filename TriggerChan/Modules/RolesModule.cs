using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Roles")]
	[IsLockable]
	public class RolesModule : BotModuleBase {
		[Command("roles", RunMode = RunMode.Async)]
		[Summary("Lists all public roles that users can assign to themselves")]
		[RequireContext(ContextType.Guild)]
		public async Task Roles() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = "The following roles are public, which allows users to assign it to themselves",
			};

			Guild guild = await Settings.GetGuild(Context.Guild.Id);
			string description = "";
			bool any = false;
			foreach (var role in Context.Guild.Roles) {
				if (guild.IsRolePublic(role.Id)) {
					if (!any)
						any = true;
					description += $"{role.Name}\n";
				}
			}
			if (!string.IsNullOrWhiteSpace(description)) {
				embed.AddField(x => {
					x.Name = "Public Roles";
					x.Value = description;
					x.IsInline = false;
				});
			}
			else {
				embed.Description = "There are no public roles";
			}

			await ReplyAsync("", false, embed.Build());
		}

		[Command("role assign", RunMode = RunMode.Async)]
		[Parameters("<role>")]
		[Summary("Assigns the role to the caller")]
		[RequireContext(ContextType.Guild)]
		public async Task Assign([Remainder]string roleName) {
			IRole role = Context.Guild.Roles.FirstOrDefault(
				r => string.Compare(r.Name, roleName, true) == 0);
			if (role == null) {
				Context.IsSuccess = false;
				await ReplyAsync($"No role with the name **{roleName}**");
				return;
			}
			if (await Settings.IsRolePublic(Context, role)) {
				var guildUser = Context.Guild.GetUser(Context.Message.Author.Id);
				if (guildUser.Roles.Any(r => r.Id == role.Id)) {
					Context.IsSuccess = false;
					await ReplyAsync($"You already have the role **{role.Name}**");
				}
				else {
					await guildUser.AddRoleAsync(role);
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
			else {
				Context.IsSuccess = false;
				await ReplyAsync($"The role **{role.Name}** is not public!");
			}
		}

		[Command("role unassign", RunMode = RunMode.Async)]
		[Parameters("<role>")]
		[Summary("Unassigns the role from the caller")]
		[RequireContext(ContextType.Guild)]
		public async Task Unassign([Remainder]string roleName) {
			IRole role = Context.Guild.Roles.FirstOrDefault(
				r => string.Compare(r.Name, roleName, true) == 0);
			if (role == null) {
				Context.IsSuccess = false;
				await ReplyAsync($"No role with the name **{roleName}**");
				return;
			}
			if (await Settings.IsRolePublic(Context, role)) {
				var guildUser = Context.Guild.GetUser(Context.Message.Author.Id);
				if (!guildUser.Roles.Any(r => r.Id == role.Id)) {
					Context.IsSuccess = false;
					await ReplyAsync($"You do not have the role **{role.Name}**");
				}
				else {
					await guildUser.RemoveRoleAsync(role);
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
			else {
				Context.IsSuccess = false;
				await ReplyAsync($"The role **{role.Name}** is not public!");
			}
		}

		[Command("role setpublic", RunMode = RunMode.Async)]
		[Parameters("<role>")]
		[Summary("Sets the role to private")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageRoles)]
		[RequireContext(ContextType.Guild)]
		public async Task SetPublic([Remainder]string roleName) {
			IRole role = Context.Guild.Roles.FirstOrDefault(
				r => string.Compare(r.Name, roleName, true) == 0);
			if (role == null) {
				Context.IsSuccess = false;
				await ReplyAsync($"No role with the name **{roleName}**");
				return;
			}
			else if (role.Permissions.Administrator) {
				Context.IsSuccess = false;
				await ReplyAsync($"Cannot make the role **{roleName}** public because it has Administrator permission");
				return;
			}
			if (!(await Settings.SetRolePublic(Context, role))) {
				Context.IsSuccess = false;
			}
			else {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("role setprivate", RunMode = RunMode.Async)]
		[Parameters("<role>")]
		[Summary("Sets the role to private")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageRoles)]
		[RequireContext(ContextType.Guild)]
		public async Task SetPrivate([Remainder]string roleName) {
			IRole role = Context.Guild.Roles.FirstOrDefault(
				r => string.Compare(r.Name, roleName, true) == 0);
			if (role == null) {
				Context.IsSuccess = false;
				await ReplyAsync($"No role with the name **{roleName}**");
				return;
			}
			/*else if (role.Permissions.Administrator) {
				Context.IsSuccess = false;
				await ReplyAsync($"Cannot make the role **{roleName}** public because it has Administrator permission");
				return;
			}*/
			if (!(await Settings.SetRolePrivate(Context, role))) {
				Context.IsSuccess = false;
			}
			else {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}
	}
}
