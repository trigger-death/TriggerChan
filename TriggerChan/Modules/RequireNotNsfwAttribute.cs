using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	/// <summary>
	/// Require that the command is invoked in a channel not marked NSFW
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class RequireNotNsfwAttribute : PreconditionAttribute {
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (context.Channel is ITextChannel text && !text.IsNsfw)
				return Task.FromResult(PreconditionResult.FromSuccess());
			else
				return Task.FromResult(PreconditionResult.FromError("This command may only be invoked in an non-NSFW channel."));
		}
	}
}
