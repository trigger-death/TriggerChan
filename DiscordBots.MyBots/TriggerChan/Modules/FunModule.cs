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

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Fun")]
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
		[Name("vaporwave <text>")]
		[Command("vaporwave"), Alias("vapor", "aesthetic")]
		[Usage("<text...>")]
		[Summary("C r e a t e   a e s t h e t i c l y   p l e a s i n g   t e x t")]
		[Example("Running in the 90's", "Outputs: *R u n n i n g   i n   t h e   9 0 ' s*")]
		public Task Vaporwave([Remainder] string text) {
			char[] letters = text.ToCharArray();
			text = string.Join(" ", letters);
			return ReplyAsync(text);
		}

		[Name("javascript")]
		[Command("javascript"), Alias("js")]
		[Summary("An image macro from Dagashi Kashi about Javascript")]
		public Task JavaScript() {
			return Context.Channel.SendFileAsync(BotResources.JavaScript);
		}

		[Name("merge")]
		[Command("merge", RunMode = RunMode.Async), Alias("mergeconflict")]
		[Summary("An image macro from New Game about Merge Conflicts")]
		public Task MergeConflict() {
			return Context.Channel.SendFileAsync(BotResources.Merge_Conflict);
		}

		[Name("culture")]
		[Command("culture", RunMode = RunMode.Async), Alias("manofculture")]
		[Summary("Ah, I see you're a man of culture as well")]
		public Task ManOfCulture() {
			return Context.Channel.SendFileAsync(BotResources.Man_of_Culture);
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

			[Name("ocarina play <notes>")]
			[Command("play"), Alias("")]
			[Priority(0)]
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
					Title = $"{configParser.EmbedPrefix}**Ocarina Notes**",
					Description = "The following words and emoji can be used to play a note",
					Color = configParser.EmbedColor,
				};
				string Ocarina = $"<:Ocarina:504062810121306122>";
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

		[Name("asciifyaa")]
		[Command("asciifyaa")]
		[Usage("[smoothness<1-3> [<scale%> [nodelete]]]")]
		[Summary("Asciify an uploaded image. Image must be a png, jpg, or bmp.\nScaled image dimensions must not be larger than 1000x1000")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" + 
				 "Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[Example("3 200", "Will create a smoth asciification at 2x the image resolution")]
		[Example("1 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public async Task<RuntimeResult> AsciifyImageAsciiArtist(/*bool sharp = true,*/ int smoothness = 1, float scale = 100.0f, string nodelete = null) {
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
		}

		[Name("asciify")]
		[Command("asciify")]
		[Usage("[smooth<yes|no> [<scale%> [nodelete]]]")]
		[Summary("Asciify an uploaded image. Image must be a png, jpg, or bmp.\nScaled image dimensions must not be larger than 1000x1000")]
		[Remarks("The nodelete parameter will keep the message with your attachment if specified.\n" +
				 "Smoothness can be between 1 and 4. Smoothness sacrifices saturation for shape accuracy.")]
		[Example("yes 200", "Will create a smooth asciification at 2x the image resolution")]
		[Example("no 50 nodelete", "Will create a closer color representation of the image with rougher features at 0.5x scale. The posted image will not be deleted.")]
		[RequiresContext(ContextType.Guild)]
		public async Task<RuntimeResult> AsciifyImageAsciiArtist(string smoothYesNo = "no", float scale = 100.0f, string nodelete = null) {
			smoothYesNo = smoothYesNo.ToLower();
			if (nodelete != null && string.Compare(nodelete, "nodelete") != 0) {
				return EmoteResults.FromInvalidArgument("Invalid nodelete");
			}
			else if (smoothYesNo != "yes" && smoothYesNo != "no") {
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
				Smooth = smoothYesNo == "yes",
				//Smoothness = smoothness,
				Scale = scale / 100.0f,
				Delete = nodelete == null,
			};
			if (attach == null) {
				await ReplyAsync($"You must upload an image attachment to asciify").ConfigureAwait(false);
			}
			else {
				await asciify.Asciify(Context, asciifyTask, true).ConfigureAwait(false);
			}
			return NormalResult.FromSuccess();
		}

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
