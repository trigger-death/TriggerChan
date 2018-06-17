using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Danbooru;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;
using TriggersTools.SteinsGate;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Fun")]
	[IsLockable]
	public class FunModule : BotModuleBase {
		[Command("say", RunMode = RunMode.Async)]
		[Parameters("<text>")]
		[Summary("Make the bot say something")]
		public Task Say([Remainder]string text)
			=> ReplyAsync(text);

		[Command("adminsay", RunMode = RunMode.Async)]
		[Parameters("<text>")]
		[Summary("Make the bot say something")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		public Task SayAdmin([Remainder]string text)
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
			string[] words = text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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

		[Command("pinreact", RunMode = RunMode.Async)]
		[Summary("Gets the number of pin reacts required to pin a message")]
		public async Task PinReactGet() {
			int count = await Fun.GetPinReactCount(Context);
			if (count == 0) {
				await ReplyAsync("**Pin-React:** disabled");
			}
			else {
				await ReplyAsync($"**Pin-React Requires:** {count} {BotReactions.PinMessage}(s)");
			}
		}
		[Command("pinreact set", RunMode = RunMode.Async)]
		[Summary("Sets the number of pin reacts required to pin a message. Use 0 to disable Pin-React")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		public async Task PinReactSet(int count) {
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
		[Command("talkback", RunMode = RunMode.Async)]
		[Summary("Get the current talkback state of the bot. Talkback is when the bot responds to certain phrases said by the user")]
		public async Task TalkBackGet() {
			bool enabled = await Fun.GetTalkBack(Context);
			await ReplyAsync($"**Talkback:** {enabled}");
		}

		[Command("talkback set", RunMode = RunMode.Async)]
		[Parameters("<enabled>")]
		[Summary("Set the current talkback state of the bot. Talkback is when the bot responds to certain phrases said by the user")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		public async Task TalkbackSet(bool enabled) {
			if (await Fun.SetTalkBack(Context, enabled)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
			else {
				await ReplyAsync($"TalkBack is already set to {enabled}");
			}
		}

		[Command("talkback cooldown", RunMode = RunMode.Async)]
		[Summary("Gets the current cooldown before the bot can talkback to users again")]
		public async Task TalkbackCurrentCooldown() {
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
		[Command("talkback cooldown reset")]
		[Summary("Resets the current talkback cooldown")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		public async Task TalkbackResetCooldown() {
			TimeSpan cooldown = await Fun.GetTalkBackCooldown(Context);
			if (cooldown == TimeSpan.Zero) {
				await ReplyAsync("**Talkback Cooldown:** none");
			}
			else {
				await ReplyAsync($"**Talkback Cooldown:** {cooldown.ToDHMSString()}");
			}
		}

		[Command("talkback cooldown get", RunMode = RunMode.Async)]
		[Summary("Gets the default talkback cooldown")]
		public async Task TalkbackGetCooldown() {
			TimeSpan cooldown = await Fun.GetTalkBackCooldown(Context);
			if (cooldown == TimeSpan.Zero) {
				await ReplyAsync("**Talkback Cooldown:** none");
			}
			else {
				await ReplyAsync($"**Talkback Cooldown:** {cooldown.ToDHMSString()}");
			}
		}

		[Command("talkback cooldown set", RunMode = RunMode.Async)]
		[Parameters("<[[hh:]mm:]ss>")]
		[Summary("Gets the default talkback cooldown")]
		[Remarks("The format for the time is `hh:mm:ss` or `none`")]
		[RequireUserPermissionOrBotOwner(GuildPermission.ManageMessages)]
		public async Task TalkbackSetCooldown(string time) {
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

		/*[Command("danbooru")]
		[Parameters("[[tags=]tag1[,tag2]] [minscore=5] [retries=1-10] [extra=extra,tags]")]
		[Summary("Looks up a random sfw image on danbooru matching the search terms")]
		[Remarks("Danbooru only allows searching for two main tags at once. " +
			"Everything specified in extras will be manually searched for based on retries x 200 posts. " +
			"Tags can start with '-' to be blacklisted and end with `*` to be open-ended.")]
		[RequireNotNsfw]
		public async Task DanbooruSfwSearch([Remainder] string parameters = "") {
			await DanbooruSearchBase(false, DanbooruRating.Safe, parameters);
		}*/

		[Command("danbooru")]
		[Parameters("[rating=sqe] [[tags=]tag1[,tag2]] [minscore=5] [retries=1-10] [extra=extra,tags...]")]
		[Summary("Looks up a random image on danbooru matching the search terms")]
		[Remarks("Danbooru only allows searching for two main tags at once. " +
			"Everything specified in extras will be manually searched for based on retries x 200 posts. " +
			"Tags can start with '-' to be blacklisted and end with `*` to be open-ended. " +
			"Ratings can be any combination of 's' (safe), 'q' (questionable), and 'e' (explicit).")]
		[RequireNsfw]
		public async Task DanbooruNsfwSearch([Remainder] string arguments = "") {
			await DanbooruSearchBase(true, DanbooruRating.Any, arguments);
		}

		[Command("hentai")]
		[Parameters("[rating=sqe] [[tags=]tag1[,tag2]] [minscore=5] [retries=1-10] [extra=extra,tags]")]
		[Summary("Looks up a random (nsfw by default) image on danbooru matching the search terms")]
		[Remarks("Danbooru only allows searching for two main tags at once. " +
			"Everything specified in extras will be manually searched for based on retries x 200 posts. " +
			"Tags can start with '-' to be blacklisted and end with `*` to be open-ended. " +
			"Ratings can be any combination of 's' (safe), 'q' (questionable), and 'e' (explicit).")]
		[RequireNsfw]
		public async Task HentaiSearch([Remainder] string arguments = "") {
			await DanbooruSearchBase(true, DanbooruRating.NotSafe, arguments);
		}

		public async Task DanbooruSearchBase(bool nsfw, DanbooruRating defaultRating, string arguments) {
			IUserMessage searchMessage = null;
			DanbooruPost post = null;
			try {
				if (arguments.Contains('&') || arguments.Contains('?') || arguments.Contains('/')) {
					InvalidArguments("Invalid characters in arguments");
					return;
				}
				bool tagsSpecified = false;
				int? retries = null;
				int? minScore = null;
				DanbooruRating? rating = null;
				string tag1 = null;
				string tag2 = null;
				List<string> extraTags = null;
				string[] args = arguments.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string arg in args) {
					string[] parts = arg.Split(new char[] { '=' });
					if (parts.Length == 1 || parts[0].ToLower() == "tags") {
						if (tagsSpecified) {
							InvalidArguments("Main tags already specified");
							return;
						}
						tagsSpecified = true;
						if (!string.IsNullOrEmpty(parts.Last())) {
							string[] tags = parts.Last().Split(',');
							if (tags.Length > 2) {
								InvalidArguments("More than two main tags");
							}
							else {
								tag1 = tags[0];
								if (tags.Length == 2)
									tag2 = tags[1];
							}
						}
					}
					else {
						switch (parts[0].ToLower()) {
						case "rating":
							if (!nsfw) {
								InvalidArguments("Ratings cannot be specified in a sfw channel");
								return;
							}
							else if (!parts[1].Any()) {
								InvalidArguments("No ratings specified");
								return;
							}
							if (rating.HasValue) {
								InvalidArguments("Rating already specified");
								return;
							}
							DanbooruRating newRating = DanbooruRating.None;
							foreach (char c in parts[1]) {
								switch (c) {
								case 's': newRating |= DanbooruRating.Safe; break;
								case 'q': newRating |= DanbooruRating.Questionable; break;
								case 'e': newRating |= DanbooruRating.Explicit; break;
								default:
									InvalidArguments($"Unknown rating character `{c}`");
									return;
								}
							}
							rating = newRating;
							break;
						case "extra":
						case "extras":
						case "extratags":
							if (extraTags != null) {
								InvalidArguments("Extra tags already specified");
								return;
							}
							extraTags = new List<string>();
							extraTags.AddRange(parts[1].Split(','));
							break;
						case "retries":
							if (retries.HasValue) {
								InvalidArguments("Retries already specified");
								return;
							}
							if (int.TryParse(parts[1], out int result) && result >= 1) {
								retries = result;
							}
							else {
								InvalidArguments("Invalid retries");
								return;
							}
							break;
						case "minscore":
							if (minScore.HasValue) {
								InvalidArguments("Min Score already specified");
								return;
							}
							if (int.TryParse(parts[1], out int result2)) {
								minScore = result2;
							}
							else {
								InvalidArguments("Invalid min score");
								return;
							}
							break;
						default:
							InvalidArguments($"Unknown paramtere `{parts[0]}`");
							return;
						}
					}
				}
				try {
					extraTags = extraTags ?? new List<string>();
					rating = rating ?? defaultRating;
					searchMessage = await ReplyAsync("🔎 Searching for a random danbooru image...");
					post = await Danbooru.Search(rating.Value, tag1, tag2, minScore ?? 5, retries ?? 1, extraTags);
					
				}
				catch (InvalidTagsException ex) {
					if (searchMessage != null)
						await searchMessage.DeleteAsync();
					await ReplyAsync(ex.Message);
					return;
				}
				catch (IllegalNsfwTagsException ex) {
					if (searchMessage != null)
						await searchMessage.DeleteAsync();
					await ReplyAsync(ex.Message);
					return;
				}
			}
			catch (Exception ex) {
				if (searchMessage != null)
					await searchMessage.DeleteAsync();
				await ReplyAsync("An error occurred while searching. The input may have been invalid.");
				return;
			}
			var embed = Danbooru.MakeBaseEmbed();
			if (post == null) {
				embed.WithDescription("No matching results found");
			}
			else {
				if (string.IsNullOrWhiteSpace(arguments))
					arguments = "*none*";
				else
					arguments = Format.Sanitize(arguments);
				embed.WithUrl(post.PostUrl);
				embed.WithDescription(//$"**Post:** <{post.PostUrl}>\n" +
					$"**Arguments:** {arguments}\n" +
					$"**Score:** {post.Score}{(nsfw ? $" | **Rating:** {post.Rating}" : "")}\n" +
					$"{post.LargeFileUrl}");
				embed.WithImageUrl(post.LargeFileUrl);
			}
			IUserMessage message = await ReplyAsync("", false, embed.Build());
			if (post != null)
				await Danbooru.AddReactions(message);
			await searchMessage.DeleteAsync();
		}

		[Command("divergence")]
		[Parameters("<text>")]
		[Summary("Draws the text onto nixie tubes.")]
		public async Task DivergenceMeter([Remainder] string text) {
			try {
				var args = new DivergenceArgs {
					Scale = DivergenceScale.Medium,
					Escape = DivergenceEscape.NewLines,
					Authenticity = DivergenceAuthenticity.Lax,
				};
				using (Bitmap bitmap = Divergence.Draw(text, args))
				using (MemoryStream stream = new MemoryStream()) {
					bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					stream.Position = 0;
					await Context.Channel.SendFileAsync(stream, "Divergence.png");
				}
			}
			catch (Exception ex) {
				await ReplyAsync($"**Error:** {ex.Message}");
			}
		}
	}
}
