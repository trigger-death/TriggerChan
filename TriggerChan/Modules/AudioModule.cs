using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Audio")]
	[IsLockable]
	[RequireContext(ContextType.Guild)]
	public class AudioModule : BotModuleBase {
		
		// You *MUST* mark these commands with 'RunMode.Async'
		// otherwise the bot will not respond until the Task times out.
		[Command("voice join", RunMode = RunMode.Async), Alias("audio join")]
		[RequireContext(ContextType.Guild)]
		[Summary("Invite the bot to join the voice channel you're in")]
		public async Task JoinVoice() {
			await Audio.JoinAudio(Context);
		}

		// Remember to add preconditions to your commands,
		// this is merely the minimal amount necessary.
		// Adding more commands of your own is also encouraged.
		[Command("voice leave", RunMode = RunMode.Async), Alias("audio leave")]
		[Summary("Tells the bot to leave the voice channel you're in")]
		public async Task LeaveVoice() {
			if (!Audio.IsInVoiceSameChannel(Context)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "No voice channel to leave";
				return;
			}
			await Audio.LeaveAudio(Context);
		}

		[Command("play", RunMode = RunMode.Async)]
		[Parameters("<audioFile>")]
		[Summary("Plays the local audio file in the voice channel. The file must have an extension.")]
		public async Task PlayAudio([Remainder] string audioFile) {
			if (!Audio.IsInVoiceSameChannel(Context)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "Not in a voice channel";
				return;
			}
			
			audioFile = audioFile.Trim();
			if (audioFile.StartsWith('"') && audioFile.EndsWith('"')) {
				audioFile = audioFile.Substring(1, audioFile.Length - 2);
			}
			if (!BotResources.IsValidAudioFile(audioFile)) {
				string prefix = await Settings.GetPrefix(Context);
				await ReplyAsync($"Audio file does not exist in Audio directory. Use `{prefix}audio` to list all available files");
				return;
			}
			//audioFile = BotResources.GetAudio(audioFile);
			audioFile = "Resources/Audio/" + audioFile;
			await Audio.SendAudioAsync(Context, audioFile);
		}

		[Command("stop", RunMode = RunMode.Async)]
		[Summary("Stops the playback of the current audio")]
		public async Task StopAudio() {
			if (!Audio.IsInVoiceSameChannel(Context)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "Not in a voice channel";
				return;
			}
			await Audio.StopAudio(Context);
		}

		[Command("audio")]
		[Summary("Lists all built-in audio files for playback")]
		public async Task AudioFiles() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Description = "These are the available audio files for use",
			};

			string description = "";
			foreach (string file in BotResources.GetAudioFiles()) {
				description += $"{file.Replace('\\', '/')}\n";
			}

			embed.AddField(x => {
				x.Name = "Audio Files";
				x.Value = description;
				x.IsInline = false;
			});

			await ReplyAsync("", false, embed.Build());
		}

		[Command("youtube"), Alias("yt")]
		[Parameters("<url>")]
		[Summary("Plays a youtube video")]
		public async Task PlayYouTube(string url) {
			if (!Audio.IsInVoiceSameChannel(Context)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "Not in a voice channel";
				return;
			}
			url = url.Trim();
			if (url.StartsWith('<') && url.EndsWith('>')) {
				url = url.Substring(1, url.Length - 2);
			}
			string videoCode;
			if (YouTube.IsYouTubeUrl(url, out videoCode)) {
				string audioFile;
				string ext;
				IMessage message;
				message = await ReplyAsync($"**Aquiring Video Info:** `{videoCode}`");
				using (var typing = Context.Channel.EnterTypingState()) {
					var info = YouTube.GetInfo(Context, url);
					ext = info.Item3;
					string timeStr = info.Item2;
					if (timeStr.Length == 0) {
						timeStr = $"0{timeStr}";
					}
					int colonCount = timeStr.Split(':').Length - 1;
					for (int i = colonCount; i < 2; i++) {
						timeStr = $"00:{timeStr}";
					}
					TimeSpan duration;
					if (!TimeSpan.TryParse(timeStr, out duration)) {
						Console.WriteLine("Failed to parse timespan");
					}
					await message.DeleteAsync();
					message = await ReplyAsync($"**Downloading Video:** {info.Item1} - {timeStr}");
				}
				using (var typing = Context.Channel.EnterTypingState()) {
					audioFile = await YouTube.Download(Context, url, ext);
					/*audioFile = audioFile.Trim();
					if (audioFile.StartsWith('"') && audioFile.EndsWith('"')) {
						audioFile = audioFile.Substring(1, audioFile.Length - 2);
					}*/
					await message.DeleteAsync();
					if (audioFile == null)
						return;
				}
				await Audio.SendAudioAsync(Context, audioFile);
			}
			else {
				await ReplyAsync("Not a valid YouTube url");
			}
		}
	}
}
