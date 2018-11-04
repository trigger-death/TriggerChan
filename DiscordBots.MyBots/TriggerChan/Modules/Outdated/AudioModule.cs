using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Maya.Music.Youtube;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.TriggerChan.Commands;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Audio")]
	[Summary("Commands for audio playback in voice channels")]
	[IsLockable(true)]
	[RequiresContext(ContextType.Guild)]
	public class AudioModule : TriggerModule {

		
		private readonly AudioService audio;
		
		public AudioModule(TriggerServiceContainer services,
						   AudioService audio)
			: base(services)
		{
			this.audio = audio;
		}


		[Group("audio"), Alias("voice")]
		[Usage("join|leave|status")]
		[Summary("Join or leave the voice channel or output the current play status")]
		public class AudioGroup : TriggerModule {
			private readonly AudioService audio;

			public AudioGroup(TriggerServiceContainer services,
							  AudioService audio)
				: base(services)
			{
				this.audio = audio;
			}
			
			[Name("audio join")]
			[Command("join")]
			[Example("Invite the bot to join the voice channel you're in")]
			public async Task<RuntimeResult> JoinVoice() {
				if ((Context.User as IVoiceState).VoiceChannel == null)
					return EmoteResults.FromNotInVoice();
				await audio.JoinAudio(Context).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("audio leave")]
			[Command("leave")]
			[Example("Tells the bot to leave the voice channel you're in")]
			public async Task<RuntimeResult> LeaveVoice() {
				if (!audio.IsInVoiceSameChannel(Context))
					return EmoteResults.FromNotInVoice();
				await audio.LeaveAudio(Context).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("audio status")]
			[Command("status")]
			[Example("Gets the current music playback status")]
			public async Task<RuntimeResult> Status() {
				if (!audio.IsInVoiceSameChannel(Context))
					return EmoteResults.FromNotInVoice();
				await audio.NewMusicStatus(Context).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}

		[Group("play")]
		[Summary("Add or insert music into the audio queue")]
		[Usage("[insert] [yt|upload] <ulr|attachment>")]
		public class PlayGroup : TriggerModule {
			private readonly AudioService audio;

			public PlayGroup(TriggerServiceContainer services,
							 AudioService audio)
				: base(services)
			{
				this.audio = audio;
			}

			/*[Name("play status")]
			[Command("status")]
			[Example("Gets the current music playback status")]
			public async Task<RuntimeResult> Status() {
				if (!audio.IsInVoiceSameChannel(Context))
					return EmoteResults.FromNotInVoice();
				await audio.NewMusicStatus(Context).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("play skip")]
			[Command("skip"), Alias("next")]
			[Example("Skips to the next song in the queue")]
			public Task<RuntimeResult> Next() {
				if (!audio.IsInVoiceSameChannel(Context))
					return Task.FromResult(EmoteResults.FromNotInVoice());
				audio.NextSong(Context);
				return Task.FromResult(NormalResult.FromSuccess());
			}*/

			[Priority(1)]
			[Name("play local <localFile>")]
			[Command("local")]
			[Usage("<localFile>")]
			//[Example(@"C:\World.Execute (Me).mp3", @"Adds the local song *""C:\World.Execute (Me).mp3""* to the music queue")]
			[RequiresSuperuser]
			public async Task<RuntimeResult> Local([Remainder] string audioFile) {
				return await AddLocal(audioFile).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play insert local")]
			[Command("insert local")]
			[Usage("<index> <audioFile>")]
			//[Example(@"1 C:\World.Execute (Me).mp3", @"Inserts the local song *""C:\World.Execute (Me).mp3""* into the first position in the music queue")]
			[RequiresSuperuser]
			public async Task<RuntimeResult> InsertLocal(int index, [Remainder] string audioFile) {
				return await AddLocal(audioFile, index).ConfigureAwait(false);
			}

			[Name("play")]
			[Command("")]
			[Usage("<url>")]
			[Priority(0)]
			[Example(@"https://youtu.be/y6120QOlsfU", "Adds the video *Darude - Sandstorm* to the music queue")]
			public async Task<RuntimeResult> YouTube(string url) {
				return await AddYouTube(url).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play yt")]
			[Command("yt"), Alias("youtube")]
			[Usage("<url>")]
			[Example(@"https://youtu.be/y6120QOlsfU", "Adds the youtube video *Darude - Sandstorm* to the music queue")]
			public async Task<RuntimeResult> Play(string url) {
				return await AddYouTube(url).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play insert")]
			[Command("insert")]
			[Usage("<index> <url>")]
			[Example(@"2 https://www.youtube.com/watch?v=dQw4w9WgXcQ", "Inserts the video *Never Gonna Give you Up* into the second position in the music queue")]
			public async Task<RuntimeResult> Insert(int index, string url) {
				return await AddYouTube(url, index).ConfigureAwait(false);
			}

			[Priority(2)]
			[Name("play insert yt")]
			[Command("insert yt"), Alias("insert youtube")]
			[Usage("<index> <url>")]
			[Example(@"2 https://www.youtube.com/watch?v=dQw4w9WgXcQ", "Inserts the youtube video *Never Gonna Give you Up* into the second position in the music queue")]
			public async Task<RuntimeResult> InsertYouTube(int index, string url) {
				return await AddYouTube(url, index).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play upload")]
			[Command("upload"), Alias("up", "attachment", "attach")]
			[Usage("<attachment>")]
			[Example("Adds the uploaded audio attachment to the music queue")]
			public async Task PlayAttachment() {
				await AddAttachment().ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play insert upload")]
			[Command("insert upload"), Alias("insert up", "insert attachment", "insert attach")]
			[Usage("<index> <attachment>")]
			[Example("Adds the uploaded audio attachment to the music queue")]
			[Example(@"3", "Inserts the uploaded audio attachment into the third position in the music queue")]
			public async Task InsertAttachment(int index) {
				await AddAttachment(index).ConfigureAwait(false);
			}

			private async Task<RuntimeResult> AddOrInsertSong(SongInfo song, int index = int.MinValue) {
				if (index >= 1) {
					index--;
					if (!await audio.InsertSong(Context, song, index).ConfigureAwait(false)) {
						return EmoteResults.FromInvalidArgument("Index was not in range");
					}
				}
				else if (index != int.MinValue) {
					return EmoteResults.FromInvalidArgument("Index was not in range");
				}
				else {
					await audio.AddSong(Context, song).ConfigureAwait(false);
				}
				return NormalResult.FromSuccess();
			}

			private async Task<RuntimeResult> AddLocal(string audioFile, int index = int.MinValue) {
				if (!audio.IsInVoiceSameChannel(Context))
					return EmoteResults.FromNotInVoice();

				audioFile = audioFile.Trim();
				if (audioFile.StartsWith("\"") && audioFile.EndsWith("\"")) {
					audioFile = audioFile.Substring(1, audioFile.Length - 2);
				}
				SongInfo song = new SongInfo() {
					Title = Path.GetFileName(audioFile),
					FileName = audioFile,
					Owner = Context.User,
					Channel = Context.Channel as ITextChannel,
				};
				song.GetLocalDetails();
				await AddOrInsertSong(song, index).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			private async Task<RuntimeResult> AddYouTube(string url, int index = int.MinValue) {
				if (!audio.IsInVoiceSameChannel(Context)) {
					return EmoteResults.FromNotInVoice();
				}
				url = url.Trim();
				if (url.StartsWith("<") && url.EndsWith(">")) {
					url = url.Substring(1, url.Length - 2);
				}
				string finalUrl;
				if (audio.IsYouTubeUrl(url, out finalUrl)) {
					IUserMessage proxyMessage = null;
					SongDownloadInfo download;
					try {
						download = MusicDownloader.GetDownloadInfo(finalUrl, onProxy: async () => {
							proxyMessage = await ReplyAsync("Attempting to access through a proxy").ConfigureAwait(false);
						});
					} catch (Exception ex) {
						Console.WriteLine(ex.ToString());
						await ReplyAsync($"Failed to aquire YouTube video <{url}>. This video may be region blocked").ConfigureAwait(false);
						return NormalResult.FromSuccess(); ;
					}
					if (proxyMessage != null)
						await proxyMessage.DeleteAsync().ConfigureAwait(false);
					download.Owner = Context.User;
					if (download != null) {
						if (download.Duration > TimeSpan.FromMinutes(20)) {
							await ReplyAsync("Duration cannot be greater than 20 minutes").ConfigureAwait(false);
						}
						else {
							await AddOrInsertSong(download, index).ConfigureAwait(false);
						}
					}
					else {
						await ReplyAsync("Failed to find YouTube video").ConfigureAwait(false);
					}
				}
				else {
					await ReplyAsync("Not a valid YouTube url").ConfigureAwait(false);
				}
				return NormalResult.FromSuccess();
			}

			private async Task<RuntimeResult> AddAttachment(int index = int.MinValue) {
				if (!audio.IsInVoiceSameChannel(Context)) {
					return EmoteResults.FromNotInVoice();
				}
				else if (Context.Message.Attachments.Count == 0) {
					return EmoteResults.FromInvalidArgument("No attachments");
				}

				var attach = Context.Message.Attachments.First();
				string url = attach.Url;

				DiscordSongInfo download = new DiscordSongInfo() {
					Title = Path.GetFileName(attach.Filename),
					DownloadUrl = url,
					Channel = Context.Channel as ITextChannel,
					Owner = Context.User,
				};
				await AddOrInsertSong(download, index).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}

		[Group("queue")]
		[Summary("Manage the positions of music in the audio queue")]
		[Usage("remove|move up|move down <index> [<distance>]")]
		public class QueueGroup : TriggerModule {
			private readonly AudioService audio;

			public QueueGroup(TriggerServiceContainer services,
							  AudioService audio)
				: base(services)
			{
				this.audio = audio;
			}

			[Name("queue remove")]
			[Command("remove")]
			[Usage("<index>")]
			[Example("3", "Removes the song at the third position in the queue")]
			public async Task<RuntimeResult> Remove(int index) {
				if (!audio.IsInVoiceSameChannel(Context))
					return EmoteResults.FromNotInVoice();
				index--;
				if (!(await audio.RemoveSong(Context, index).ConfigureAwait(false))) {
					return EmoteResults.FromInvalidArgument("Index was not in range");
				}
				return NormalResult.FromSuccess();
			}

			[Name("queue move up")]
			[Command("move up")]
			[Usage("<index> [<distance>]")]
			[Example("Moves the song forward 1 position in the queue")]
			[Example("2", "Moves the song forward 2 positions in the queue")]
			public async Task<RuntimeResult> MoveUp(int index, int distance = 1) {
				if (distance <= 0) {
					return EmoteResults.FromInvalidArgument("Distance was not in range");
				}
				index--;
				if (!(await audio.MoveSong(Context, index, -distance).ConfigureAwait(false))) {
					return EmoteResults.FromInvalidArgument("Indexes were not in range");
				}
				return NormalResult.FromSuccess();
			}

			[Name("queue move down")]
			[Command("move down")]
			[Usage("<index> [<distance>]")]
			[Example("Moves the song backward 1 position in the queue")]
			public async Task<RuntimeResult> MoveDown(int index, int distance = 1) {
				if (distance <= 0) {
					return EmoteResults.FromInvalidArgument("Distance was not in range");
				}
				index--;
				if (!(await audio.MoveSong(Context, index, distance).ConfigureAwait(false))) {
					return EmoteResults.FromInvalidArgument("Indexes were not in range");
				}
				return NormalResult.FromSuccess();
			}
		}

		[Name("skip")]
		[Command("skip"), Alias("next")]
		[Summary("Skips to the next song in the queue")]
		public Task<RuntimeResult> Next() {
			if (!audio.IsInVoiceSameChannel(Context))
				return Task.FromResult(EmoteResults.FromNotInVoice());
			audio.NextSong(Context);
			return Task.FromResult(NormalResult.FromSuccess());
		}

		[Name("stop")]
		[Command("stop")]
		[Summary("Stops the playback of the current audio and closes the queue")]
		public Task<RuntimeResult> StopAudio() {
			if (!audio.IsInVoiceSameChannel(Context)) {
				return Task.FromResult(EmoteResults.FromNotInVoice());
			}
			audio.Stop(Context);
			return Task.FromResult(NormalResult.FromSuccess());
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
