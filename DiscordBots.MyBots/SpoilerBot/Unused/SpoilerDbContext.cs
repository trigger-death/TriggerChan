using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using Discord.Commands;

namespace TriggersTools.DiscordBots.SpoilerBot.Database {
	/// <summary>
	/// The database context for spoilers.
	/// </summary>
	public abstract class SpoilerDbContext : DbContextEx {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SpoilerDbContext"/>.
		/// </summary>
		/// <param name="dbProvider">The bot database provider service.</param>
		public SpoilerDbContext(IDbProvider dbProvider) : base(dbProvider) { }

		#endregion

		#region Tables

		/// <summary>
		/// The table of all active spoilers.
		/// </summary>
		public DbSet<Spoiler> Spoilers { get; set; }
		/// <summary>
		/// The table of all users that have been spoiled to a specific spoiler.
		/// </summary>
		public DbSet<SpoiledUser> SpoiledUsers { get; set; }
		/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		//public DbSet<SpoilerUser> SpoilerUsers { get; set; }
		/*/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		public DbSet<Guild> Guilds { get; set; }*/
		/*/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		public DbSet<DM> DMs { get; set; }
		/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		public DbSet<UserProfile> UserProfiles { get; set; }*/


		protected internal abstract Task<IDbSpoilerUser> FindSpoilerUserAsync(ulong userId);

		public abstract Task EnsureEndUserData(ulong userId);
		public Task EnsureEndUserData(IDbEndUserData dbEud) {
			return EnsureEndUserData(dbEud.EndUserDataId);
		}

		/*public async Task<User> LoadEndUserData(ulong userId) {
			User eud = await Users.FindAsync(userId).ConfigureAwait(false);
			if (eud == null)
				return null;
			await LoadAllCollectionsAsync(eud).ConfigureAwait(false);
			return eud;
		}


		public Task<bool> RemoveEndUserData(ulong userId) {
			return RemoveEndUserDataBase(Users, userId);
		}*/

		#endregion

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			//modelBuilder.Entity<SpoiledUser>()
			//	.HasKey(su => new { su.MessageId, su.EndUserDataId });
			base.OnModelCreating(modelBuilder);
		}

		/*public async Task<Guild> GetOrAddGuild(ICommandContext context) {
			if (context.Guild != null) {
				Guild guild = await Guilds.FindAsync(context.Guild.Id).ConfigureAwait(false);
				if (guild == null) {
					await Guilds.AddAsync(guild = new Guild { Id = context.Guild.Id }).ConfigureAwait(false);
					await SaveChangesAsync().ConfigureAwait(false);
				}
				return guild;
			}
			return null;
		}
		public async Task<Guild> FindGuildAsync(ICommandContext context) {
			if (context.Guild != null) {
				return await Guilds.FindAsync(context.Guild.Id).ConfigureAwait(false);
			}
			return null;
		}*/
	}
}
