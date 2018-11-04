using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
	public class LavaAudioModule : TriggerModule {
		
		private readonly LavaAudioService audio;
		
		public LavaAudioModule(TriggerServiceContainer services,
							   LavaAudioService audio)
			: base(services)
		{
			this.audio = audio;
		}

		[Group("play")]
		[Summary("Add music into the audio queue. Search results can only be used when `yt` or `sc` is specified, otherwise a url must be used")]
		[Usage("status|[yt|sc] <ulr|search>")]
		[Remarks("Supports: YouTube | SoundCloud | BandCamp | Twitch | Vimeo")]
		//[Usage("[insert] [yt|upload] <ulr|attachment>")]
		public class PlayGroup : TriggerModule {
			private readonly LavaAudioService audio;

			public PlayGroup(TriggerServiceContainer services,
							 LavaAudioService audio)
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
			[Name("play status")]
			[Command("status"), Alias("queue")]
			[Example("Reposts the audio playback status and queue")]
			public Task<RuntimeResult> Status() {
				return audio.GetQueue(Context).NewStatusAsync(Context);
			}

			[Priority(1)]
			[Name("play local <localFile>")]
			[Command("local")]
			[Usage("<localFile>")]
			// Hide this superuser command from prying eyes
			//[Example(@"C:\World.Execute (Me).mp3", @"Adds the local song *""C:\World.Execute (Me).mp3""* to the music queue")]
			[RequiresSuperuser]
			public Task<RuntimeResult> Local([Remainder] string audioFile) {
				return audio.GetQueue(Context).QueueLocal(Context, audioFile);
				//return await AddLocal(audioFile).ConfigureAwait(false);
			}

			/*[Priority(1)]
			[Name("play insert local")]
			[Command("insert local")]
			[Usage("<index> <audioFile>")]
			//[Example(@"1 C:\World.Execute (Me).mp3", @"Inserts the local song *""C:\World.Execute (Me).mp3""* into the first position in the music queue")]
			[RequiresSuperuser]
			public async Task<RuntimeResult> InsertLocal(int index, [Remainder] string audioFile) {
				return await AddLocal(audioFile, index).ConfigureAwait(false);
			}*/

			[Name("play <url>")]
			[Command("")]
			[Usage("<url>")]
			[Priority(0)]
			[Example(@"https://youtu.be/y6120QOlsfU", "Adds the YouTube video *Darude - Sandstorm* to the music queue")]
			public Task<RuntimeResult> PlayUrl(string url) {
				return audio.GetQueue(Context).QueueUrl(Context, url);
				//return await AddYouTube(url).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play yt <url|search>")]
			[Command("yt"), Alias("youtube")]
			[Usage("<url|search>")]
			[Example(@"https://www.youtube.com/watch?v=dQw4w9WgXcQ", "Adds the YouTube video *Never Gonna Give you Up* to the music queue")]
			[Example(@"Walk Like An Egyptian", "Adds the first YouTube search result for *Walk Like An Egyptian* to the music queue")]
			public Task<RuntimeResult> PlayYouTube([Remainder] string url) {
				return audio.GetQueue(Context).QueueYouTube(Context, url);
				//return await AddYouTube(url).ConfigureAwait(false);
			}

			[Priority(1)]
			[Name("play sc <url|search>")]
			[Command("sc"), Alias("soundcloud")]
			[Usage("<url|search>")]
			[Example(@"https://soundcloud.com/torcht/the-monolith", "Adds the SoundCloud song *Monolith* to the music queue")]
			[Example(@"Despacito", "Adds the first SoundCloud search result for *Despacito* to the music queue")]
			public Task<RuntimeResult> PlaySoundCloud([Remainder] string url) {
				return audio.GetQueue(Context).QueueSoundCloud(Context, url);
				//return await AddYouTube(url).ConfigureAwait(false);
			}

			/*[Priority(1)]
			[Name("play insert")]
			[Command("insert")]
			[Usage("<index> <url>")]
			[Example(@"2 https://www.youtube.com/watch?v=dQw4w9WgXcQ", "Inserts the video *Never Gonna Give you Up* into the second position in the music queue")]
			public async Task<RuntimeResult> Insert(int index, string url) {
				return await AddYouTube(url, index).ConfigureAwait(false);
			}*/

			/*[Priority(2)]
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
			}*/

			/*private async Task<RuntimeResult> AddOrInsertSong(SongInfo song, int index = int.MinValue) {
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
			}*/
		}

		/*[Group("queue")]
		[Summary("Manage the positions of music in the audio queue")]
		[Usage("remove|move up|move down <index> [<distance>]")]
		public class QueueGroup : TriggerModule {
			private readonly LavaAudioService audio;

			public QueueGroup(TriggerServiceContainer services,
							  LavaAudioService audio)
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
		}*/

		[Name("join")]
		[Command("join")]
		[Summary("Invite me to join the voice channel you're in")]
		public Task<RuntimeResult> JoinVoice() {
			return audio.GetQueue(Context).ConnectAsync(Context);
		}

		[Name("leave")]
		[Command("leave")]
		[Summary("Tells me to leave the voice channel you're in")]
		public Task<RuntimeResult> LeaveVoice() {
			return audio.GetQueue(Context).DisconnectAsync(Context);
		}

		[Name("skip")]
		[Command("skip"), Alias("next")]
		[Summary("Skips to the next track in the queue")]
		public Task<RuntimeResult> Next() {
			return audio.GetQueue(Context).NextTrackAsync(Context);
		}

		[Name("stop")]
		[Command("stop")]
		[Summary("Stops the playback of the current track and closes the queue")]
		public Task<RuntimeResult> StopAudio() {
			return audio.GetQueue(Context).StopAsync(Context);
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
