using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Model {
	public class ContextBase {
		/// <summary>
		/// The cooldown allowing the bot to respond to user messages talking about it.
		/// </summary>
		public TimeSpan TalkBackCooldown { get; set; }
		/// <summary>
		/// True if talkback is enabled.
		/// </summary>
		public bool TalkBackEnabled { get; set; }
		/// <summary>
		/// The number of "📌" reacts before a message is pinned.
		/// </summary>
		public int PinReactCount { get; set; }
	}
}
