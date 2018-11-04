using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using WeCantSpell.Hunspell;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	public class SpoilerSpellCheckService : DiscordBotService {

		#region Fields

		private readonly WordList dictionary;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SpoilerSpellCheckService"/>.
		/// </summary>
		public SpoilerSpellCheckService(DiscordBotServiceContainer services) : base(services) {
			Commands.CommandExecuted += OnCommandExecutedAsync;
			var affix = new AffixConfig.Builder {
				MaxDifferency = 9,
				Options = AffixConfigOptions.OnlyMaxDiff,
			}.ToImmutable();
			string[] words = new[] { "spoil", "spoiler" };//, "spoilers", "spoiled" };
			dictionary = WordList.CreateFromWords(words, affix);
		}

		#endregion

		#region Event handlers

		private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext contextBase, IResult result) {
			if (result.IsSuccess || !result.Error.HasValue || result.Error != CommandError.UnknownCommand) return;
			if (!(contextBase is IDiscordBotCommandContext context)) return;

			IUserMessage message = context.Message;
			int argPos = context.ArgPos;

			Guild guild;
			using (var db = GetDb<SpoilerDbContext>())
				guild = await db.FindGuildAsync(context).ConfigureAwait(false);

			if (result.Error.HasValue && result.Error.Value == CommandError.UnknownCommand && context.Guild != null) {
				if ((guild == null || guild.SpellCheck) && IsCloseToSpoiler(message.Content.Substring(argPos))) {
					await message.DeleteAsync().ConfigureAwait(false);
					var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
					await dm.SendMessageAsync("**Mispelled Spoiler:** The command was not spelled correctly and was guessed to be `spoiler`").ConfigureAwait(false);
					await dm.SendMessageAsync(context.Message.Content).ConfigureAwait(false);
				}
			}
		}

		#endregion

		#region SpoilerScore

		private bool IsCloseToSpoiler(string text) {
			text = text.ToLower();
			int maxIndex = Math.Min(text.Length, 15);
			for (int i = 3; i <= maxIndex; i++) {
				string word = text.Substring(0, i);
				var suggestions = dictionary.Suggest(word);
				if (suggestions.Any())
					return true;
			}
			return false;
		}

		#endregion
	}
}
