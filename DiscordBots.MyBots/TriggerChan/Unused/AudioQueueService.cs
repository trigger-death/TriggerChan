using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Audio;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class AudioQueueService : TriggerService {

		#region Constants

		private static readonly TimeSpan StatusInterval = TimeSpan.FromSeconds(5);

		#endregion

		#region Fields

		private readonly ConcurrentDictionary<ulong, AudioQueue> connections = new ConcurrentDictionary<ulong, AudioQueue>();
		private readonly ConfigParserService configParser;

		#endregion

		#region Constructors

		public AudioQueueService(TriggerServiceContainer services,
								 ConfigParserService configParser)
			: base(services)
		{
			this.configParser = configParser;
			Client.Ready += OnReadyAsync;
			//Client.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
		}

		private async Task OnUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState) {
			if (user.Id != Client.CurrentUser.Id) return;
			if (oldState.VoiceChannel?.Id != newState.VoiceChannel?.Id) {
				if (newState.VoiceChannel != null) {
					await VoiceSwitch(newState.VoiceChannel.Guild, newState.VoiceChannel).ConfigureAwait(false);
				}
				else if (oldState.VoiceChannel != null) {
					// We already handle connecting for the first time ourselves
					await VoiceLeave(oldState.VoiceChannel.Guild).ConfigureAwait(false);
				}
			}
		}

		#endregion

		#region Event Handlers

		private async Task OnReadyAsync() {
			Client.Ready -= OnReadyAsync;
			ulong myId = Client.CurrentUser.Id;
			foreach (var guild in Client.Guilds) {
				AudioQueue queue = new AudioQueue { Guild = guild };
				connections.TryAdd(guild.Id, queue);
				foreach (var channel in guild.VoiceChannels) {
					if (channel.Users.Any(u => u.Id == myId)) {
						await VoiceConnect(queue, channel).ConfigureAwait(false);
						break;
					}
				}
			}
			/*foreach (var channel in Client.Guilds.SelectMany(g => g.VoiceChannels)) {
			}*/
		}

		#endregion

		#region Voice State

		public async Task<AudioQueue> VoiceConnect(AudioQueue queue, IVoiceChannel channel) {
			AudioQueue currentQueue = connections.GetOrAdd(queue.Guild.Id, queue);
			if (currentQueue != queue) {
				if (queue != queue2) {
					queue.StatusTimer.Dispose();
				}
			}
		}

		public async Task VoiceJoin(IGuild guild, IVoiceChannel channel) {
			AudioQueue queue = new AudioQueue {
				Guild = guild,
				Channel = channel,
			};
		}
		public async Task VoiceSwitch(IGuild guild, IVoiceChannel channel) {
			AudioQueue queue = new AudioQueue {
				Guild = guild,
				Channel = channel,
			};
		}
		public async Task VoiceLeave(IGuild guild) {
			AudioQueue queue = new AudioQueue {
				Guild = guild,
				Channel = channel,
			};
		}


		#endregion

		#region Queue Actions

		public async void ResetQueue(AudioQueue queue, IVoiceChannel channel) {

		}

		public async void OnUpdateStatus(object state) {
			AudioQueue queue = (AudioQueue) state;
			if (connections.TryGetValue(queue.Guild.Id, out AudioQueue queue2)) {
				if (queue != queue2) {
					queue.StatusTimer.Dispose();
				}
			}
			try {
				await UpdateMusicStatus(info).ConfigureAwait(false);
			} catch { }
		}

		#endregion

		#region Playback


		private async Task PlayAudio(AudioQueue queue) {
			using (var ffmpeg = CreateStream(path))
			using (var audio = info.Client.CreatePCMStream(AudioApplication.Music)) {
				await Task.Delay(400).ConfigureAwait(false);
				if (ffmpeg.HasExited && ffmpeg.ExitCode != 0) {
					await song.Channel.SendMessageAsync($"FFmpeg failed to play song `{song.Title}`").ConfigureAwait(false);
				}
				else {
					try {
						lock (info) {
							info.StartTime = DateTime.UtcNow;
						}
						await ffmpeg.StandardOutput.BaseStream.CopyToAsync(audio, 81920, info.Token.Token).ConfigureAwait(false);
					} catch (OperationCanceledException) {
						await audio.FlushAsync().ConfigureAwait(false);
					} finally {
						await audio.FlushAsync().ConfigureAwait(false);
					}
				}
			}
		}

		#endregion

		#region FFMpeg


		private Process CreateStream(string path) {
			return Process.Start(new ProcessStartInfo {
				FileName = @"ffmpeg",
				Arguments = $"-hide_banner -xerror -loglevel quiet -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				//Arguments = $"-hide_banner -i \"{path}\" -sample_fmt s16 -ar 48000 -ac 2 -acodec libopus -b:a 192k -vbr on -compression_level 10 -map 0:a -f data pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}

		#endregion
	}
}
