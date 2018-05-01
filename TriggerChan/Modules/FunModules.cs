using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Fun")]
	[IsLockable]
	public class FunModule : BotModuleBase {
		[Command("say", RunMode = RunMode.Async)]
		[Parameters("<text>")]
		[Summary("Make the bot say something")]
		public Task Say([Remainder]string text)
			=> ReplyAsync(text);

		[Command("talk", RunMode = RunMode.Async)]
		[Parameters("<text>")]
		[Summary("Make the bot say something with text-to-speech")]
		[RequireUserPermissionOrBotOwner(GuildPermission.SendTTSMessages)]
		public Task Speech([Remainder]string text)
			 => ReplyAsync(text, true);

		[Command("clap", RunMode = RunMode.Async)]
		[Parameters("<text>")]
		[Summary("Insert 👏 claps 👏 between 👏 words")]
		public Task Clap([Remainder]string text) {
			string[] words = text.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
			text = string.Join(" 👏 ", words);
			return ReplyAsync(text);
		}

		[Command("javascript", RunMode = RunMode.Async), Alias("js")]
		[Summary("An image macro from Dagashi Kashi")]
		public async Task JavaScript() {
			await Context.Channel.SendFileAsync(BotResources.JavaScript);
		}

		[Command("merge", RunMode = RunMode.Async), Alias("mc")]
		[Summary("An image macro from New Game")]
		public async Task MergeConflict() {
			await Context.Channel.SendFileAsync(BotResources.Merge_Conflict);
		}

		[Command("culture", RunMode = RunMode.Async), Alias("manofculture")]
		[Summary("Ah, I see you're a man of culture as well")]
		public async Task ManOfCulture() {
			await Context.Channel.SendFileAsync(BotResources.Man_of_Culture);
		}

		[Command("asciify")]
		[Parameters("[smoothness 1-4] [scale%] [nodelete]")]
		[Summary("Asciify an uploaded image. Image must be a png, jpg, or bmp.\nScaled image dimensions must not be larger than 1000x1000")]
		[Remarks("A file to asciify must be uploaded within one minute of entering this command.\n" +
			"The nodelete parameter will keep the message with your attachment if specified.\n" + 
			"Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[RequireContext(ContextType.Guild)]
		public async Task Asciify(int smoothness = 1, float scale = 100.0f, string nodelete = null) {
			LocalGuild guild = Settings.GetLocalGuild(Context.Guild.Id);
			if (guild.IsAsciifying) {
				IUser user = guild.Asciify.User;
				string name = user.GetName(Context.Guild);
				if (guild.Asciify.Task == null)
					await ReplyAsync($"Asciification is waiting for the {name} to upload an attachment");
				else
					await ReplyAsync($"{name} is currently asciifying an image");
				return;
			}
			if (nodelete != null && string.Compare(nodelete, "nodelete") != 0) {
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid nodelete";
				Context.IsSuccess = false;
				return;
			}
			else if (smoothness <= 0 || smoothness > 4) {
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid precision";
				Context.IsSuccess = false;
				return;
			}
			else if (scale <= 0) {
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid scale";
				Context.IsSuccess = false;
				return;
			}
			IAttachment attach = Context.Message.Attachments.FirstOrDefault();
			AsciifyParameters asciify = new AsciifyParameters() {
				Channel = Context.Message.Channel as ITextChannel,
				User = Context.Message.Author,
				Message = Context.Message,
				Attachment = attach,
				TimeStamp = DateTime.UtcNow,
				Smoothness = smoothness,
				Scale = scale / 100.0f,
				Delete = nodelete == null,
			};
			lock (guild) {
				guild.Asciify = asciify;
			}
			if (attach == null) {
				Context.CustomError = CustomCommandError.WaitingForUpload;
				Context.IsSuccess = false;
				asciify.ExpireTimer = new Timer(OnExpire, guild, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(-1));
			}
			else {
				await Fun.AsciifyImage(Context, guild, asciify);
			}
		}
		
		private async void OnExpire(object state) {
			LocalGuild guild = (LocalGuild) state;
			await guild.Asciify.Channel.SendMessageAsync("", false, guild.Asciify.BuildTimeoutEmbed());
			guild.Asciify.ExpireTimer.Dispose();
			guild.Asciify.ExpireTimer = null;
			guild.Asciify = null;
		}

		[Group("pinreact")]
		public class PinReacting : BotModuleBase {
			[Command("", RunMode = RunMode.Async)]
			[Summary("Gets the number of pin reacts required to pin a message")]
			public async Task Get() {
				int count = await Fun.GetPinReactCount(Context);
				if (count == 0) {
					await ReplyAsync("**Pin-React:** disabled");
				}
				else {
					await ReplyAsync($"**Pin-React Requires:** {count} {BotReactions.PinMessage}(s)");
				}
			}

			[Command("set", RunMode = RunMode.Async)]
			[Summary("Sets the number of pin reacts required to pin a message. Use 0 to disable Pin-React")]
			[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
			public async Task Set(int count) {
				if (count < 0) {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.InvalidArgument;
					Context.ErrorReason = "Count cannot be less than zero";
				}
				else {
					await Fun.SetPinReactCount(Context, count);
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
		}

		[Group("talkback")]
		public class TalkBack : BotModuleBase {
			[Command("", RunMode = RunMode.Async)]
			[Summary("Get the current talkback state of the bot. Talkback is when the bot responds to certain phrases said by the user")]
			public async Task Get() {
				bool enabled = await Fun.GetTalkBack(Context);
				await ReplyAsync($"**Talkback:** {enabled}");
			}

			[Command("set", RunMode = RunMode.Async)]
			[Parameters("<enabled>")]
			[Summary("Set the current talkback state of the bot. Talkback is when the bot responds to certain phrases said by the user")]
			[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
			public async Task Set(bool enabled) {
				if (await Fun.SetTalkBack(Context, enabled)) {
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
				else {
					await ReplyAsync($"TalkBack is already set to {enabled}");
				}
			}

			[Group("cooldown")]
			public class Cooldown : BotModuleBase {

				[Command("", RunMode = RunMode.Async)]
				[Summary("Gets the current cooldown before the bot can talkback to users again")]
				public async Task CurrentCooldown() {
					TimeSpan cooldown = await Fun.GetTalkBackCooldown(Context);
					Stopwatch timer = Settings.GetLocalChannel(Context).TalkBackTimer;
					TimeSpan remaining = cooldown - timer.Elapsed;
					if (timer == null || !timer.IsRunning || remaining <= TimeSpan.Zero) {
						await ReplyAsync("**Current Talkback Cooldown:** finished");
					}
					else {
						string time = "";
						if (remaining.Days > 0) {
							string plural = (remaining.Days != 1 ? "s" : "");
							time += string.Format(" {0:%d} day{1}", remaining, plural);
						}
						if (remaining.Hours > 0) {
							string plural = (remaining.Hours != 1 ? "s" : "");
							time += string.Format(" {0:%h} hour{1}", remaining, plural);
						}
						if (remaining.Minutes > 0) {
							string plural = (remaining.Minutes != 1 ? "s" : "");
							time += string.Format(" {0:%m} min{1}", remaining, plural);
						}
						if (remaining.Seconds > 0) {
							string plural = (remaining.Seconds != 1 ? "s" : "");
							time += string.Format(" {0:%s} sec{1}", remaining, plural);
						}
						await ReplyAsync($"**Current Talkback Cooldown:** {time}");
					}
				}

				[Command("reset")]
				[Summary("Resets the current talkback cooldown")]
				[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
				public async Task ResetCooldown() {
					TimeSpan cooldown = await Fun.GetTalkBackCooldown(Context);
					if (cooldown == TimeSpan.Zero) {
						await ReplyAsync("**Talkback Cooldown:** none");
					}
					else {
						await ReplyAsync($"**Talkback Cooldown:** {cooldown.ToDHMSString()}");
					}
				}

				[Command("get", RunMode = RunMode.Async)]
				[Summary("Gets the default talkback cooldown")]
				public async Task GetCooldown() {
					TimeSpan cooldown = await Fun.GetTalkBackCooldown(Context);
					if (cooldown == TimeSpan.Zero) {
						await ReplyAsync("**Talkback Cooldown:** none");
					}
					else {
						await ReplyAsync($"**Talkback Cooldown:** {cooldown.ToDHMSString()}");
					}
				}

				[Command("set", RunMode = RunMode.Async)]
				[Parameters("<[[hh:]mm:]ss>")]
				[Summary("Gets the default talkback cooldown")]
				[Remarks("The format for the time is `hh:mm:ss` or `none`")]
				[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
				public async Task SetCooldown(string time) {
					TimeSpan cooldown;
					bool error = false;
					if (time == "none")
						cooldown = TimeSpan.Zero;
					else {
						int colonCount = time.Split(':').Length - 1;
						for (int i = colonCount; i < 2; i++) {
							time = $"0:{time}";
						}
						if (!TimeSpan.TryParse(time, out cooldown))
							error = true;
					}
					if (!error) {
						await Fun.SetTalkBackCooldown(Context, cooldown);
						Context.IsSuccess = false;
						Context.CustomError = CustomCommandError.Success;
					}
					else {
						Context.IsSuccess = false;
						Context.Error = CommandError.ParseFailed;
						Context.ErrorReason = "Failed to parse time";
					}
				}
			}
		}
	}
}
