using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	/// <summary>
	/// The state for playing audio.
	/// </summary>
	public enum AudioState {
		/// <summary>A song is currently playing.</summary>
		Playing,
		/// <summary>Nothing is playing.</summary>
		Stopped,
		/// <summary>A song is currently downloading.</summary>
		Downloading,
	}
}
