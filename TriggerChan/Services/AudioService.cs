using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class AudioService : BotServiceBase {

		private class AudioInfo {
			public IVoiceChannel Channel { get; }
			public IAudioClient Client { get; set; }
			public CancellationTokenSource Token { get; }
			public bool IsPlaying { get; set; }

			public AudioInfo(IVoiceChannel channel, IAudioClient client) {
				Channel = channel;
				Client = client;
				Token = new CancellationTokenSource();
				IsPlaying = false;
			}
		}

		private readonly ConcurrentDictionary<ulong, AudioInfo> connections;
		
		public AudioService() {
			connections = new ConcurrentDictionary<ulong, AudioInfo>();
		}

		protected override async void OnInitialized(ServiceProvider services) {
			base.OnInitialized(services);
			foreach (SocketGuild guild in Client.Guilds) {
				foreach (var channel in guild.VoiceChannels) {
					if (channel.Users.Any(u => u.Id == Client.CurrentUser.Id)) {
						await JoinAudio(guild, channel, true);
					}
				}
			}
		}

		public async Task JoinAudio(SocketCommandContext context, bool rejoin = false) {
			IVoiceChannel channel = (context.User as IVoiceState).VoiceChannel;
			await JoinAudio(context.Guild, channel, rejoin);
		}
		public async Task JoinAudio(IGuild guild, IVoiceChannel channel, bool rejoin = false) {
			AudioInfo info;
			if (connections.TryGetValue(guild.Id, out info)) {
				if (channel.Id != info.Channel.Id) {
					connections.TryRemove(guild.Id, out info);
				}
				else {
					if (rejoin) {
						await info.Client.StopAsync();
						info.Client = await channel.ConnectAsync();
						info.IsPlaying = false;
					}
					return;
				}
			}
			if (channel.Guild.Id != guild.Id) {
				return;
			}

			info = new AudioInfo(channel, await channel.ConnectAsync());
			if (rejoin) {
				await info.Client.StopAsync();
				info = new AudioInfo(channel, await channel.ConnectAsync());
			}

			if (connections.TryAdd(guild.Id, info)) {
				// If you add a method to log happenings from this service,
				// you can uncomment these commented lines to make use of that.
				//await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
			}
		}

		public async Task<bool> StopAudio(SocketCommandContext context) {
			IVoiceChannel channel = (context.User as IVoiceState).VoiceChannel;
			AudioInfo info;
			if (connections.TryGetValue(context.Guild.Id, out info)) {
				bool sameChannel = (info.Channel.Id == channel.Id);
				if (sameChannel) {
					info.Token.Cancel();
					info.IsPlaying = false;
					await JoinAudio(context.Guild, info.Channel, true);
				}
				return sameChannel;
				//await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
			}
			return false;
		}



		public async Task<bool> LeaveAudio(SocketCommandContext context) {
			IVoiceChannel channel = (context.User as IVoiceState).VoiceChannel;
			AudioInfo info;
			if (connections.TryRemove(context.Guild.Id, out info)) {
				bool sameChannel = (info.Channel.Id == channel.Id);
				if (sameChannel) {
					await info.Client.StopAsync();
				}
				return sameChannel;
				//await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
			}
			return false;
		}

		public async Task SendAudioAsync(SocketCommandContext context, string path) {
			// Your task: Get a full path to the file if the value of 'path' is only a filename.
			if (!File.Exists(path)) {
				await context.Channel.SendMessageAsync("File does not exist.");
				return;
			}
			AudioInfo info;
			if (connections.TryGetValue(context.Guild.Id, out info)) {
				if (info.IsPlaying) {
					await JoinAudio(context, true);
					//info = connections[guild.Id];
				}
				info.IsPlaying = true;
				//await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
				using (var ffmpeg = CreateStream(path))
				using (var audio = info.Client.CreatePCMStream(AudioApplication.Music)) {
					//try {
					//byte[] data = File.ReadAllBytes(path);
					//await stream.WriteAsync(data, 0, data.Length);
					//stream.Flush();
					//}
					try {
						await ffmpeg.StandardOutput.BaseStream.CopyToAsync(audio, 81920, info.Token.Token);
					}
					catch (OperationCanceledException) {
						await audio.FlushAsync();
					}
					finally {
						await audio.FlushAsync();
					}
				}
			}
		}

		private Process CreateStream(string path) {
			return Process.Start(new ProcessStartInfo {
				FileName = @"C:\Users\Onii-chan\Downloads\Discord.Net-Example-1.0\Discord.Net-Example-1.0\src\bin\Debug\netcoreapp2.0\ffmpeg.exe",
				Arguments = $"-hide_banner -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}

		public bool IsInVoiceSameChannel(SocketCommandContext context) {
			IVoiceChannel channel = (context.User as IVoiceState).VoiceChannel;
			AudioInfo info;
			if (connections.TryGetValue(context.Guild.Id, out info)) {
				return (channel != null && info.Channel.Id == channel.Id);
			}
			return false;
		}
	}
}
