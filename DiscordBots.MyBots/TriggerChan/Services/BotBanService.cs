using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class BotBanService : TriggerService {
		#region Constructors

		public BotBanService(TriggerServiceContainer services) : base(services) {
			Client.JoinedGuild += OnJoinedGuildAsync;
			Client.MessageReceived += OnMessageReceivedAsync;
		}

		private async Task OnMessageReceivedAsync(SocketMessage socketMessage) {
			if (!(socketMessage.Channel is SocketGuild socketGuild)) return;
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(socketGuild.Id).ConfigureAwait(false);
				if (guild.Banned)
					await socketGuild.LeaveAsync().ConfigureAwait(false);
			}
		}

		private async Task OnJoinedGuildAsync(SocketGuild socketGuild) {
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(socketGuild.Id).ConfigureAwait(false);
				if (guild.Banned)
					await socketGuild.LeaveAsync().ConfigureAwait(false);
			}
		}

		public async Task<bool> IsUserBannedAsync(ulong id) {
			using (var db = GetDb<TriggerDbContext>())
				return (await db.FindUserAsync(id).ConfigureAwait(false)).Banned;
		}

		#endregion
	}
}
