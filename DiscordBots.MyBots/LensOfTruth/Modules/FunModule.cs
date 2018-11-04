using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Contexts;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.Commands;
using System.Text;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Fun")]
	[Summary("Fun extra commands")]
	[RequiresContext(ContextType.Guild)]
	[IsLockable(true)]
	public class FunModule : DiscordBotModule {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="FunModule"/>.
		/// </summary>
		/// <param name="services"></param>
		public FunModule(DiscordBotServiceContainer services) : base(services) { }

		#endregion

		[Group("ocarina")]
		[Usage("[play|notes|aliases] [notes...]")]
		[Summary("Create Ocarina of Time style music sheets")]
		public class OcarinaGroup : DiscordBotModule {

			#region Fields

			private readonly OcarinaService ocarina;
			private readonly HelpService help;

			#endregion

			#region Constructors

			public OcarinaGroup(DiscordBotServiceContainer services,
								OcarinaService ocarina,
								HelpService help)
				: base(services)
			{
				this.ocarina = ocarina;
				this.help = help;
			}

			#endregion

			[Name("ocarina play <notes>")]
			[Command("play")]
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
					await ReplyAsync($"Coudl not parse note: `{ex.Message}`").ConfigureAwait(false);
				}
			}

			[Name("ocarina notes")]
			[Command("notes"), Alias("aliases")]
			[Priority(0)]
			[Example("List the aliases for all ocarina notes")]
			public async Task Notes() {
				const string Separator = " **:** ";
				EmbedBuilder embed = new EmbedBuilder {
					Title = "**Ocarina Notes**",
					Description = "The following words and emoji can be used to play a note",
					Color = ColorUtils.Parse(Config["embed_color"]),
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
	}
}
