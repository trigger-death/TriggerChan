using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public class BotModuleBase : ModuleBase<BotCommandContext> {

		protected override void BeforeExecute(CommandInfo command) {
			base.BeforeExecute(command);
			
		}

		protected async override void AfterExecute(CommandInfo command) {
			base.AfterExecute(command);
			await HandleResult(Context, Context);
			/*if (!Context.IsSuccess) {
				bool errorStated = false;
				ReactionInfo reaction = null;
				if (Context.Error.HasValue)
					reaction = BotReactions.GetReaction(Context.Error.Value.ToString());
				else if (reaction == null && Context.CustomError.HasValue)
					reaction = BotReactions.GetReaction(Context.CustomError.Value.ToString());
				if (reaction != null) {
					await Context.Message.AddReactionAsync(reaction.Emoji);
					errorStated = true;
				}
				if (!errorStated) {
					if (Context.Exception != null) {
						await Context.Message.AddReactionAsync(BotReactions.Exception);
						Console.WriteLine(Context.Exception.ToString());
					}
					if (Context.ErrorReason != null) {
						await Context.Channel.SendMessageAsync(Context.ErrorReason);
					}
				}
			}*/
		}

		public static async Task HandleResult(BotCommandContext context, IBotErrorResult result) {
			if (!result.IsSuccess) {
				bool errorStated = false;
				ReactionInfo reaction = null;
				if (result.Error.HasValue)
					reaction = BotReactions.GetReaction(result.Error.Value.ToString());
				else if (reaction == null && result.CustomError.HasValue)
					reaction = BotReactions.GetReaction(result.CustomError.Value.ToString());
				if (reaction != null) {
					await context.Message.AddReactionAsync(reaction.Emoji);
					errorStated = true;
				}
				if (!errorStated) {
					if (result.Exception != null) {
						//await context.Message.AddReactionAsync(BotReactions.Exception);
						Console.WriteLine(result.Exception.ToString());
					}
					if (result.ErrorReason != null) {
						//await context.Channel.SendMessageAsync(result.ErrorReason);
					}
				}
			}
		}

		public DiscordSocketClient Client => Context.Client;
		public CommandService Commands => Context.Commands;
		public IConfigurationRoot Config => Context.Config;
		public IServiceProvider Services => Context.Services;
		public SettingsService Settings => Context.Settings;
		public LoggingService Logging => Context.Logging;
		public AudioService Audio => Context.Audio;
		public YouTubeService YouTube => Context.YouTube;
		public FunService Fun => Context.Fun;
		public SpoilerService Spoilers => Context.Spoilers;
		public HelpService Help => Context.Help;
		public StartupService Startup => Context.Startup;
	}
}
