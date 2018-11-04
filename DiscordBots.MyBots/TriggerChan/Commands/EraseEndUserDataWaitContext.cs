using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Context;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.TriggerChan.Database;

namespace TriggersTools.DiscordBots.TriggerChan.Commands {
	public class EraseEndUserDataWaitContext : ConfirmationUserWaitContext {

		public EndUserDataType EndUserDataType { get; }
		public ulong Id { get; }
		public bool Me { get; }

		/// <summary>
		/// Constructs the <see cref="EraseEndUserDataWaitContext"/>.
		/// </summary>
		/// <param name="context">The underlying command context.</param>
		public EraseEndUserDataWaitContext(DiscordBotCommandContext context, EndUserDataType type, ulong id, bool me = false)
			: base(context, GetName(type, me), TimeSpan.FromMinutes(2), ConfirmationType.Digit4)
		{
			EndUserDataType = type;
			Id = id;
			Me = me;
			Started += OnStarted;
		}

		private Task OnStarted(SocketUserWaitContext arg) {
			Finished += OnFinished;
			return Task.FromResult<object>(null);
		}

		private async Task OnFinished(SocketUserWaitContext arg) {
			using (var db = Services.GetDb<TriggerDbContext>()) {
				bool anyDeleted = await Services.DiscordBot.DeleteEndUserDataAsync(db, Id, EndUserDataContents.All, EndUserDataType).ConfigureAwait(false);
				if (Me) {
					if (anyDeleted)
						await OutputChannel.SendMessageAsync($"**{Name}:** All information on you was removed from the database").ConfigureAwait(false);
					else
						await OutputChannel.SendMessageAsync($"**{Name}:** No information on you was found in the database").ConfigureAwait(false);
				}
				else {
					if (anyDeleted)
						await OutputChannel.SendMessageAsync($"**{Name}:** {EndUserDataType} with Id of {Id} was removed from the database").ConfigureAwait(false);
					else
						await OutputChannel.SendMessageAsync($"**{Name}:** {EndUserDataType} with Id of {Id} had no information in the database").ConfigureAwait(false);
				}
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		private static string GetName(EndUserDataType type, bool me) {
			if (me)
				return "Erase Me";
			switch (type) {
			case EndUserDataType.User: return "Erase User";
			case EndUserDataType.Guild: return "Erase Guild";
			}
			throw new ArgumentException(nameof(type));
		}
	}
}
