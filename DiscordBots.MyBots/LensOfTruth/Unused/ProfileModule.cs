using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Attributes.Preconditions;
using TriggersTools.DiscordBots.Context;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.SpoilerBot.Database;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Profile")]
	public class ProfileModule : DiscordModule {
		
		public ProfileModule(DiscordServiceProvider services) : base(services) { }

		[Command("eraseme")]
		[Summary("Erases all information on you from the database.")]
		[AllowBots(false)]
		public async Task EraseMe() {
			ulong id = Context.User.Id;
			using (var db = Services.GetDb<SpoilerDbContext>()) {
				bool anyDeleted = await db.RemoveEndUserData(id).ConfigureAwait(false);
				//bool anyDeleted = await DiscordBot.DeleteEndUserData(id, EndUserDataContents.All, EndUserDataType.User).ConfigureAwait(false);
				if (anyDeleted)
					await ReplyAsync($"**Erase Me:** All information on you was removed from the database").ConfigureAwait(false);
				else
					await ReplyAsync($"**Erase Me:** No information on you was found in the database").ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		

		/*public IEnumerable<string> EraseableInformation {
			get {
				yield return 
			}
		}*/

	}
}
