using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using WeCantSpell.Hunspell;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	/// <summary>
	/// A service for checking, executing, and removing mispelled spoiler commands.
	/// </summary>
	public class SpoilerCommandHandlerService : CommandHandlerService {

		#region Fields

		/// <summary>
		/// The dictionary and settings of words that can be matched to the "spoiler" command.
		/// </summary>
		private readonly WordList dictionary;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SpoilerCommandHandlerService"/>.
		/// </summary>
		public SpoilerCommandHandlerService(DiscordBotServiceContainer services) : base(services) {
			//Client.MessageReceived += OnMessageReceived;
			Commands.CommandExecuted += OnCommandExecutedAsync;
			var affix = new AffixConfig.Builder {
				MaxDifferency = 9,
				Options = AffixConfigOptions.OnlyMaxDiff,
			}.ToImmutable();
			string[] words = new[] { "spoil", "spoiler" };//, "spoilers", "spoiled" };
			dictionary = WordList.CreateFromWords(words, affix);
		}

		#endregion
		
		#region Override Methods

		/// <summary>
		/// Performs additional checks to see if the command is allowed to be processed.
		/// </summary>
		/// <param name="context">The context of the command.</param>
		/// <returns>True if the command can be processed.</returns>
		protected override async Task<bool> AllowCommandAsync(IDiscordBotCommandContext context) {
			string text = context.Message.Content.ToLower();
			// Prevent commands when abbreviating Steins;Gate and such
			if (text.Length >= 4 && text[1] == ';' && !char.IsWhiteSpace(text[2]) && char.IsWhiteSpace(text[3]))
				return false;
			Guild guild;
			using (var db = GetDb<SpoilerDbContext>())
				guild = await db.FindGuildAsync(context).ConfigureAwait(false);
			return (!context.User.IsBot || (guild != null && guild.AllowBots));
		}

		#endregion

		#region Event handlers

		/// <summary>
		/// Checks for mispelled spoilers after an unknown command is executed.
		/// </summary>
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

		/// <summary>
		/// Scores the command's likeness to "spoiler" and sees if it's close enough to warrant removing.
		/// </summary>
		/// <param name="text">The text to check.</param>
		/// <returns>True if the command is close enough.</returns>
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
