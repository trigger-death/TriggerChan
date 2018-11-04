using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	/// <summary>
	/// The class for the current <see cref="AudioQueueItem"/> being played.
	/// </summary>
	public class AudioCurrentItem {

		/// <summary>
		/// Gets or sets the FFMpeg process playing the current song.
		/// </summary>
		public Process FFMpeg { get; set; }

		/// <summary>
		/// Gets or sets the cancellation token for the current voice channel.
		/// </summary>
		public CancellationTokenSource Cancel { get; set; }

		/// <summary>
		/// Gets or sets the song that is being played.
		/// </summary>
		public AudioQueueItem Song { get; set; }

		public string FilePath { get; set; }
	}
}
