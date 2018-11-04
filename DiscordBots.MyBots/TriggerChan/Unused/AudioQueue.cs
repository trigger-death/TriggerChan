using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	public partial class AudioQueue : TriggerService {

		#region General

		/// <summary>
		/// Gets the guild associated with this audio queue.
		/// </summary>
		public IGuild Guild { get; }
		/// <summary>
		/// Gets the queued list of songs.
		/// </summary>
		public ConcurrentQueue<AudioQueueItem> Queue { get; } = new ConcurrentQueue<AudioQueueItem>();

		#endregion

		#region Connection

		/// <summary>
		/// Gets or sets the voice channel the audio queue is streaming in.
		/// </summary>
		public IVoiceChannel Channel { get; set; }
		/// <summary>
		/// Gets or sets the audio client used for sending and streaming music.
		/// </summary>
		public IAudioClient AudioClient { get; set; }
		/// <summary>
		/// Gets or sets the current audio stream for the current voice channel.
		/// </summary>
		public AudioOutStream AudioStream { get; set; }
		/// <summary>
		/// Gets or sets the state of the audio playback.
		/// </summary>
		public AudioState AudioState { get; set; }

		public Process FFMpeg { get; set; }
		/// <summary>
		/// Gets or sets the current song.
		/// </summary>
		public AudioCurrentItem CurrentSong { get; set; }
		/// <summary>
		/// Gets or sets the token for cancelling the current playback.
		/// </summary>
		public CancellationTokenSource Cancel { get; set; }

		public ManualResetEvent CancelComplete { get; } = new ManualResetEvent(true);

		#endregion

		#region Status Message

		/// <summary>
		/// Gets or sets the embed message that displays the playback status.
		/// </summary>
		public IUserMessage StatusMessage { get; set; }
		/// <summary>
		/// Gets or sets the timer for updating the playback status message.
		/// </summary>
		public Timer StatusTimer { get; set; }

		#endregion

		#region Locks

		/// <summary>
		/// Gets the object for locking the audio queue.
		/// </summary>
		public object AudioLock { get; } = new object();
		/// <summary>
		/// Gets the object for locking the audio queue.
		/// </summary>
		public object QueueLock { get; } = new object();
		/// <summary>
		/// Gets the lock used when changing voice channels.
		/// </summary>
		public object VoiceLock { get; } = new object();

		#endregion

		#region Constructors

		public AudioQueue(TriggerServiceContainer services, IGuild guild) : base(services) {
			Guild = guild;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the guild Id for the audio queue.
		/// </summary>
		public ulong Id => Guild.Id;
		/// <summary>
		/// Gets the guild voice channel Id for the audio queue.
		/// </summary>
		public ulong ChannelId => Channel?.Id ?? 0;
		/// <summary>
		/// Gets if the audio queue is currently connected.
		/// </summary>
		public bool IsConnected => Channel != null && Client != null;

		#endregion
	}
}
