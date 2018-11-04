using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class ContextingService : DiscordBotService, IContextingService {

		#region Constructors

		public ContextingService(DiscordBotServiceContainer services) : base(services) { }

		#endregion

		#region Properties

		/// <summary>
		/// Gets the default prefix used by the Discord Bot.
		/// </summary>
		public string DefaultPrefix => Config["prefix"];

		#endregion

		#region Command Contexts

		/// <summary>
		/// Constructs the required type of command context that is used by this Discord bot.
		/// </summary>
		/// <param name="msg">The message to construct the context from.</param>
		/// <returns>The created context.</returns>
		public IDiscordBotCommandContext CreateCommandContext(SocketUserMessage msg) {
			return new DiscordBotCommandContext(Services, Client, msg);
		}

		#endregion

		#region Database Contexts

		/// <summary>
		/// Gets if the current context supports custom prefixes.
		/// </summary>
		/// <param name="context">The command context to check.</param>
		/// <returns>True if custom prefixes are supported.</returns>
		public bool IsDbPrefixContext(ICommandContext context) {
			return IsDbCommandContext(context);
		}
		/// <summary>
		/// Gets if the current context supports lockable commands.
		/// </summary>
		/// <param name="context">The command context to check.</param>
		/// <returns>True if lockable commands are supported.</returns>
		public bool IsDbLockableContext(ICommandContext context) {
			return IsDbCommandContext(context);
		}
		/// <summary>
		/// Gets if the current context supports a manager Id.
		/// </summary>
		/// <param name="context">The command context to check.</param>
		/// <returns>True if a manager Id are supported.</returns>
		public bool IsDbManagerContext(ICommandContext context) {
			return IsDbCommandContext(context);
		}
		/// <summary>
		/// Gets if the current context supports a database command context.
		/// </summary>
		/// <param name="context">The command context to check.</param>
		/// <returns>True if the database command context is supported.</returns>
		public bool IsDbCommandContext(ICommandContext context) {
			return context.Guild != null;// || context.Channel is IDMChannel;
		}
		
		/// <summary>
		/// Gets the database used to locate the command context.
		/// </summary>
		/// <returns>The database context that must be disposed of.</returns>
		public TriggerDbContext GetCommandContextDb() => GetDb<TriggerDbContext>();
		DbContextEx IContextingService.GetCommandContextDb() => GetCommandContextDb();

		/*private Guild CreateGuild(ulong id) {
			return new Guild {
				Id = id,
				SpellCheckSpoilers = true,
				AllowBotSpoilers = false,
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
		/// <param name="db">The database to search in.</param>
		/// <param name="id">The id of the guild to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<Guild> FindGuildAsync(TriggerDbContext db, ulong id, bool add) {
			Guild guild = await db.Guilds.FindAsync(id).ConfigureAwait(false);
			if (guild == null) {
				guild = CreateGuild(id);
				await db.Guilds.AddAsync(guild).ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			return guild;
		}
		/// <summary>
		/// Finds the guild context in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the guild to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<Guild> FindGuildAsync(ulong id, bool add) {
			using (var db = GetDb<TriggerDbContext>())
				return await db.FindGuildAsync(id).ConfigureAwait(false);
		}

		/// <summary>
		/// Finds the user context in the database and optionally adds it.
		/// </summary>
		/// <param name="db">The database to search in.</param>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<User> FindUserAsync(TriggerDbContext db, ulong id, bool add) {
			User user = await db.Users.FindAsync(id).ConfigureAwait(false);
			if (user == null) {
				user = CreateUser(id);
				await db.Users.AddAsync(user).ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			return user;
		}
		/// <summary>
		/// Finds the user context in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<User> FindUserAsync(ulong id, bool add) {
			using (var db = GetDb<TriggerDbContext>())
				return await FindUserAsync(id, add).ConfigureAwait(false);
		}

		/// <summary>
		/// Finds the user profile in the database and optionally adds it.
		/// </summary>
		/// <param name="db">The database to search in.</param>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the profile should be added if it's missing.</param>
		/// <returns>The profile, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<UserProfile> FindUserProfileAsync(TriggerDbContext db, ulong id, bool add) {
			UserProfile userProfile = await db.UserProfiles.FindAsync(id).ConfigureAwait(false);
			if (userProfile == null) {
				userProfile = CreateUserProfile(id);
				await db.UserProfiles.AddAsync(userProfile).ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			return userProfile;
		}
		/// <summary>
		/// Finds the user profile in the database and optionally adds it.
		/// </summary>
		/// <param name="id">The id of the user to look for.</param>
		/// <param name="add">True if the profile should be added if it's missing.</param>
		/// <returns>The profile, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<UserProfile> FindUserProfileAsync(ulong id, bool add) {
			using (var db = GetDb<TriggerDbContext>())
				return await FindUserProfileAsync(id, add).ConfigureAwait(false);
		}*/

		/// <summary>
		/// Finds the prefixable context in the database and optionally adds it.
		/// </summary>
		/// <param name="db">The database to search in.</param>
		/// <param name="context">The command context to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<IDbPrefixContext> FindDbPrefixContextAsync(TriggerDbContext db, ICommandContext context, bool add) {
			if (context.Guild != null)
				return await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			//else if (context.Channel is IDMChannel) {
			//	return FindDMAsync(db, context.Guild.Id, add);
			return null;
		}
		Task<IDbPrefixContext> IContextingService.FindDbPrefixContextAsync(DbContextEx db, ICommandContext context, bool add) {
			return FindDbPrefixContextAsync((TriggerDbContext) db, context, add);
		}
		/// <summary>
		/// Finds the lockable context in the database and optionally adds it.
		/// </summary>
		/// <param name="db">The database to search in.</param>
		/// <param name="context">The command context to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<IDbLockableContext> FindDbLockableContextAsync(TriggerDbContext db, ICommandContext context, bool add) {
			if (context.Guild != null)
				return await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			//else if (context.Channel is IDMChannel) {
			//	return FindDMAsync(db, context.Guild.Id, add);
			return null;
		}
		Task<IDbLockableContext> IContextingService.FindDbLockableContextAsync(DbContextEx db, ICommandContext context, bool add) {
			return FindDbLockableContextAsync((TriggerDbContext) db, context, add);
		}
		/// <summary>
		/// Finds the manager context in the database and optionally adds it.
		/// </summary>
		/// <param name="dbBase">The database to search in.</param>
		/// <param name="context">The command context to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<IDbManagerContext> FindDbManagerContextAsync(TriggerDbContext db, ICommandContext context, bool add) {
			if (context.Guild != null)
				return await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			//else if (context.Channel is IDMChannel) {
			//	return FindDMAsync(db, context.Guild.Id, add);
			return null;
		}
		Task<IDbManagerContext> IContextingService.FindDbManagerContextAsync(DbContextEx db, ICommandContext context, bool add) {
			return FindDbManagerContextAsync((TriggerDbContext) db, context, add);
		}
		/// <summary>
		/// Finds the command context in the database and optionally adds it.
		/// </summary>
		/// <param name="db">The database to search in.</param>
		/// <param name="context">The command context to look for.</param>
		/// <param name="add">True if the context should be added if it's missing.</param>
		/// <returns>The context, or null if it does not exist, and <see cref="add"/> is false.</returns>
		public async Task<IDbCommandContext> FindDbCommandContextAsync(TriggerDbContext db, ICommandContext context, bool add) {
			if (context.Guild != null)
				return await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);
			//else if (context.Channel is IDMChannel) {
			//	return FindDMAsync(db, context.Guild.Id, add);
			return null;
		}
		Task<IDbCommandContext> FindDbCommandContextAsync(DbContextEx db, ICommandContext context, bool add) {
			return FindDbCommandContextAsync((TriggerDbContext) db, context, add);
		}

		#endregion
	}
}
