using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Context;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.TriggerChan.Database;

namespace TriggersTools.DiscordBots.TriggerChan.Commands {
	public class BotBanWaitContext : ConfirmationUserWaitContext {

		public EndUserDataType EndUserDataType { get; }
		public ulong Id { get; }

		/// <summary>
		/// Constructs the <see cref="BotBanWaitContext"/>.
		/// </summary>
		/// <param name="context">The underlying command context.</param>
		public BotBanWaitContext(DiscordBotCommandContext context, EndUserDataType type, ulong id)
			: base(context, GetName(type), TimeSpan.FromMinutes(2), ConfirmationType.Digit4)
		{
			EndUserDataType = type;
			Id = id;
			Started += OnStarted;
		}

		private Task OnStarted(SocketUserWaitContext arg) {
			Finished += OnFinished;
			return Task.FromResult<object>(null);
		}

		private async Task OnFinished(SocketUserWaitContext arg) {
			using (var db = Services.GetDb<TriggerDbContext>()) {
				bool anyDeleted = await Services.DiscordBot.DeleteEndUserDataAsync(db, Id, EndUserDataContents.All, EndUserDataType).ConfigureAwait(false);
				if (anyDeleted)
					await OutputChannel.SendMessageAsync($"**{Name}:** {EndUserDataType} with Id of {Id} was removed from the database").ConfigureAwait(false);
				else
					await OutputChannel.SendMessageAsync($"**{Name}:** {EndUserDataType} with Id of {Id} had no information in the database").ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		private static string GetName(EndUserDataType type) {
			switch (type) {
			case EndUserDataType.User: return "Bot Ban User";
			case EndUserDataType.Guild: return "Bot Ban Guild";
			}
			throw new ArgumentException(nameof(type));
		}
	}
}
