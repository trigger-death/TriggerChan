using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class SettingsService : BotServiceBase {

		public async Task<string> GetPrefix(SocketCommandContext context) {
			string prefix = (await GetChannel(context.Channel)).Prefix;
			if (prefix == null)
				return (await GetGuild(context.Guild.Id)).Prefix;
			return prefix;
		}

		public async Task<string> GetGuildPrefix(SocketCommandContext context) {
			return (await GetGuild(context.Guild.Id)).Prefix;
		}

		/*public async Task<string> GetPrefix(ulong id) {
			return (await GetSettings(id)).Prefix;
		}*/

		public async Task<string> GetChannelPrefix(SocketCommandContext context) {
			return (await GetGuildChannel(context.Guild.Id, context.Channel.Id)).Prefix;
		}

		/*public async Task<string> GetChannelPrefix(ulong guildId, ulong channelId) {
			return (await GetGuildChannel(guildId, channelId)).Prefix;
		}*/

		public async Task SetPrefix(SocketCommandContext context, string prefix) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await GetSettings(database, context, true);
				settings.Prefix = prefix;
				database.UpdateSettings(settings);
				await database.SaveChangesAsync();
				/*if (context.Guild != null) {
					Guild guild = await GetGuild(database, context.Guild.Id, true, false);
					guild.Prefix = prefix;
					database.Guilds.Update(guild);
				}
				else if (context.Channel is IGuildChannel) {
					GuildChannel gChannel = await GetGuildChannel(database, context.Channel.Id, true, false);
					gChannel.Prefix = prefix;
					database.GuildChannels.Update(gChannel);
				}
				else if (context.Channel is IGroupChannel) {
					Group group = await GetGroup(database, context.Channel.Id, true, false);
					group.Prefix = prefix;
					database.Groups.Update(group);
				}
				else if (context.Channel is IDMChannel) {
					DM dm = await GetDM(database, context.Channel.Id, true, false);
					dm.Prefix = prefix;
					database.DMs.Update(dm);
				}*/
			}
		}

		public async Task SetChannelPrefix(SocketCommandContext context, string prefix) {
			using (var database = new BotDatabaseContext()) {
				GuildChannel channel = await GetGuildChannel(database, context, true);
				channel.Prefix = prefix;
				database.GuildChannels.Update(channel);
				await database.SaveChangesAsync();
				/*if (context.Channel is IGuildChannel) {
					GuildChannel gChannel = await GetGuildChannel(database, context.Guild.Id, context.Channel.Id, true, false);
					gChannel.Prefix = prefix;
					database.GuildChannels.Update(gChannel);
				}
				else if (context.Channel is IGroupChannel) {
					Group group = await GetGroup(database, context.Channel.Id, true, false);
					group.Prefix = prefix;
					database.Groups.Update(group);
				}
				else if (context.Channel is IDMChannel) {
					DM dm = await GetDM(database, context.Channel.Id, true, false);
					dm.Prefix = prefix;
					database.DMs.Update(dm);
				}
				await database.SaveChangesAsync();*/
			}
		}

	}
}
