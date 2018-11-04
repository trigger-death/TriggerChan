using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Spoilers")]
	[IsLockable]
	public class SpoilerModule : BotModuleBase {
		
		[Command("spoiler", RunMode = RunMode.Async), Alias("spoil", "spoilers")]
		[Summary("Hide a spoiler behind a reactable message")]
		[Parameters("[#target_channel] [{spoilerLabel}] <spoilerContent...>")]
		[Remarks("The first attachment uploaded with this command will be included")]
		[RequireContext(ContextType.Guild | ContextType.Group)]
		public async Task Spoiler([Remainder]string content = "") {
			if (string.IsNullOrWhiteSpace(content) && !Context.Message.Attachments.Any()) {
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "No spoiler content or attachment";
				Context.IsSuccess = false;
				return;
			}
			ParsedSpoiler spoiler = await Spoilers.ParseSpoiler(Context, content);
			await Spoilers.WriteSpoiler(spoiler);
		}

		[Command("aspoiler", RunMode = RunMode.Async), Alias("aspoil", "aspoilers")]
		[Summary("A command for mobile users making it possible to create a spoiler with an attachment.")]
		[Parameters("[#target_channel] [{spoilerLabel}] [spoilerContent...]")]
		[Remarks("First, execute the aspoiler command. Next, upload an attachment in a separate message. The spoiler will timeout in 3 minutes if no attachment is uploaded")]
		[RequireContext(ContextType.Guild | ContextType.Group)]
		public async Task MobileSpoiler([Remainder]string content = "") {
			ParsedSpoiler spoiler = await Spoilers.ParseSpoiler(Context, content, true);
			Spoilers.WaitForAttachment(spoiler);
		}
	}
}
