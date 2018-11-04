using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Context;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Commands {
	public class TimeZoneAbbreviationWaitContext : DiscordBotUserWaitContext {

		#region Fields

		private readonly ConfigParserService configParser;

		public TimeZoneAbbreviationMatches Matches { get; }
		public IUserMessage StartMessage { get; private set; }

		#endregion
		public int Count => Matches.Count;
		public string Abbreviation => Matches.Abbreviation;
		public ulong Id { get; }
		public bool Me { get; }

		/// <summary>
		/// Constructs the <see cref="TimeZoneAbbreviationWaitContext"/>.
		/// </summary>
		/// <param name="context">The underlying command context.</param>
		public TimeZoneAbbreviationWaitContext(DiscordBotCommandContext context,
			TimeZoneAbbreviationMatches matches, ConfigParserService configParser)
			: base(context, GetName(matches), TimeSpan.FromMinutes(1.5))
		{
			Matches = matches;
			this.configParser = configParser;
			OutputChannel = context.User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();
			HookEvents();
		}


		public void HookEvents() {
			Ended += OnEndedAsync;
			Started += OnStartedAsync;
			Expired += OnExpiredAsync;
			Canceled += OnCanceledAsync;
			MessageReceived += OnMessageReceivedAsync;
		}

		private async Task OnEndedAsync(SocketUserWaitContext arg) {
			await StartMessage.DeleteAsync().ConfigureAwait(false);
		}
		private async Task OnStartedAsync(SocketUserWaitContext arg) {
			DateTime now = DateTime.UtcNow;
			EmbedBuilder embed = new EmbedBuilder() {
				Title = $"Abbreviation \"{Abbreviation}\" matches multiple timezones!",//\nPlease enter the number of the correct timezone in this channel",
				Color = configParser.EmbedColor,
			};
			StringBuilder str = new StringBuilder();

			int index = 1;
			foreach (var ianaTimeZone in Matches.TimeZones) {
				DateTime time = TimeZoneInfo.ConvertTimeFromUtc(now, ianaTimeZone.TimeZone);
				str.Append($"`[{index}]` {ianaTimeZone.Iana} `[{time.TimeOfDay:hh\\:mm\\:ss}]`\n");
				index++;
			}
			embed.WithDescription(str.ToString());
			var dm = await User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
			await dm.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
			StartMessage = await OutputChannel.SendMessageAsync($"**{Name}:** Input the number of the correct timezone to finish assigning").ConfigureAwait(false);
		}
		private async Task OnExpiredAsync(SocketUserWaitContext arg) {
			await OutputChannel.SendMessageAsync($"**{Name}:** Timed out!").ConfigureAwait(false);
		}
		private async Task OnCanceledAsync(SocketUserWaitContext arg) {
			await OutputChannel.SendMessageAsync($"**{Name}:** Was canceled!").ConfigureAwait(false);
		}
		private async Task OnMessageReceivedAsync(SocketUserWaitContext context, IUserMessage msg) {
			if (OutputChannel.Id != msg.Channel.Id)
				return;
			string input = msg.Content.Trim();
			if (input == "cancel") {
				await CancelAsync().ConfigureAwait(false);
			}
			else if (int.TryParse(input, out int index) && index >= 1 && index <= Count) {
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(User.Id).ConfigureAwait(false);
					userProfile.TimeZone = Matches.TimeZones[index - 1].TimeZone;
					db.ModifyOnly(userProfile, up => up.TimeZone);
					await db.SaveChangesAsync().ConfigureAwait(false);
					await msg.AddReactionAsync(TriggerReactions.Success).ConfigureAwait(false);
					await FinishAsync().ConfigureAwait(false);
				}
			}
			else {
				await OutputChannel.SendMessageAsync($"**Invalid Input:** Input must be integer between {1} and {Count}").ConfigureAwait(false);
			}
		}

		private static string GetName(TimeZoneAbbreviationMatches matches) {
			return $"Time Zone \"{matches.Abbreviation}\" Ambiguities";
		}
	}
}

