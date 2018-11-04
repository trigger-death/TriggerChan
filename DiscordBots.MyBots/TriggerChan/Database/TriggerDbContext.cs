using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.TriggerChan.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Database {
	/// <summary>
	/// The database context for trigger_chan.
	/// </summary>
	public class TriggerDbContext : DbContextEx, ISpoilerDbContext {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="TriggerDbContext"/>.
		/// </summary>
		/// <param name="dbProvider">The bot database provider service.</param>
		public TriggerDbContext(IDbProvider dbProvider) : base(dbProvider) { }

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
		public DbSet<User> Users { get; set; }
		/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		public DbSet<Guild> Guilds { get; set; }
		/*/// <summary>
		/// The table that lists all end user data.
		/// </summary>
		public DbSet<DM> DMs { get; set; }*/
		/// <summary>
		/// The table that lists all customizable user profiles.
		/// </summary>
		public DbSet<UserProfile> UserProfiles { get; set; }
		/// <summary>
		/// The table that lists all cached guild emotes
		/// </summary>
		public DbSet<GuildEmote> GuildEmotes { get; set; }

		async Task<IDbSpoilerUser> ISpoilerDbContext.FindSpoilerUserAsync(ulong userId) {
			return await Users.FindAsync(userId).ConfigureAwait(false);
		}

		public async Task EnsureEndUserData(ulong userId) {
			var eud = await Users.FindAsync(userId).ConfigureAwait(false);
			if (eud == null)
				await Users.AddAsync(new User { EndUserDataId = userId }).ConfigureAwait(false);
		}
		public Task EnsureEndUserData(IDbEndUserData dbEud) {
			return EnsureEndUserData(dbEud.EndUserDataId);
		}

		public async Task<User> LoadEndUserData(ulong userId) {
			User eud = await Users.FindAsync(userId).ConfigureAwait(false);
			if (eud == null)
				return null;
			await LoadAllCollectionsAsync(eud).ConfigureAwait(false);
			return eud;
		}


		public Task<bool> RemoveEndUserData(ulong userId) {
			return RemoveEndUserDataBase(Users, userId);
		}

		public async Task<Guild> GetOrAddGuild(ICommandContext context) {
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
		}
		#endregion


		private Guild CreateGuild(ulong id) {
			return new Guild {
				Id = id,
				SpellCheckSpoilers = true,
				AllowBotSpoilers = false,
				TalkBackEnabled = true,
				TalkBackCooldown = TimeSpan.FromMinutes(10),
				PinReactCount = 10,
			};
		}
		private User CreateUser(ulong id) {
			return new User {
				Id = id,
			};
		}
		private UserProfile CreateUserProfile(ulong id) {
			return new UserProfile {
				Id = id,
			};
		}

		/// <summary>
		/// Finds the guild context in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the guild to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<Guild> FindGuildAsync(ulong id) {
			Guild guild = await Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null /*&& add*/) {
				guild = CreateGuild(id);
				await Guilds.AddAsync(guild).ConfigureAwait(false);
				await SaveChangesAsync().ConfigureAwait(false);
			}
			return guild;
		}
		/// <summary>
		/// Finds the user context in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<User> FindUserAsync(ulong id) {
			User user = await Users.FindAsync(id).ConfigureAwait(false);
			if (user == null /*&& add*/) {
				user = CreateUser(id);
				await Users.AddAsync(user).ConfigureAwait(false);
				await SaveChangesAsync().ConfigureAwait(false);
			}
			return user;
		}
		/// <summary>
		/// Finds the user profile in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the profile should be added if it's missing.</param>
		/// <returns>The profile, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<UserProfile> FindUserProfileAsync(ulong id) {
			await EnsureEndUserData(id).ConfigureAwait(false);
			UserProfile userProfile = await UserProfiles.FindAsync(id).ConfigureAwait(false);
			if (userProfile == null /*&& add*/) {
				userProfile = CreateUserProfile(id);
				await UserProfiles.AddAsync(userProfile).ConfigureAwait(false);
				await SaveChangesAsync().ConfigureAwait(false);
			}
			return userProfile;
		}
	}
}
