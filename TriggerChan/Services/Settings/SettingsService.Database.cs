using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class SettingsService : BotServiceBase {


		private static readonly TimeSpan DefaultILoveCooldown = TimeSpan.FromMinutes(10);
		private const string DefaultLockedCommandsStr = "|purge|say|talk|youtube|";
		private const string DefaultLockedModulesStr = "|Audio|";


		private Guild MakeGuild(ulong guildId) {
			return InitSettings(new Guild() {
				GuildId = guildId,
				CleanupOnLeave = false,
				PublicRolesStr = "|",
			});
		}

		private GuildChannel MakeGuildChannel(ulong guildId, ulong channelId) {
			return InitChannel(new GuildChannel() {
				GuildId = guildId,
				ChannelId = channelId,
				LockedCommandsStr = "|",
				LockedModulesStr = "|",
			});
		}

		private Group MakeGroup(ulong groupId) {
			return InitChannel(InitSettings(new Group() {
				GroupId = groupId,
			}));
		}

		private DM MakeDM(ulong dmId) {
			return InitChannel(InitSettings(new DM() {
				DMId = dmId,
			}));
		}

		private GuildUser MakeGuildUser(ulong guildId, ulong userId) {
			return new GuildUser() {
				GuildId = guildId,
				UserId = userId,
				MALUsername = null,
				IsBotOwner = false,
			};
		}

		private TChannel InitChannel<TChannel>(TChannel c) where TChannel : ChannelBase {
			return c;
		}

		private TSettings InitSettings<TSettings>(TSettings s) where TSettings : SettingsBase {
			s.TalkBackCooldown = DefaultILoveCooldown;
			s.Prefix = DefaultPrefix;
			s.LockedCommandsStr = DefaultLockedCommandsStr;
			s.LockedModulesStr = DefaultLockedModulesStr;
			s.TalkBack = true;
			s.PinReactCount = 10;
			return s;
		}

		/*public async Task<SettingsBase> GetSettings(BotDatabaseContext database, ulong id, bool track) {
			Guild guild;
			GuildChannel gChannel;
			Group group;
			DM dm;

			if (track) guild = await database.Guilds.FirstOrDefaultAsync(g => g.Id == id);
			else guild = await database.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (guild != null) return guild;

			if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == id);
			else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (gChannel != null) return gChannel;

			if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == id);
			else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (group != null) return group;

			if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == id);
			else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
			if (dm != null) return dm;
			
			return null;
		}*/
		
		public async Task<SettingsBase> GetSettings(BotDatabaseContext database,
			SocketCommandContext context, bool track)
		{
			ulong guildId = (context.Guild != null ? context.Guild.Id : 0);
			ulong channelId = (context.Channel != null ? context.Channel.Id : 0);

			if (context.Guild != null) {
				Guild guild;
				if (track) guild = await database.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
				else guild = await database.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
				if (guild == null) {
					guild = MakeGuild(guildId);
					database.Guilds.Add(guild);
					database.SaveChanges();
				}
				return guild;
			}
			else if (context.Channel is IGuildChannel) {
				GuildChannel gChannel;
				if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == channelId);
				else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (gChannel == null) {
					gChannel = MakeGuildChannel(guildId, channelId);
					database.GuildChannels.Add(gChannel);
					database.SaveChanges();
				}
				return gChannel;
			}
			else if (context.Channel is IGroupChannel) {
				Group group;
				if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == channelId);
				else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (group == null) {
					group = MakeGroup(channelId);
					database.Groups.Add(group);
					database.SaveChanges();
				}
				return group;
			}
			else if (context.Channel is IDMChannel dmChannel) {
				DM dm;
				if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == channelId);
				else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == channelId);
				if (dm == null) {
					dm = MakeDM(channelId);
					database.DMs.Add(dm);
					database.SaveChanges();
				}
				return dm;
			}
			
			return null;
		}

		public async Task<SettingsBase> GetSettings(BotDatabaseContext database,
			IUserMessage message, bool track)
		{
			ulong channelId = (message.Channel != null ? message.Channel.Id : 0);

			if (message.Channel is IGuildChannel gChannel) {
				ulong guildId = gChannel.GuildId;
				Guild guild;
				if (track) guild = await database.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
				else guild = await database.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
				if (guild == null) {
					guild = MakeGuild(guildId);
					database.Guilds.Add(guild);
					database.SaveChanges();
				}
				return guild;
			}
			/*else if (message.Channel is IGuildChannel) {
				GuildChannel gChannel;
				if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == channelId);
				else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (gChannel == null) {
					gChannel = MakeGuildChannel(guildId, channelId);
					database.GuildChannels.Add(gChannel);
					database.SaveChanges();
				}
				return gChannel;
			}*/
			else if (message.Channel is IGroupChannel) {
				Group group;
				if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == channelId);
				else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (group == null) {
					group = MakeGroup(channelId);
					database.Groups.Add(group);
					database.SaveChanges();
				}
				return group;
			}
			else if (message.Channel is IDMChannel dmChannel) {
				DM dm;
				if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == channelId);
				else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == channelId);
				if (dm == null) {
					dm = MakeDM(channelId);
					database.DMs.Add(dm);
					database.SaveChanges();
				}
				return dm;
			}
			
			return null;
		}

		/*public async Task<SettingsBase> GetSettings(ulong id) {
			using (var database = new BotDatabaseContext()) {
				return await GetSettings(database, id, false);
			}
		}*/

		public async Task<SettingsBase> GetSettings(SocketCommandContext context) {
			using (var database = new BotDatabaseContext()) {
				return await GetSettings(database, context, false);
			}
		}

		public async Task<SettingsBase> GetSettings(IUserMessage message) {
			using (var database = new BotDatabaseContext()) {
				return await GetSettings(database, message, false);
			}
		}

		/*public async Task<ChannelBase> GetChannel(BotDatabaseContext database, ulong id, bool track) {
			GuildChannel gChannel;
			Group group;
			DM dm;
			
			if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == id);
			else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (gChannel != null) return gChannel;

			if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == id);
			else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (group != null) return group;

			if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == id);
			else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
			if (dm != null) return dm;

			return null;
		}*/

		public async Task<ChannelBase> GetChannel(BotDatabaseContext database,
			SocketCommandContext context, bool track)
		{
			ulong guildId = (context.Guild != null ? context.Guild.Id : 0);
			ulong channelId = (context.Channel != null ? context.Channel.Id : 0);

			if (context.Channel is IGuildChannel) {
				GuildChannel gChannel;
				if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == channelId);
				else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (gChannel == null) {
					gChannel = MakeGuildChannel(guildId, channelId);
					database.GuildChannels.Add(gChannel);
					database.SaveChanges();
				}
				return gChannel;
			}
			else if (context.Channel is IGroupChannel) {
				Group group;
				if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == channelId);
				else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
				if (group == null) {
					group = MakeGroup(channelId);
					database.Groups.Add(group);
					database.SaveChanges();
				}
				return group;
			}
			else if (context.Channel is IDMChannel dmChannel) {
				DM dm;
				if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == channelId);
				else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == channelId);
				if (dm == null) {
					dm = MakeDM(channelId);
					database.DMs.Add(dm);
					database.SaveChanges();
				}
				return dm;
			}
			return null;
		}

		public async Task<ChannelBase> GetChannel(IChannel channel) {
			using (var database = new BotDatabaseContext()) {
				GuildChannel gChannel;
				Group group;
				DM dm;
				ulong id = channel.Id;
				
				if (channel is IGuildChannel guildChannel) {
					gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
					if (gChannel == null) {
						gChannel = MakeGuildChannel(guildChannel.GuildId, id);
						database.GuildChannels.Add(gChannel);
						database.SaveChanges();
					}
					return gChannel;
				}
				else if (channel is IGroupChannel) {
					group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
					if (group == null) {
						group = MakeGroup(id);
						database.Groups.Add(group);
						database.SaveChanges();
					}
					return group;
				}
				else if (channel is IDMChannel) {
					dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
					if (dm == null) {
						dm = MakeDM(id);
						database.DMs.Add(dm);
						database.SaveChanges();
					}
					return dm;
				}
			}
			return null;
		}

		/*public async Task<ChannelBase> GetChannel(ulong id) {
			using (var database = new BotDatabaseContext()) {
				return await GetChannel(database, id, false);
			}
		}*/

		public async Task<Guild> GetGuild(BotDatabaseContext database, ulong id, bool track) {
			Guild guild;
			if (track) guild = await database.Guilds.FirstOrDefaultAsync(g => g.Id == id);
			else guild = await database.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (guild == null) {
				guild = MakeGuild(id);
				database.Guilds.Add(guild);
				database.SaveChanges();
			}
			return guild;
		}

		public async Task<Guild> GetGuild(ulong id) {
			using (var database = new BotDatabaseContext()) {
				return await GetGuild(database, id, false);
			}
		}

		public async Task<GuildChannel> GetGuildChannel(BotDatabaseContext database,
			SocketCommandContext context, bool track)
		{
			return await GetGuildChannel(database, context.Guild.Id,
				context.Channel.Id, track);
		}

		public async Task<GuildChannel> GetGuildChannel(BotDatabaseContext database,
			ulong guildId, ulong channelId, bool track)
		{
			GuildChannel gChannel;
			if (track) gChannel = await database.GuildChannels.FirstOrDefaultAsync(g => g.Id == channelId);
			else gChannel = await database.GuildChannels.AsNoTracking().FirstOrDefaultAsync(g => g.Id == channelId);
			if (gChannel == null) {
				gChannel = MakeGuildChannel(guildId, channelId);
				database.GuildChannels.Add(gChannel);
				database.SaveChanges();
			}
			return gChannel;
		}

		public async Task<GuildChannel> GetGuildChannel(ulong guildId, ulong channelId) {
			using (var database = new BotDatabaseContext()) {
				return await GetGuildChannel(database, guildId, channelId, false);
			}
		}

		public async Task<Group> GetGroup(BotDatabaseContext database, ulong id, bool track) {
			Group group;
			if (track) group = await database.Groups.FirstOrDefaultAsync(g => g.Id == id);
			else group = await database.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
			if (group == null) {
				group = MakeGroup(id);
				database.Groups.Add(group);
				database.SaveChanges();
			}
			return group;
		}

		public async Task<Group> GetGroup(ulong id) {
			using (var database = new BotDatabaseContext()) {
				return await GetGroup(database, id, false);
			}
		}

		public async Task<DM> GetDM(BotDatabaseContext database, ulong id, bool track) {
			DM dm;
			if (track) dm = await database.DMs.FirstOrDefaultAsync(d => d.Id == id);
			else dm = await database.DMs.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
			if (dm == null) {
				dm = MakeDM(id);
				database.DMs.Add(dm);
				database.SaveChanges();
			}
			return dm;
		}

		public async Task<DM> GetDM(ulong id) {
			using (var database = new BotDatabaseContext()) {
				return await GetDM(database, id, false);
			}
		}

		public async Task<GuildUser> GetGuildUser(BotDatabaseContext database, ulong guildId, ulong userId, bool track) {
			GuildUser gUser = null;
			if (track) gUser = await database.GuildUsers.FirstOrDefaultAsync(u => u.GuildId == guildId && u.UserId == userId);
			else gUser = await database.GuildUsers.AsNoTracking().FirstOrDefaultAsync(u => u.GuildId == guildId && u.UserId == userId);
			if (gUser == null) {
				gUser = MakeGuildUser(guildId, userId);
				database.GuildUsers.Add(gUser);
				database.SaveChanges();
			}
			return gUser;
		}

		public async Task<GuildUser> GetGuildUser(BotDatabaseContext database, IGuildUser user, bool track) {
			return await GetGuildUser(database, user.GuildId, user.Id, track);
		}

		public async Task<GuildUser> GetGuildUser(BotDatabaseContext database, SocketCommandContext context, bool track) {
			return await GetGuildUser(database, context.Guild.Id, context.User.Id, track);
		}

		public async Task<GuildUser> GetGuildUser(ulong guildId, ulong userId) {
			using (var database = new BotDatabaseContext()) {
				return await GetGuildUser(database, guildId, userId, false);
			}
		}

		public async Task<GuildUser> GetGuildUser(IGuildUser user) {
			using (var database = new BotDatabaseContext()) {
				return await GetGuildUser(database, user.GuildId, user.Id, false);
			}
		}

		public async Task<GuildUser> GetGuildUser(SocketCommandContext context) {
			using (var database = new BotDatabaseContext()) {
				return await GetGuildUser(database, context.Guild.Id, context.User.Id, false);
			}
		}
	}
}
