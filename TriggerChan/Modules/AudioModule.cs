using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Maya.Music.Youtube;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;

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
			if ((Context.User as IVoiceState).VoiceChannel == null) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "No voice channel to join";
				return;
			}
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

		[Command("play status", RunMode = RunMode.Async), Alias("queue status")]
		[Summary("Gets the current music playback status.")]
		public async Task PlayStatus() {
			await Audio.NewMusicStatus(Context);
		}

		[Command("play skip", RunMode = RunMode.Async), Alias("play next",
			"queue skip", "queue next")]
		[Summary("Skips to the next song.")]
		public async Task PlayNext() {
			Audio.NextSong(Context);
		}

		[Command("play local", RunMode = RunMode.Async), Alias("queue local")]
		[Parameters("<audioFile>")]
		[Summary("Adds the local song to the music queue")]
		public async Task PlayAudio([Remainder] string audioFile) {
			await AddLocal(audioFile);
		}

		[Command("play insert local", RunMode = RunMode.Async), Alias("queue insert local")]
		[Parameters("<index> <audioFile>")]
		[Summary("Inserts the local song into the music queue")]
		public async Task InsertAudio(int index, [Remainder] string audioFile) {
			await AddLocal(audioFile, index);
		}

		private async Task AddLocal(string audioFile, int index = int.MinValue) {
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
			SongInfo song = new SongInfo() {
				Title = Path.GetFileName(audioFile),
				FileName = audioFile,
				Owner = Context.User,
				Channel = Context.Channel as ITextChannel,
			};
			song.GetLocalDetails();
			await AddOrInsertSong(song, index);
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
			Audio.Stop(Context);
		}

		[Command("play youtube"), Alias("play yt", "queue youtube", "queue yt")]
		[Parameters("<url>")]
		[Summary("Adds the youtube video to the music queue")]
		public async Task PlayYouTube(string url) {
			await AddYouTube(url);
		}

		[Command("play insert youtube"), Alias("play insert yt", "queue insert youtube", "queue insert yt")]
		[Parameters("<index> <url>")]
		[Summary("Inserts the youtube video into the music queue")]
		public async Task InsertYouTube(int index, string url) {
			await AddYouTube(url, index);
		}

		private async Task AddYouTube(string url, int index = int.MinValue) {
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
			string finalUrl;
			if (Audio.IsYouTubeUrl(url, out finalUrl)) {
				IUserMessage proxyMessage = null;
				SongDownloadInfo download;
				try {
					download = MusicDownloader.GetDownloadInfo(finalUrl, onProxy: async () => {
						proxyMessage = await ReplyAsync("Attempting to access through a proxy");
					});
				}
				catch (Exception ex) {
					Console.WriteLine(ex.ToString());
					await ReplyAsync($"Failed to aquire YouTube video <{url}>. This video may be region blocked");
					return;
				}
				if (proxyMessage != null)
					await proxyMessage.DeleteAsync();
				download.Owner = Context.User;
				if (download != null) {
					if (download.Duration > TimeSpan.FromMinutes(20)) {
						await ReplyAsync("Duration cannot be greater than 20 minutes");
					}
					else {
						await AddOrInsertSong(download, index);
					}
				}
				else {
					await ReplyAsync("Failed to find YouTube video");
				}
			}
			else {
				await ReplyAsync("Not a valid YouTube url");
			}
		}

		[Command("play upload"), Alias("play attachment", "play attach",
			"queue upload", "queue attachment", "queue attach")]
		[Parameters("<audio attachment>")]
		[Summary("Adds the uploaded audio attachment to the music queue")]
		public async Task PlayAttachment() {
			await AddAttachment();
		}

		[Command("play insert upload"), Alias("play insert attachment", "play insert attach",
			"queue insert upload", "queue insert attachment", "queue insert attach")]
		[Parameters("<index> <audio attachment>")]
		[Summary("Adds the uploaded audio attachment to the music queue")]
		public async Task InsertAttachment(int index) {
			await AddAttachment(index);
		}

		private async Task AddAttachment(int index = int.MinValue) {
			if (!Audio.IsInVoiceSameChannel(Context)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.NotInVoiceChannel;
				Context.ErrorReason = "Not in a voice channel";
				return;
			}
			else if (Context.Message.Attachments.Count == 0) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "No attachments";
				return;
			}

			var attach = Context.Message.Attachments.First();
			string url = attach.Url;

			DiscordSongInfo download = new DiscordSongInfo() {
				Title = Path.GetFileName(attach.Filename),
				DownloadUrl = url,
				Channel = Context.Channel as ITextChannel,
				Owner = Context.User,
			};
			await AddOrInsertSong(download, index);
		}

		private async Task AddOrInsertSong(SongInfo song, int index = int.MinValue) {
			if (index >= 1) {
				index--;
				if (!await Audio.InsertSong(Context, song, index)) {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.InvalidArgument;
					Context.ErrorReason = "Index was not in range";
				}
			}
			else if (index != int.MinValue) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Index was not in range";
			}
			else {
				await Audio.AddSong(Context, song);
			}
		}

		[Command("play remove"), Alias("queue remove")]
		[Parameters("<index>")]
		[Summary("Removes an index from the queue")]
		public async Task QueueRemove(int index) {
			index--;
			if (!(await Audio.RemoveSong(Context, index))) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Index was not in range";
			}
		}

		[Command("play move up"), Alias("queue move up")]
		[Parameters("<index> [distance=1]")]
		[Summary("Moves a queued song up in the list")]
		public async Task QueueMoveUp(int index, int distance = 1) {
			if (distance <= 0) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Distance was not in range";
				return;
			}
			index--;
			if (!(await Audio.MoveSong(Context, index, -distance))) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Indexes were not in range";
			}
		}

		[Command("play move down"), Alias("queue move down")]
		[Parameters("<index> [distance=1]")]
		[Summary("Moves a queued song down in the list")]
		public async Task QueueMoveDown(int index, int distance = 1) {
			if (distance <= 0) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Distance was not in range";
				return;
			}
			index--;
			if (!(await Audio.MoveSong(Context, index, distance))) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Indexes were not in range";
			}
		}

		/*[Command("audio")]
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
		}*/
	}
}
