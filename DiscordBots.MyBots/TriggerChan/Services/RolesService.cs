using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class RolesService : TriggerService {
		
		private readonly ConfigParserService configParser;

		/// <summary>
		/// Constructs the <see cref="RolesService"/>.
		/// </summary>
		public RolesService(TriggerServiceContainer services,
							ConfigParserService configParser)
			: base(services)
		{
			this.configParser = configParser;
		}

		public async Task<RuntimeResult> AddRole(ICommandContext context, IRole role) {
			var guildUser = (IGuildUser) context.User;
			if (guildUser.RoleIds.Contains(role.Id)) {
				await context.Channel.SendMessageAsync($"You already have the role `{role.Name}`").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			Guild guild;
			using (var db = GetDb<TriggerDbContext>())
				guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			if (guild != null && guild.PublicRoles.Contains(role.Id) && !role.Permissions.Administrator) {
				try {
					await guildUser.AddRoleAsync(role).ConfigureAwait(false);
				} catch {
					await context.Channel.SendMessageAsync("An error occured. I may not have permission to manage this role.").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
				return EmoteResults.FromSuccess();
			}
			else {
				await context.Channel.SendMessageAsync($"The role `{role.Name}` is not public! Use the command `roles` to see all available roles").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}
		public async Task<RuntimeResult> RemoveRole(ICommandContext context, IRole role) {
			var guildUser = (IGuildUser) context.User;
			if (!guildUser.RoleIds.Contains(role.Id)) {
				await context.Channel.SendMessageAsync($"You do not have the role `{role.Name}`").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			Guild guild;
			using (var db = GetDb<TriggerDbContext>())
				guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			if (guild != null && guild.PublicRoles.Contains(role.Id) && !role.Permissions.Administrator) {
				try {
					await guildUser.RemoveRoleAsync(role).ConfigureAwait(false);
				} catch {
					await context.Channel.SendMessageAsync("An error occured. I may not have permission to manage this role.").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
				return EmoteResults.FromSuccess();
			}
			else {
				await context.Channel.SendMessageAsync($"The role `{role.Name}` is not public! Use the command `roles` to see all available roles.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}
		public async Task<RuntimeResult> MakeRolePublic(ICommandContext context, IRole role) {
			if (role.Permissions.Administrator) {
				await context.Channel.SendMessageAsync($"The role `{role.Name}` cannot be made public because it has the Administrater permission!").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
				if (guild.PublicRoles.Add(role.Id)) {
					db.ModifyOnly(guild, g => g.PublicRoles);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
				else {
					await context.Channel.SendMessageAsync($"The role `{role.Name}` is already a public role!").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
			}
		}
		public async Task<RuntimeResult> MakeRolePrivate(ICommandContext context, IRole role) {
			/*Guild guild;
			using (var db = GetDb<TriggerDbContext>())
				guild = await contexting.FindGuildAsync(db, context.Guild.Id, true).ConfigureAwait(false);
			if (guild.PublicRoles.Remove(role.Id)) {
				return EmoteResults.FromSuccess();
			}
			else {
				await context.Channel.SendMessageAsync($"The role `{role.Name}` is not a public role!").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}*/
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
				if (guild.PublicRoles.Remove(role.Id)) {
					db.ModifyOnly(guild, g => g.PublicRoles);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
				else {
					await context.Channel.SendMessageAsync($"The role `{role.Name}` is not a public role!").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
			}
		}
		public async Task ListRoles(ICommandContext context) {
			EmbedBuilder embed = new EmbedBuilder {
				Color = configParser.EmbedColor,
				Title = $"{context.Guild.Name} Public Roles",
				Description = "The following roles are public and can be assigned or unassigned by anybody with the `iam` and `iamnot` commands."
			};
			Guild guild;
			using (var db = GetDb<TriggerDbContext>())
				guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			List<IRole> roles = new List<IRole>();
			foreach (ulong roleId in guild.PublicRoles) {
				IRole role;
				try {
					role = context.Guild.GetRole(roleId);
					if (role == null || role.Permissions.Administrator)
						continue;
					roles.Add(role);
				} catch { }
			}
			if (roles.Any()) {
				string roleStr = string.Join("\n", roles.Select(r => r.Name));
				embed.AddField("Public Roles", roleStr);
			}
			else {
				embed.AddField("Public Roles", "*There are no public roles*");
			}
			await context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
		}
	}
}
