using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Audio;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class LavaAudioService : TriggerService {

		private readonly Lavalink lavalink;
		private readonly ConfigParserService configParser;
		private LavaNode node;

		private readonly ConcurrentDictionary<ulong, LavaGuildPlayer> queues = new ConcurrentDictionary<ulong, LavaGuildPlayer>();

		public LavaAudioService(TriggerServiceContainer services,
									ConfigParserService configParser,
									Lavalink lavalink)
			: base(services)
		{
			this.lavalink = lavalink;
			this.configParser = configParser;
			Client.Ready += OnReadyAsync;
			Client.JoinedGuild += OnJoinedGuildAsync;
			Client.LeftGuild += OnLeftGuildAsync;
			//Client.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
		}

		#region Event Handlers

		private async Task OnReadyAsync() {
			node = await lavalink.ConnectAsync(Client, new LavaConfig {
				Authorization = Config["lavalink:password"],
				Endpoint = new Endpoint {
					Host = Config["lavalink:address"],
					Port = ushort.Parse(Config["lavalink:port"]),
				},
			}).ConfigureAwait(false);
			node.Finished += OnPlayerFinishedAsync;
			node.Exception += OnPlayerExceptionAsync;
			node.Updated += OnPlayerUpdatedAsync;
			foreach (var guild in Client.Guilds) {
				var queue = new LavaGuildPlayer(Services, configParser, guild, node);
				queues.TryAdd(guild.Id, queue);
				IVoiceChannel channel = guild.CurrentUser.VoiceChannel;
				if (channel != null) {
					var audioClient = await channel.ConnectAsync().ConfigureAwait(false);
					await Task.Delay(100).ConfigureAwait(false);
					await audioClient.StopAsync().ConfigureAwait(false);
					await queue.JoinAsync(channel, true).ConfigureAwait(false);
				}
			}
		}
		private Task OnPlayerExceptionAsync(LavaPlayer player, LavaTrack track, string message) {
			return GetQueue(player.Guild.Id).ExceptionAsync(track, message);
		}
		private Task OnPlayerFinishedAsync(LavaPlayer player, LavaTrack track, TrackReason reason) {
			return GetQueue(player.Guild.Id).FinishedAsync(track, reason);
		}
		private Task OnPlayerUpdatedAsync(LavaPlayer player, LavaTrack track, TimeSpan position) {
			return GetQueue(player.Guild.Id).UpdatedAsync(track, position);
		}

		private Task OnJoinedGuildAsync(SocketGuild guild) {
			queues.TryAdd(guild.Id, new LavaGuildPlayer(Services, configParser, guild, node));
			return Task.FromResult<object>(null);
		}
		private Task OnLeftGuildAsync(SocketGuild guild) {
			return Task.FromResult<object>(null);
		}

		private Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState) {
			if (user.Id != Client.CurrentUser.Id) return Task.FromResult<object>(null);
			if (oldState.VoiceChannel?.Id != newState.VoiceChannel?.Id) {
				IGuild guild = oldState.VoiceChannel?.Guild ?? newState.VoiceChannel?.Guild;
				var queue = GetQueue(guild.Id);
				if (newState.VoiceChannel == null)
					return queue.LeaveAsync();
				else if (oldState.VoiceChannel != null)
					return queue.JoinAsync(newState.VoiceChannel);
			}
			return Task.FromResult<object>(null);
		}

		#endregion

		#region Queue

		public LavaGuildPlayer GetQueue(ICommandContext context) {
			return GetQueue(context.Guild.Id);
		}
		public LavaGuildPlayer GetQueue(ulong guildId) {
			return queues[guildId];
		}

		#endregion
	}
}
