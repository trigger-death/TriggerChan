using Discord.Commands;
using System;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class IsLockableAttribute : PreconditionAttribute {

		public bool IsLockable { get; }

		public IsLockableAttribute(bool lockable = true) {
			IsLockable = lockable;
		}

		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext baseContext, CommandInfo command,
			IServiceProvider services)
		{
			BotCommandContext context = (BotCommandContext) baseContext;

			if (await context.Settings.IsCommandLocked(context, command)) {
				var error = new BotErrorResult() {
					CustomError = CustomCommandError.CommandLocked,
					ErrorReason = "The command is locked and cannot be used",
					IsSuccess = false,
				};
				return new BotPreconditionResult(error);
			}
			return PreconditionResult.FromSuccess();
		}
	}

	public class BotPreconditionResult : PreconditionResult {

		public IBotErrorResult Result { get; }

		public BotPreconditionResult(IBotErrorResult result) : base(CommandError.Unsuccessful, null) {
			Result = result;
		}
	}
}
