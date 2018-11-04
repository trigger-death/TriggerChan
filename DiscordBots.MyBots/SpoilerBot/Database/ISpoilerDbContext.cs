using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.SpoilerBot.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Database {
	public interface ISpoilerDbContext {
		/// <summary>
		/// The table of all active spoilers.
		/// </summary>
		DbSet<Spoiler> Spoilers { get; set; }
		/// <summary>
		/// The table of all users that have been spoiled to a specific spoiler.
		/// </summary>
		DbSet<SpoiledUser> SpoiledUsers { get; set; }
		
		Task<IDbSpoilerUser> FindSpoilerUserAsync(ulong userId);

		Task EnsureEndUserData(ulong userId);
		Task EnsureEndUserData(IDbEndUserData dbEud);
	}
}
