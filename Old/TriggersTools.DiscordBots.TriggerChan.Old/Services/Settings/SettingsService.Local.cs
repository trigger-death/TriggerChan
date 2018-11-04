using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class SettingsService : BotServiceBase {

		public LocalSettingsBase GetLocalSettings(ulong id) {
			LocalGuild guild;
			LocalGuildChannel gChannel;
			LocalGroup group;
			LocalDM dm;

			localGuilds.TryGetValue(id, out guild);
			if (guild != null) return guild;

			localGuildChannels.TryGetValue(id, out gChannel);
			if (gChannel != null) return gChannel;

			localGroups.TryGetValue(id, out group);
			if (group != null) return group;

			localDMs.TryGetValue(id, out dm);
			if (dm != null) return dm;

			return null;
		}

		public LocalSettingsBase GetLocalSettings(SocketCommandContext context) {
			ulong guildId = (context.Guild != null ? context.Guild.Id : 0);
			ulong channelId = (context.Channel != null ? context.Channel.Id : 0);

			if (context.Guild != null) {
				LocalGuild guild;
				localGuilds.TryGetValue(guildId, out guild);
				if (guild == null) {
					guild = new LocalGuild() { Id = guildId };
					localGuilds.TryAddOrGet(guildId, ref guild);
				}
			}
			else if (context.Channel is IGuildChannel) {
				LocalGuildChannel gChannel;
				localGuildChannels.TryGetValue(channelId, out gChannel);
				if (gChannel == null) {
					gChannel = new LocalGuildChannel() { Id = channelId, GuildId = guildId };
					localGuildChannels.TryAddOrGet(channelId, ref gChannel);
				}
				return gChannel;
			}
			else if (context.Channel is IGroupChannel) {
				LocalGroup group;
				localGroups.TryGetValue(channelId, out group);
				if (group == null) {
					group = new LocalGroup() { Id = channelId };
					localGroups.TryAddOrGet(channelId, ref group);
				}
				return group;
			}
			else if (context.Channel is IDMChannel) {
				LocalDM dm;
				localDMs.TryGetValue(channelId, out dm);
				if (dm == null) {
					dm = new LocalDM() { Id = channelId };
					localDMs.TryAddOrGet(channelId, ref dm);
				}
				return dm;
			}
			return null;
		}

		public LocalChannelBase GetLocalChannel(ulong id) {
			LocalGuildChannel gChannel;
			LocalGroup group;
			LocalDM dm;
			
			localGuildChannels.TryGetValue(id, out gChannel);
			if (gChannel != null) return gChannel;

			localGroups.TryGetValue(id, out group);
			if (group != null) return group;

			localDMs.TryGetValue(id, out dm);
			if (dm != null) return dm;

			return null;
		}

		public LocalChannelBase GetLocalChannel(SocketCommandContext context) {
			return GetLocalChannel(context.Channel);
		}

		public LocalChannelBase GetLocalChannel(IChannel channel) {
			ulong id = channel.Id;
				
			if (channel is IGuildChannel)
				return GetLocalGuildChannel(id);
			else if (channel is IGroupChannel)
				return GetLocalGroup(id);
			else if (channel is IDMChannel)
				return GetLocalDM(id);

			return null;
		}

		public LocalGuild GetLocalGuild(ulong id) {
			LocalGuild guild;
			localGuilds.TryGetValue(id, out guild);
			if (guild == null) {
				guild = new LocalGuild() { Id = id };
				localGuilds.TryAddOrGet(id, ref guild);
			}
			return guild;
		}

		public LocalGuildChannel GetLocalGuildChannel(ulong id) {
			LocalGuildChannel gChannel;
			localGuildChannels.TryGetValue(id, out gChannel);
			if (gChannel == null) {
				gChannel = new LocalGuildChannel() { Id = id };
				localGuildChannels.TryAddOrGet(id, ref gChannel);
			}
			return gChannel;
		}

		public LocalGroup GetLocalGroup(ulong id) {
			LocalGroup group;
			localGroups.TryGetValue(id, out group);
			if (group == null) {
				group = new LocalGroup() { Id = id };
				localGroups.TryAddOrGet(id, ref group);
			}
			return group;
		}

		public LocalDM GetLocalDM(ulong id) {
			LocalDM dm;
			localDMs.TryGetValue(id, out dm);
			if (dm == null) {
				dm = new LocalDM() { Id = id };
				localDMs.TryAddOrGet(id, ref dm);
			}
			return dm;
		}

	}
}
