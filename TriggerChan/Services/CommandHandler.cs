using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Discord;
using Discord.Rest;
using System.Collections.Generic;
using TriggersTools.DiscordBots.TriggerChan.Context;
using Discord.Commands.Builders;
using TriggersTools.DiscordBots.TriggerChan.Info;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TriggersTools.DiscordBots.TriggerChan.Modules;
using TriggersTools.DiscordBots.TriggerChan.Util;
using System.Diagnostics;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class CommandHandler : BotServiceBase {

		private bool lockableTested;

		protected override void OnInitialized(ServiceProvider services) {
			base.OnInitialized(services);
			Client.MessageReceived += OnMessageReceivedAsync;
			lockableTested = false;
		}
		
		private async Task OnMessageReceivedAsync(SocketMessage s) {
			var msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
			if (msg == null)
				return;
			if (s.Author.IsBot)
				return;     // Ignore bots and self when checking commands
			
			var context = new BotCommandContext(this, msg);     // Create the command context

			string prefix = await Settings.GetPrefix(context);
			int argPos = 0;     // Check if the message has a valid command prefix
			bool hasPrefix = msg.HasStringPrefix(prefix, ref argPos, StringComparison.OrdinalIgnoreCase);
			bool hasMention = msg.HasMentionPrefix(Client.CurrentUser, ref argPos);
			if (hasPrefix || hasMention) {
				var result = await Commands.ExecuteAsync(context, argPos, Services);     // Execute the command

				TestLockables();

				if (result is BotPreconditionResult precon) {
					await BotModuleBase.HandleResult(context, precon.Result);
				}

				CommandError? commandError = result.Error ?? context.Error;
				CustomCommandError? customError = context.CustomError;
				string errorReason = result.ErrorReason ?? context.ErrorReason;
				
				if (!result.IsSuccess || !context.IsSuccess) {    // If not successful, reply with the error.
					// Don't react to unknown commands when mentioning
					if (hasMention && result.Error.HasValue &&
						result.Error == CommandError.UnknownCommand)
						return;

					bool errorStated = false;
					ReactionInfo reaction = null;
					if (commandError.HasValue)
						reaction = BotReactions.GetReaction(commandError.Value.ToString());
					else if (reaction == null && customError.HasValue)
						reaction = BotReactions.GetReaction(customError.Value.ToString());
					if (reaction != null) {
						await msg.AddReactionAsync(reaction.Emoji);
						errorStated = true;
					}
					if (!errorStated) {
						if (context.Exception != null) {
							await msg.AddReactionAsync(BotReactions.Exception);
							Console.WriteLine(context.Exception.ToString());
						}
						else if (result is ExecuteResult exResult) {
							Console.WriteLine(context.Exception.ToString());
						}
						//if (!result.IsSuccess)
						//	await context.Channel.SendMessageAsync(result.ToString());
						//else if (context.ErrorReason != null)
						//	await context.Channel.SendMessageAsync(context.ErrorReason);
					}
				}

			}
		}

		private void TestLockables() {
			if (!lockableTested) {
				lockableTested = true;
				bool first = true;
				foreach (CommandInfo cmd in Commands.Commands) {
					if (!cmd.HasLockable()) {
						if (first) {
							first = false;
							Debug.WriteLine("The following commands have no IsLockableAttribute!");
						}
						Debug.WriteLine(cmd.Aliases.First());
					}
				}
				first = true;
				HashSet<string> groups = new HashSet<string>();
				foreach (ModuleInfo mod in Commands.Modules) {
					ModuleInfo root = mod.RootModule();
					if (!root.HasLockable()) {
						if (first) {
							first = false;
							Debug.WriteLine("The following command groups have no IsLockableAttribute!");
						}
						if (groups.Add(root.Name))
							Debug.WriteLine(root.Name);
					}
				}
			}
		}
	}
}
