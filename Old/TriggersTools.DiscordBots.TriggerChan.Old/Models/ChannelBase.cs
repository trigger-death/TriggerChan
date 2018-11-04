using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public abstract class ChannelBase : SettingsBase {
		//public int LastMessageCount { get; set; }
		//public DateTime LastMessageTimeStamp { get; set; }
	}

	public abstract class LocalChannelBase : LocalSettingsBase {

		public Stopwatch TalkBackTimer { get; set; }
	}
}
