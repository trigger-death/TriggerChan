using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using TriggersTools.DiscordBots.TriggerChan.Info;

namespace TriggersTools.DiscordBots.TriggerChan.Context {
	public class BotErrorResult : IBotErrorResult, IResult {
		
		public Exception Exception { get; set; }
		public CustomCommandError? CustomError { get; set; }
		public CommandError? Error { get; set; }
		public string ErrorReason { get; set; }
		public bool IsSuccess { get; set; } = true;
	}

	public interface IBotErrorResult : IResult {
		Exception Exception { get; }
		CustomCommandError? CustomError { get; }
	}
}
