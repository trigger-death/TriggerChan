using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class SettingsService : BotServiceBase {
		private ConcurrentDictionary<ulong, LocalGuild> localGuilds;
		private ConcurrentDictionary<ulong, LocalGuildChannel> localGuildChannels;
		private ConcurrentDictionary<ulong, LocalGroup> localGroups;
		private ConcurrentDictionary<ulong, LocalDM> localDMs;

		public string DefaultPrefix => Config["Prefix"];

		public SettingsService() {
			localGuilds = new ConcurrentDictionary<ulong, LocalGuild>();
			localGuildChannels = new ConcurrentDictionary<ulong, LocalGuildChannel>();
			localGroups = new ConcurrentDictionary<ulong, LocalGroup>();
			localDMs = new ConcurrentDictionary<ulong, LocalDM>();
		}

		protected override void OnInitialized(ServiceProvider services) {
			base.OnInitialized(services);
			//Client.JoinedGuild += OnJoinedGuild;
			Client.LeftGuild += OnLeftGuild;
		}

		/*private async Task OnJoinedGuild(SocketGuild arg) {
			
		}*/

		private async Task OnLeftGuild(SocketGuild arg) {
			/*using (var database = new BotDatabaseContext()) {
				Guild guild = await database.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == arg.Id);
				if (guild != null) {
					database.Guilds.Remove(guild);
					await database.SaveChangesAsync();
				}
			}*/
		}
		
		
	}
}
