using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Utils;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.SpoilerBot.Modules;
using TriggersTools.DiscordBots.SpoilerBot.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Fun")]
	[Summary("Fun and useful commands")]
	[IsLockable(true)]
	public class FunModule : TriggerModule {

		private readonly MALApiDowntimeService malApiDowntime;
		private readonly DivergenceService divergence;
		private readonly AsciifyService asciify;
		private readonly TalkBackService talkBack;

		public FunModule(TriggerServiceContainer services,
						 MALApiDowntimeService malApiDowntime,
						 DivergenceService divergence,
						 AsciifyService asciify,
						 TalkBackService talkBack)
			: base(services)
		{
			this.malApiDowntime = malApiDowntime;
			this.divergence = divergence;
			this.asciify = asciify;
			this.talkBack = talkBack;
		}

		[Group("spoiler")]//, Alias("spoilers", "spoil", "spoiled")]
		[Usage("[raw] [up|upload|remove <last|messageId>|rename <last|messageId> <title...>] [<{title}>] [<content...>]")]
		[Summary("Hide a spoiler behind a message that can be viewed by reacting with 🔍")]
		[Remarks("`content...` must be included when the first argument is not present\nIt's common courtesy to include a `{title}` with all spoilers")]
		[RequiresContext(ContextType.Guild)]
		public class SpoilerGroup : TriggerModule {

			#region Fields

			private readonly ISpoilerService spoilers;
			private readonly HelpService help;

			#endregion

			#region Constructors

			public SpoilerGroup(TriggerServiceContainer services,
								ISpoilerService spoilers,
								HelpService help)
				: base(services)
			{
				this.spoilers = spoilers;
				this.help = help;
			}

			#endregion
			
			[Name("spoiler <content>")]
			[Command("")]
			[Priority(0)]
			[Example("{Steins;Gate} 1.048596", "Hide *1.048596* behind a spoiler")]
			public Task<RuntimeResult> Spoiler([Remainder] string content) {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), false, false);
			}

			[Name("spoiler raw <content>")]
			[Command("raw")]
			[Priority(1)]
			[Example("{What!?} Navy Seals Copypasta", "Post a raw spoiler that has room for the entire Navy Seals copypasta")]
			public Task<RuntimeResult> RawSpoiler([Remainder] string content) {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), false, true);
			}

			[Name("spoiler upload [content]")]
			[Command("upload"), Alias("up")]
			[Priority(2)]
			[Example("{Incoming Attachment}", "Send an attachment to your DM to complete the spoiler")]
			[AllowBots(false)]
			public Task<RuntimeResult> AttachmentSpoiler([Remainder] string content = "") {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), true, false);
			}

			[Name("spoiler raw upload [content]")]
			[Command("raw upload"), Alias("raw up", "upload raw", "up raw")]
			[Priority(3)]
			[AllowBots(false)]
			public Task<RuntimeResult> RawAttachmentSpoiler([Remainder] string content = "") {
				return spoilers.RunSpoilerAsync(Context, ResolveContent(content), true, true);
			}

			[Name("spoiler rename last")]
			[Command("rename last")]
			[Priority(2)]
			[Example("New Title Bois!", "Rename the title of your last posted spoiler in this channel to *New Title Bois!*")]
			public Task<RuntimeResult> RenameLastSpoiler([Remainder] string title) {
				return spoilers.RenameLastSpoilerAsync(Context, title);
			}

			[Name("spoiler rename <messageId>")]
			[Command("rename")]
			[Priority(1)]
			[Example("499990259434782720 New Title Bois!", "Rename the title of your spoiler with this Id to *New Title Bois!*")]
			public Task<RuntimeResult> RenameSpoiler(ulong id, [Remainder] string title) {
				return spoilers.RenameSpoilerAsync(Context, id, title);
			}

			[Name("spoiler remove last")]
			[Command("remove last")]
			[Priority(2)]
			[Example("Remove your last posted spoiler in this channel")]
			public Task RemoveLastSpoiler() {
				return spoilers.RemoveLastSpoilerAsync(Context);
			}

			[Name("spoiler remove <messageId>")]
			[Command("remove")]
			[Priority(1)]
			[Example("499990259434782720", "Remove your spoiler with this Id")]
			public Task RemoveSpoiler(ulong id) {
				return spoilers.RemoveSpoilerAsync(Context, id);
			}

			/// <summary>
			/// For those poor fools who typed `s;command help` instead of `s;help command`.
			/// </summary>
			[Name("spoiler help")]
			[Command("help")]
			[Priority(3)]
			public async Task SpoilerHelp() {
				CommandDetails command = Commands.CommandSet.FindCommand("spoiler", Context);
				if (command == null)
					await ReplyAsync($"No command with the name `spoiler` exists").ConfigureAwait(false);
				else if (!command.IsUsable(Context))
					await ReplyAsync($"The command `{command.Alias}` cannot be used").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildCommandHelpAsync(Context, command).ConfigureAwait(false)).ConfigureAwait(false);
			}

			private string ResolveContent(string content) {
				if (string.IsNullOrWhiteSpace(content))
					return string.Empty;
				int index = Context.Message.Content.LastIndexOf(content);
				return Context.Message.Resolve(index, everyoneHandling: TagHandling.Name, emojiHandling: TagHandling.Ignore);
			}
		}

		[Name("clap <text>")]
		[Command("clap")]
		[Usage("<text...>")]
		[Summary("Insert 👏 claps 👏 between 👏 words")]
		[Example("HTML isn't a programming language", "Outputs: *HTML 👏 isn't 👏 a 👏 programming 👏language)")]
		public Task Clap([Remainder] string text) {
			string[] words = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			text = string.Join(" 👏 ", words);
			return ReplyAsync(text);
		}
		[Group("vaporwave"), Alias("vapor", "aesthetic")]
		[Usage("[quote] <text...>")]
		[Summary("Ｃｒｅａｔｅ　ａｅｓｔｈｅｔｉｃａｌｌｙ　ｐｌｅａｓｉｎｇ　ｔｅｘｔ")]
		public class VarporwaveGroup : TriggerModule {
			
			private readonly Dictionary<char, char> fullWidthMap = new Dictionary<char, char>();
			
			public VarporwaveGroup(TriggerServiceContainer services) : base(services) {
				fullWidthMap.Add(' ', '　');
				for (char c = '!'; c <= '~'; c++) {
					fullWidthMap.Add(c, (char) (c + 0xFF00 - ' '));
				}
				fullWidthMap.Add('⦅', '｟');
				fullWidthMap.Add('⦆', '｠');
				fullWidthMap.Add('｡', '。');
				fullWidthMap.Add('｢', '「');
				fullWidthMap.Add('｣', '」');
				fullWidthMap.Add('､', '、');
				fullWidthMap.Add('¢', '￠');
				fullWidthMap.Add('£', '￡');
				fullWidthMap.Add('¬', '￢');
				fullWidthMap.Add('¦', '￤');
				fullWidthMap.Add('¥', '￥');
				fullWidthMap.Add('₩', '￦');
			}

			[Name("vaporwave <text>")]
			[Command("")]
			[Example("Running in the 90's", "Outputs: *Ｒｕｎｎｉｎｇ　ｉｎ　ｔｈｅ　９０＇ｓ*")]
			public Task Vaporwave([Remainder] string text) {
				return ReplyAsync(VaporwaveText(text));
			}
			[Name("vaporwave quote <text>")]
			[Command("quote")]
			[Example("Nattive Patties", "Outputs: *【Ｎａｔｔｉｖｅ　Ｐａｔｔｉｅｓ】*")]
			[Priority(1)]
			public Task VaporwaveQuote([Remainder] string text) {
				return ReplyAsync($"【{VaporwaveText(text)}】");
			}

			private string VaporwaveText(string text) {
				char[] letters = text.ToCharArray();
				//return string.Join(" ", letters);
				for (int i = 0; i < letters.Length; i++) {
					if (fullWidthMap.TryGetValue(letters[i], out char c))
						letters[i] = c;
				}
				return new string(letters);
			}
		}
		/*[Name("vaporwave <text>")]
		[Command("vaporwave"), Alias("vapor", "aesthetic")]
		[Usage("<text...>")]
		[Summary("C r e a t e   a e s t h e t i c l y   p l e a s i n g   t e x t")]
		[Example("Running in the 90's", "Outputs: *R u n n i n g   i n   t h e   9 0 ' s*")]
		public Task Vaporwave([Remainder] string text) {
			char[] letters = text.ToCharArray();
			//text = string.Join(" ", letters);
			//return ReplyAsync(text);
			for (int i = 0; i < letters.Length; i++) {
				if (fullWidthMap.TryGetValue(letters[i], out char c))
					letters[i] = c;
			}
			return ReplyAsync(new string(letters));
		}
		[Name("vaporwavequote <text>")]
		[Command("vaporwave quote"), Alias("vapor quote", "aesthetic quote")]
		[Usage("<text...>")]
		[Summary("C r e a t e   a e s t h e t i c l y   p l e a s i n g   t e x t")]
		[Example("quote Nattive Patties", "Outputs: *【Ｎａｔｔｉｖｅ　Ｐａｔｔｉｅｓ】*")]
		[Priority(1)]
		public Task VaporwaveQuote([Remainder] string text) {
			char[] letters = text.ToCharArray();
			//text = string.Join(" ", letters);
			//return ReplyAsync(text);
			for (int i = 0; i < letters.Length; i++) {
				if (fullWidthMap.TryGetValue(letters[i], out char c))
					letters[i] = c;
			}
			return ReplyAsync("【" + new string(letters) + "】");
		}*/

		[Name("tasts")]
		[Command("tasts")]
		[Summary("Show the true meaning of everyone's \"tasts\"")]
		public Task Tasts() {
			return Context.Channel.SendFileAsync(TriggerResources.Tasts);
		}

		[Name("javascript")]
		[Command("javascript"), Alias("js")]
		[Summary("An image macro from Dagashi Kashi about Javascript")]
		public Task JavaScript() {
			return Context.Channel.SendFileAsync(TriggerResources.JavaScript);
		}

		[Name("merge")]
		[Command("merge"), Alias("mergeconflict")]
		[Summary("An image macro from New Game about Merge Conflicts")]
		public Task MergeConflict() {
			return Context.Channel.SendFileAsync(TriggerResources.MergeConflict);
		}

		[Name("shock")]
		[Command("shock"), Alias("shocked")]
		[Summary("An image macro for being shocked")]
		public Task Shock() {
			return Context.Channel.SendFileAsync(TriggerResources.Shock);
		}

		[Name("swat")]
		[Command("swat"), Alias("police")]
		[Summary("Open the door and put your hands up!")]
		public Task Swat() {
			return Context.Channel.SendFileAsync(TriggerResources.Swat);
		}

		[Name("modabuse")]
		[Command("modabuse"), Alias("mod abuse")]
		[Summary("An image macro for when mods are abusing their authority")]
		public Task ModAbuse() {
			return Context.Channel.SendFileAsync(TriggerResources.ModAbuse);
		}

		[Name("nonono")]
		[Command("nonono"), Alias("no no no")]
		[Summary("No, no, no... BAD!")]
		public Task NoNoNo() {
			return Context.Channel.SendFileAsync(TriggerResources.NoNoNo);
		}

		[Name("culture")]
		[Command("culture", RunMode = RunMode.Async), Alias("manofculture")]
		[Summary("Ah, I see you're a man of culture as well")]
		public Task ManOfCulture() {
			return Context.Channel.SendFileAsync(TriggerResources.ManOfCulture);
		}

		[Name("divergence <text>")]
		[Command("divergence"), Alias("div", "nixie")]
		[Usage("<text...>")]
		[Summary("Draws the text onto divergence meter-style nixie tubes")]
		[Remarks("Max Line Length: 24 | Max Lines: 3")]
		public async Task Divergence([Remainder] string text) {
			try {
				using (Bitmap bitmap = divergence.Draw(text, false))
					await ReplyBitmapAsync(bitmap, "Divergence.png").ConfigureAwait(false);
			} catch (Exception ex) {
				await ReplyAsync($"**Error:** {ex.Message}").ConfigureAwait(false);
			}
		}
		[Name("downtime")]
		[Command("downtime"), Alias("api")]
		[Summary("Display the downtime for MyAnimeList's API")]
		[Remarks("Fix your shit DeNA!")]
		public async Task Downtime() {
			using (Bitmap bitmap = malApiDowntime.Draw())
				await ReplyBitmapAsync(bitmap, "MALApiDowntime.png").ConfigureAwait(false);
		}

		
		[Group("ocarina")]
		[Usage("[play] <notes...>|notes|aliases")]
		[Summary("Create Ocarina of Time style music sheets")]
		public class OcarinaGroup : TriggerModule {

			#region Fields

			private readonly OcarinaService ocarina;
			private readonly ConfigParserService configParser;
			private readonly HelpService help;

			#endregion

			#region Constructors

			public OcarinaGroup(TriggerServiceContainer services,
								OcarinaService ocarina,
								ConfigParserService configParser,
								HelpService help)
				: base(services)
			{
				this.ocarina = ocarina;
				this.configParser = configParser;
				this.help = help;
			}

			#endregion

			[Name("ocarina <notes>")]
			[Command("")]
			[Priority(0)]
			[Example("v a v a > v > v", "Bolero of Fire")]
			public Task PlayShort([Remainder] string notes) {
				return Play(notes);
			}

			[Name("ocarina play <notes>")]
			[Command("play")]
			[Priority(1)]
			[Example("r d a d r u", "Plays Oath to Order")]
			public async Task Play([Remainder] string notes) {
				try {
					using (Context.Channel.EnterTypingState()) {
						byte[] bytes = ocarina.DrawStaffsToBytes(notes);
						using (MemoryStream stream = new MemoryStream(bytes))
							await Context.Channel.SendFileAsync(stream, "Ocarina.png").ConfigureAwait(false);
					}
				} catch (FormatException ex) {
					await ReplyAsync($"Could not parse note: `{ex.Message}`").ConfigureAwait(false);
				}
			}

			[Name("ocarina notes")]
			[Command("notes"), Alias("aliases")]
			[Priority(1)]
			[Example("List the aliases for all ocarina notes")]
			public async Task Notes() {
				const string Separator = " **:** ";
				EmbedBuilder embed = new EmbedBuilder {
					Title = $"{configParser.EmbedPrefix}**Ocarina Notes** <:ocarina:526524354591195138>",
					Description = "The following words and emoji can be used to play a note",
					Color = configParser.EmbedColor,
				};
				string Ocarina = $"<:ocarina:526524354591195138>";
				NoteAliases[] noteAliases = ocarina.NoteAliases;
				StringBuilder str = new StringBuilder();
				//str.AppendLine($"{Ocarina} **Ocarina Notes** {Ocarina}");
				foreach (NoteAliases note in noteAliases) {
					str.Append(note.Emote?.ToString() ?? $"**`{note.Note.ToString()}`**");
					str.Append(Separator);
					str.AppendLine(string.Join(" ", note.Aliases));
					//embed.AddField(note.Emote?.ToString() ?? $"*{note.Note.ToString()}*", string.Join(" ", note.Aliases));
				}
				embed.AddField("Notes", str.ToString());

				NoteAliases[] specialAliases = ocarina.SpecialAliases;
				str.Clear();
				//str.AppendLine($"{Ocarina} **Ocarina Notes** {Ocarina}");
				foreach (NoteAliases special in specialAliases) {
					str.Append($"**`{special.Note.ToString()}`**");
					str.Append(Separator);
					str.AppendLine(string.Join(" ", special.Aliases));
					//embed.AddField(note.Emote?.ToString() ?? $"*{note.Note.ToString()}*", string.Join(" ", note.Aliases));
				}
				embed.AddField("Special", str.ToString());
				//str.AppendLine("`New Staff` **:** `newline`");
				//await ReplyAsync(str.ToString());
				await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
			}

			/// <summary>
			/// For those poor fools who typed `s;command help` instead of `s;help command`.
			/// </summary>
			[Name("ocarina help")]
			[Command("help")]
			[Priority(3)]
			public async Task OcarinaHelp() {
				CommandDetails command = Commands.CommandSet.FindCommand("ocarina", Context);
				if (command == null)
					await ReplyAsync($"No command with the name `ocarina` exists").ConfigureAwait(false);
				else if (!command.IsUsable(Context))
					await ReplyAsync($"The command `{command.Alias}` cannot be used").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildCommandHelpAsync(Context, command).ConfigureAwait(false)).ConfigureAwait(false);
			}
		}

		[Command("pinreact")]
		[Name("pinreact"), Alias("pinreacts")]
		[Summary("Gets the number of 📌 reacts required to pin a message")]
		[Remarks("Pin-React allows users to decide if a message should be pinned without the need of a moderator.\n" +
			"If enough users react to a message with 📌, then I'll pin the message to the channel.")]
		[Priority(0)]
		public async Task GetPinReact() {
			int count = 0;
			using (var db = GetDb<TriggerDbContext>())
				count = (await db.FindGuildAsync(Context).ConfigureAwait(false)).PinReactCount;
			if (count == 0) {
				await ReplyAsync("**Pin-React:** disabled").ConfigureAwait(false);
			}
			else {
				await ReplyAsync($"**Pin-React Requires:** {count} {TriggerReactions.PinMessage}(s)").ConfigureAwait(false);
			}
		}

		/*[Name("asciifyaa")]
		[Command("asciifyaa")]
		[Usage("[smoothness<1-3> [<scale%> [nodelete]]]")]
		[Summary("Asciify an uploaded image. Image must be a png, jpg, or bmp.\nScaled image dimensions must not be larger than 1000x1000")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" + 
				 "Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[Example("3 200", "Will create a smoth asciification at 2x the image resolution")]
		[Example("1 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public async Task<RuntimeResult> AsciifyImageAsciiArtist(int smoothness = 1, float scale = 100.0f, string nodelete = null) {
			if (nodelete != null && string.Compare(nodelete, "nodelete") != 0) {
				return EmoteResults.FromInvalidArgument("Invalid nodelete");
			}
			else if (smoothness <= 0 || smoothness > 3) {
				return EmoteResults.FromInvalidArgument("Invalid smoothness");
			}
			else if (scale <= 0) {
				return EmoteResults.FromInvalidArgument("Invalid scale");
			}
			IAttachment attach = Context.Message.Attachments.FirstOrDefault();
			AsciifyTask asciifyTask = new AsciifyTask() {
				Channel = Context.Message.Channel as ITextChannel,
				User = Context.Message.Author,
				Message = Context.Message,
				Attachment = attach,
				TimeStamp = DateTime.UtcNow,
				//Sharp = sharp,
				Smoothness = smoothness,
				Scale = scale / 100.0f,
				Delete = nodelete == null,
			};
			if (attach == null) {
				await ReplyAsync($"You must upload an image attachment to asciify").ConfigureAwait(false);
			}
			else {
				await asciify.Asciify(Context, asciifyTask, false).ConfigureAwait(false);
			}
			return NormalResult.FromSuccess();
		}*/

		[Name("asciify")]
		[Command("asciify")]
		[Usage("[dot|section] [<scale%>] [nodelete]")]
		[Summary("Asciify an uploaded image. Image must be a `png`, `jpg`, or `bmp`.\nScaled image dimensions must not be larger than 2048x2048")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" +
				"Dot asciification scores the average of the entire character while Sectioned scores portions of the character.")]
		[Example("section 200", "Will create a smooth asciification at 2x the image resolution")]
		[Example("dot 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public async Task<RuntimeResult> AsciifyImageMode([Remainder] string input = "") {//Mode mode = Mode.Dot, double scale = 100d, NoDelete? nodelete = null) {
			string[] args = input.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			bool? sectioned = null;
			double? scale = null;
			bool? noDelete = null;

			foreach (string arg in args.Select(a => a.ToLower())) {
				if (arg == "dot" || arg == "section" || arg == "sectioned") {
					if (sectioned.HasValue)
						return EmoteResults.FromInvalidArgument();
					sectioned = (arg != "dot");
				}
				else if (arg == "nodelete") {
					if (noDelete.HasValue)
						return EmoteResults.FromInvalidArgument();
					noDelete = true;
				}
				else if (double.TryParse(arg, out double parsedScale) && parsedScale > 0) {
					if (scale.HasValue)
						return EmoteResults.FromInvalidArgument();
					scale = parsedScale;
				}
				else {
					return EmoteResults.FromInvalidArgument();
				}
			}
			IAttachment attach = Context.Message.Attachments.FirstOrDefault();
			if (attach == null) {
				await ReplyAsync($"You must upload an image attachment to asciify").ConfigureAwait(false);
			}
			else {
				AsciifyTask asciifyTask = new AsciifyTask() {
					Channel = Context.Message.Channel as ITextChannel,
					User = Context.Message.Author,
					Message = Context.Message,
					Attachment = attach,
					TimeStamp = DateTime.UtcNow,
					Sectioned = sectioned ?? false,
					//Smoothness = smoothness,
					Scale = (scale ?? 100d) / 100d,
					Delete = !(noDelete ?? false),
				};
				await asciify.Asciify(Context, asciifyTask).ConfigureAwait(false);
			}
			return NormalResult.FromSuccess();
		}

		/*[Group("asciify")]
		[Usage("[dot|section] [<scale%>] [nodelete]")]
		[Summary("Asciify an uploaded image. Image must be a `png`, `jpg`, or `bmp`.\nScaled image dimensions must not be larger than 2048x2048")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" +
				 "Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[Example("section 200", "Will create a smooth asciification at 2x the image resolution")]
		[Example("dot 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public class AsciifyGroup : TriggerModule {

			private readonly AsciifyService asciify;

			public AsciifyGroup(TriggerServiceContainer services,
								AsciifyService asciify)
				: base(services)
			{
				this.asciify = asciify;
			}

			private enum NoDelete {
				NoDelete,
			}
			private enum Mode {
				Dot,
				Sectioned,
			}


			
		}

		[Name("asciify")]
		[Command("asciify")]
		[Usage("[[dot|section] [<scale%> [nodelete]]]")]
		[Summary("Asciify an uploaded image. Image must be a `png`, `jpg`, or `bmp`.\nScaled image dimensions must not be larger than 2048x2048")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" +
				 "Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[Example("section 200", "Will create a smooth asciification at 2x the image resolution")]
		[Example("dot 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public async Task<RuntimeResult> AsciifyImageAsciiArtist(string mode = "dot", float scale = 100.0f, string nodelete = null) {
			mode = mode.ToLower();
			if (nodelete != null && string.Compare(nodelete, "nodelete", true) != 0) {
				return EmoteResults.FromInvalidArgument("Invalid nodelete");
			}
			else if (mode != "section" && mode != "no") {
				return EmoteResults.FromInvalidArgument("Invalid mode");
			}
			else if (scale <= 0) {
				return EmoteResults.FromInvalidArgument("Invalid scale");
			}
			IAttachment attach = Context.Message.Attachments.FirstOrDefault();
			AsciifyTask asciifyTask = new AsciifyTask() {
				Channel = Context.Message.Channel as ITextChannel,
				User = Context.Message.Author,
				Message = Context.Message,
				Attachment = attach,
				TimeStamp = DateTime.UtcNow,
				Sectioned = mode != "dot",
				//Smoothness = smoothness,
				Scale = scale / 100.0f,
				Delete = nodelete == null,
			};
			if (attach == null) {
				await ReplyAsync($"You must upload an image attachment to asciify").ConfigureAwait(false);
			}
			else {
				await asciify.Asciify(Context, asciifyTask).ConfigureAwait(false);
			}
			return NormalResult.FromSuccess();
		}*/

		[Name("talkback")]
		[Command("talkback")]
		[Priority(0)]
		[Summary("Gets the current talkback status and cooldown")]
		[Remarks("Talk/React-Back allows me to respond to messages that contain certain key phrases. I have a cooldown per channel to prevent spam")]
		public async Task GetTalkBack() {
			bool enabled;
			//TimeSpan cooldown;
			var (talk, react) = await talkBack.GetRemainingCooldownsAsync(Context).ConfigureAwait(false);
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
				enabled = guild.TalkBackEnabled;
				//cooldown = guild.TalkBackCooldown;
			}
			await ReplyAsync($"**Talkback:** `{(enabled ? "on" : "off")}`\n" +
							 $"**Talk Cooldown:** {(talk.HasValue ? FormatCooldown(talk.Value) : "*ready*")}\n" +
							 $"**React Cooldown:** {(react.HasValue ? FormatCooldown(react.Value) : "*ready*")}").ConfigureAwait(false);
		}
		private string FormatCooldown(TimeSpan cooldown) {
			StringBuilder str = new StringBuilder();
			// Minimum cooldown of 1 second.
			if (cooldown.TotalSeconds < 1)
				cooldown = TimeSpan.FromSeconds(1);
			if (cooldown.Days > 0)
				str.Append($" {cooldown:%d} day{Plural(cooldown.Days)}");
			if (cooldown.Hours > 0)
				str.Append($" {cooldown:%h} hour{Plural(cooldown.Hours)}");
			if (cooldown.Minutes > 0)
				str.Append($" {cooldown:%m} min{Plural(cooldown.Minutes)}");
			if (cooldown.Seconds > 0)
				str.Append($" {cooldown:%s} sec{Plural(cooldown.Seconds)}");
			return str.ToString().TrimStart();
		}

		private string Plural(int count) {
			return (count != 1 ? "s" : "");
		}
	}
}
