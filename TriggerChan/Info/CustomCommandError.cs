using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Info {
	public enum CustomCommandError {
		Success,
		CommandLocked,
		NotInVoiceChannel,
		InvalidArgument,
		WaitingForUpload,
		DMSent,
	}
}
