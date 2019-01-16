using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.Utils;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	public class LavaGuildPlayer : DiscordBotService {

		#region Constants

		private const string SoundCloudPattern = @"^https?:\/\/soundcloud\.com\/(\S+\/\S+)$";
		private const string YouTubePattern = @"^https?:\/\/(?:(?:www|m)\.)?(?:youtube\.com\/watch(?:\?v=|\?.+?&v=)|youtu\.be\/)([a-z0-9_-]+)(?:&|$)";
		private static readonly Regex SoundCloudRegex = new Regex(SoundCloudPattern, RegexOptions.IgnoreCase);
		private static readonly Regex YouTubeRegex = new Regex(YouTubePattern, RegexOptions.IgnoreCase);

		//private static readonly TimeSpan StatusInterval = TimeSpan.FromSeconds(5);

		#endregion
		
		#region Fields

		private readonly ConfigParserService configParser;
		private readonly object queueLock = new object();
		/// <summary>
		/// The Lavalink node.
		/// </summary>
		private readonly LavaNode node;
		/// <summary>
		/// The Lavalink player for the current voice channel.
		/// </summary>
		private LavaPlayer player;
		/// <summary>
		/// The track playlist queue
		/// </summary>
		private readonly LavaTrackQueue queue = new LavaTrackQueue();

		/// <summary>
		/// Gets the guild for the player.
		/// </summary>
		public SocketGuild Guild { get; }
		/// <summary>
		/// Gets the owner info for the current track.
		/// </summary>
		public LavaTrackOwner CurrentTrack { get; private set; }

		#endregion

		#region Properties

		/// <summary>
		/// Gets the voice channel the audio queue is streaming in.
		/// </summary>
		public IVoiceChannel Channel => player?.VoiceChannel;
		
		/// <summary>
		/// Gets the track playlist queue
		/// </summary>
		public IReadOnlyList<LavaTrackOwner> Queue => queue;
		/// <summary>
		/// Gets the playback position of current track.
		/// </summary>
		public TimeSpan Position => player?.Position ?? TimeSpan.Zero;
		/// <summary>
		/// Gets the number of tracks in the queue.
		/// </summary>
		public int Count => queue.Count;
		/// <summary>
		/// Gets the track at the specified index in the queue.
		/// </summary>
		/// <param name="index">The index of the track.</param>
		/// <returns>The track at <paramref name="index"/>.</returns>
		public LavaTrackOwner this[int index] => queue[index];

		/// <summary>
		/// Gets or sets the embed message that displays the playback status.
		/// </summary>
		public IUserMessage StatusMessage { get; private set; }

		#endregion

		#region Status Message

		#endregion

		#region Constructors

		public LavaGuildPlayer(DiscordBotServiceContainer services,
							  ConfigParserService configParser,
							  SocketGuild guild, LavaNode node)
			: base(services)
		{
			this.configParser = configParser;
			Guild = guild;
			this.node = node;
		}

		#endregion

		#region Service Event Handlers
		
		internal Task ExceptionAsync(LavaTrack track, string message) {
			return Task.FromResult<object>(null);
		}
		internal async Task FinishedAsync(LavaTrack track, TrackReason reason) {
			//if (track != null)
			//	trackOwners.TryRemove(track, out _);
			switch (reason) {
			case TrackReason.LoadFailed:
				// Let the user know "I failed, I failed, I failed"
				var channel = (ITextChannel) StatusMessage?.Channel ?? CurrentTrack?.Channel;
				if (channel != null)
					await channel.SendMessageAsync($"I failed to load *{Format.Sanitize(track.Title)}*\n*I failed! I failed! I failed! I failed!*").ConfigureAwait(false);
				goto case TrackReason.Finished;
			case TrackReason.Finished:
				// We need to take action
				await NextTrackAsync().ConfigureAwait(false);
				break;

			case TrackReason.Stopped:
			case TrackReason.Cleanup:
				CurrentTrack = null;
				break;

			case TrackReason.Replaced:
				// CurrentTrack is already assigned
				break;
			}
		}
		internal Task UpdatedAsync(LavaTrack track, TimeSpan position) {
			//Debug.WriteLine(LastStatusWatch.Elapsed);
			//if (LastStatusWatch.Elapsed >= StatusInterval)
				return UpdateStatusAsync();
			//return Task.FromResult<object>(null);
		}

		#endregion

		public async Task<RuntimeResult> ConnectAsync(ICommandContext context) {
			var channel = (context.User as IVoiceState)?.VoiceChannel;
			if (channel != null) {
				await JoinAsync(channel).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			return EmoteResults.FromNotInVoice();
		}
		public async Task<RuntimeResult> DisconnectAsync(ICommandContext context) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			await DisconnectAsync().ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}

		public async Task JoinAsync(IVoiceChannel channel, bool rejoin = false) {
			player = await node.JoinAsync(channel).ConfigureAwait(false);
			CurrentTrack = null;
			//Queue = player.Queue.GetOrAdd(Guild.Id, Queue);
		}

		public async Task LeaveAsync() {
			player?.Stop();
			await node.LeaveAsync(Guild.Id).ConfigureAwait(false);
			lock (queueLock) {
				player = null;
				queue.Clear();
				CurrentTrack = null;
			}
			await DeleteStatusAsync().ConfigureAwait(false);
		}
		public async Task DisconnectAsync() {
			await LeaveAsync().ConfigureAwait(false);
		}

		public async Task<RuntimeResult> StopAsync(ICommandContext context) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			lock (queueLock) {
				player.Stop();
				queue.Clear();
				CurrentTrack = null;
			}
			await UpdateStatusAsync().ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}

		public bool IsInVoiceChannel => Guild.CurrentUser.VoiceChannel != null;
		public bool IsInSameVoiceChannel(ICommandContext context) {
			IVoiceChannel userChannel = (context.User as IVoiceState).VoiceChannel;
			return (Channel != null && userChannel != null && Channel.Id == userChannel.Id);
		}

		private bool FilterYouTubeUrl(ref string query) {
			Match match;
			if ((match = YouTubeRegex.Match(query)).Success) {
				query = $@"https://youtube.com/watch?v={match.Groups[1].Value}";
				//return await QueueYouTube(context, query, index).ConfigureAwait(false);
				return true;
			}
			return false;
		}
		private bool FilterSoundCloudUrl(ref string query) {
			Match match;
			if ((match = SoundCloudRegex.Match(query)).Success) {
				query = $@"https://soundcloud.com/{match.Groups[1].Value}";
				//return await QueueYouTube(context, query, index).ConfigureAwait(false);
				return true;
			}
			return false;
		}

		public async Task<RuntimeResult> QueueLocal(ICommandContext context, string path, int index = -1) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			path = path.Trim();
			if (path.StartsWith("\"") && path.EndsWith("\""))
				path = path.Substring(1, path.Length - 2);
			//Uri uri = new Uri(path);
			LavaTrack track = (await node.GetTracksAsync(path).ConfigureAwait(false)).Tracks.FirstOrDefault();
			return await HandleQueueTrackAsync(context, path, false, true, track, index).ConfigureAwait(false);
		}
		private async Task<RuntimeResult> QueueUrlFinal(ICommandContext context, string path, bool isUrl, int index = -1) {
			Uri uri = new Uri(path);
			LavaTrack track = (await node.GetTracksAsync(path).ConfigureAwait(false)).Tracks.FirstOrDefault();
			return await HandleQueueTrackAsync(context, path, isUrl, false, track, index).ConfigureAwait(false);
		}
		public async Task<RuntimeResult> QueueUrl(ICommandContext context, string query, int index = -1) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			// This may be a search string and not a url, but doesn't matter.
			query = UrlFormat.StripBraces(query, out bool isUrl);
			if (isUrl) {
				if (!FilterYouTubeUrl(ref query))
					FilterSoundCloudUrl(ref query);
				return await QueueUrlFinal(context, query, isUrl, index).ConfigureAwait(false);
			}
			query = $"`{Format.Sanitize(query)}`";
			await context.Channel.SendMessageAsync($"You didn't give me a url: {query}").ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}
		public async Task<RuntimeResult> QueueSoundCloud(ICommandContext context, string query, int index = -1) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			// This may be a search string and not a url, but doesn't matter.
			query = UrlFormat.StripBraces(query, out bool isUrl);
			if (FilterSoundCloudUrl(ref query))
				return await QueueUrlFinal(context, query, isUrl, index).ConfigureAwait(false);
			LavaTrack track = (await node.SearchSoundCloudAsync(query).ConfigureAwait(false)).Tracks.FirstOrDefault();
			return await HandleQueueTrackAsync(context, query, isUrl, false, track, index).ConfigureAwait(false);
		}
		public async Task<RuntimeResult> QueueYouTube(ICommandContext context, string query, int index = -1) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			// This may be a search string and not a url, but doesn't matter.
			query = UrlFormat.StripBraces(query, out bool isUrl);
			if (FilterYouTubeUrl(ref query))
				return await QueueUrlFinal(context, query, isUrl, index).ConfigureAwait(false);
			LavaTrack track = (await node.SearchYouTubeAsync(query).ConfigureAwait(false)).Tracks.FirstOrDefault();
			return await HandleQueueTrackAsync(context, query, isUrl, false, track, index).ConfigureAwait(false);
		}

		private async Task QueueTrackAsync(ICommandContext context, LavaTrack track, int index) {
			lock (queueLock) {
				LavaTrackOwner trackOwner = new LavaTrackOwner(context, track);
				/*if (!trackOwners.TryAdd(track, owner)) {
					Console.WriteLine("THIS SHOULDN'T HAPPEN?");
				}*/
				// TODO: Queue insert
				if (CurrentTrack == null) {
					player.Play(track);
					CurrentTrack = new LavaTrackOwner(context, track);
					//await NewStatusAsync(trackOwners[track].Channel).ConfigureAwait(false);
				}
				else {
					queue.Enqueue(context, track);
					//player.Enqueue(track);
					//Queue.AddLast(track);
				}
			}
			await NewStatusAsync(CurrentTrack.Channel).ConfigureAwait(false);
		}

		private async Task<RuntimeResult> HandleQueueTrackAsync(ICommandContext context, string query, bool isUrl, bool isLocal, LavaTrack track, int index) {
			if (track != null) {
				await QueueTrackAsync(context, track, index).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			else {
				if (isLocal) {
					query = $"*{Format.Sanitize(query)}*";
					await context.Channel.SendMessageAsync($"I couldn't find a track at the file path {query}").ConfigureAwait(false);
				}
				if (isUrl) {
					query = $"<{query}>";
					await context.Channel.SendMessageAsync($"I couldn't find a track at {query}. The track may be unavailable in my country").ConfigureAwait(false);
				}
				else {
					query = $"`{Format.Sanitize(query)}`";
					await context.Channel.SendMessageAsync($"Could not find any search results for {query}").ConfigureAwait(false);
				}
			}
			return NormalResult.FromSuccess();
		}

		private async Task<bool> NextTrackAsync(bool skip = false) {
			LavaTrackOwner trackOwner;
			lock (queueLock) {
				trackOwner = queue.FirstOrDefault();
				if (trackOwner != null) {
					player.Play(trackOwner);
					CurrentTrack = queue.Dequeue();
				}
				else {
					CurrentTrack = null;
				}
			}
			if (trackOwner != null)
				await NewStatusAsync(CurrentTrack.Channel).ConfigureAwait(false);
			else
				await UpdateStatusAsync().ConfigureAwait(false);
			return (trackOwner != null);
		}
		public async Task<RuntimeResult> NextTrackAsync(ICommandContext context) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			await NextTrackAsync(true).ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}

		/*public async Task<RuntimeResult> QueueAttachment(ICommandContext context, string attachmentUrl, int index = -1) {

		}*/

		#region Status

		public async Task<RuntimeResult> NewStatusAsync(ICommandContext context) {
			if (!IsInSameVoiceChannel(context))
				return EmoteResults.FromNotInVoice();
			await NewStatusAsync((ITextChannel) context.Channel).ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}

		private async Task NewStatusAsync(ITextChannel channel) {
			channel = channel ?? CurrentTrack.Channel ?? ((ITextChannel) StatusMessage.Channel);
			if (channel == null)
				return;
			//LastStatusWatch.Restart();
			if (StatusMessage != null) {
				await DeleteStatusAsync().ConfigureAwait(false);
			}
			var embed = BuildStatusEmbed();
			StatusMessage = await channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
		}

		private async Task DeleteStatusAsync() {
			if (StatusMessage != null) {
				IUserMessage deletedMessage = StatusMessage;
				StatusMessage = null;
				try {
					await deletedMessage.DeleteAsync().ConfigureAwait(false);
				} catch { }
			}
		}
		
		private async Task UpdateStatusAsync() {
			if (StatusMessage != null) {
				//LastStatusWatch.Restart();
				var embed = BuildStatusEmbed();
				try {
					await StatusMessage.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
				} catch (HttpException ex) {
					if (ex.HttpCode == HttpStatusCode.NotFound) {
						// Deleted, post a new message
						try {
							StatusMessage = await StatusMessage.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
						} catch { }
					}
				}
				//LastStatusWatch.Restart();
			}
		}

		private Embed BuildStatusEmbed() {
			var embed = new EmbedBuilder() {
				Title = $"{configParser.EmbedPrefix}Not Playing",
				Color = configParser.EmbedColor,
			};
			if (CurrentTrack != null) {
				LavaTrack track = CurrentTrack;
				LavaTrackOwner owner = CurrentTrack;
				string title;
				string description =
					$"__{owner.User.GetName(Guild, true)}'s Choice__\n" +
					$"{Format.Sanitize(track.Title)}\n";
				/*if (info.CurrentSong is SongDownloadInfo download &&
					download.DownloadStatus == DownloadStatus.InProgress) {
					title = "Downloading <a:processing:526524731487158283> ";
					if (download.Proxy != null) {
						title += " though proxy";
					}
				}
				else {*/
				title = $"{configParser.EmbedPrefix}Playing <a:equalizer:526524761065652224> ";
					description += $"`{player.Position.ToPlayString()} / {track.Length.ToPlayString()}`";
				//}
				embed.WithTitle(title);
				embed.WithDescription(description);
			}
			IEnumerable<string> ListTracks() {
				int index = 0;
				foreach (LavaTrack track in Queue) {
					index++;
					string item = $"`{index})` {Format.Sanitize(track.Title)}";
					if (track.Length != TimeSpan.Zero)
						item += $" `[{track.Length.ToPlayString()}]`";
					yield return item;
				}
				if (index == 0)
					yield return "*No queued tracks*";
			}
			/*string queuedList = "";
			foreach (LavaTrack track in Queue) {
				count++;
				queuedList += $"`{count})` {Format.Sanitize(track.Title)}";
				if (track.Length != TimeSpan.Zero)
					queuedList += $" `[{track.Length.ToPlayString()}]`";
				queuedList += "\n";
			}
			embed.AddField($"Queued Tracks: {Queue.Count}", queuedList);
			if (string.IsNullOrEmpty(queuedList))
				queuedList = "*No queued tracks*";*/
			embed.PaginateField($"Queued Tracks: {Queue.Count}", ListTracks());
			return embed.Build();
		}

		#endregion
	}
}
