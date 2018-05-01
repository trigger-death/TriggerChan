using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public class RequireUserPermissionOrBotOwnerAttribute : RequireUserPermissionAttribute {

		public RequireUserPermissionOrBotOwnerAttribute(GuildPermission permission) : base(permission) {
		}
		public RequireUserPermissionOrBotOwnerAttribute(ChannelPermission permission) : base(permission) {
		}

		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext baseContext, CommandInfo command, IServiceProvider services) {
			var guildUser = baseContext.User as IGuildUser;

			BotCommandContext context = (BotCommandContext) baseContext;

			if (guildUser != null) {
				if ((await context.Settings.GetGuildUser(guildUser)).IsBotOwner) {
					return await Task.FromResult(PreconditionResult.FromSuccess());
				}
			}

			return await base.CheckPermissionsAsync(baseContext, command, services);
		}
	}
}
