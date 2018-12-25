using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;

namespace TriggersTools.DiscordBots.TriggerChan.Commands {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class RequiresHomeGuildAttribute : PreconditionAttribute {
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext contextBase,
			CommandInfo command, IServiceProvider services)
		{
			IDiscordBotCommandContext context = (IDiscordBotCommandContext) contextBase;
			if (context.Guild != null && context.Guild.Id == ulong.Parse(context.Config["ids:discord:home:guild"]))
				return Task.FromResult(PreconditionResult.FromSuccess());
			else
				return Task.FromResult(PreconditionAttributeResult.FromError("This command can only be invoked in the bot's \"Home Server\".", this));
		}

	}
}
