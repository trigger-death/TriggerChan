using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.TriggerChan.Profiles;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public abstract class DatabaseProfileModule : TriggerModule {

		public DatabaseType Database { get; }
		public string Command { get; }
		public IDatabaseUserProfileReader Reader { get; }
		public string DatabaseName => Database.ToInfo().DefaultName;

		protected DatabaseProfileModule(TriggerServiceContainer services, DatabaseType database, string command)
			: base(services)
		{
			Database = database;
			Command = command;
			Reader = Services.GetService<DatabaseProfileService>().GetReader(database);
		}

		protected string GetUsername(UserProfile userProfile) {
			switch (Database) {
			case DatabaseType.MyAnimeList:
				return userProfile.MALUsername;
			case DatabaseType.AniList:
				return userProfile.AniListUsername;
			case DatabaseType.Kitsu:
				return userProfile.KitsuUsername;
			case DatabaseType.MyFigureCollection:
				return userProfile.MFCUsername;
			case DatabaseType.VNDb:
				return userProfile.VNdbUsername;
			default:
				throw new ArgumentException(nameof(Database));
			}
		}

		protected string SetUsername(UserProfile userProfile, string username) {
			switch (Database) {
			case DatabaseType.MyAnimeList:
				return userProfile.MALUsername = username;
			case DatabaseType.AniList:
				return userProfile.AniListUsername = username;
			case DatabaseType.Kitsu:
				return userProfile.KitsuUsername = username;
			case DatabaseType.MyFigureCollection:
				return userProfile.MFCUsername = username;
			case DatabaseType.VNDb:
				return userProfile.VNdbUsername = username;
			default:
				throw new ArgumentException(nameof(Database));
			}
		}
		private Expression<Func<UserProfile, string>> GetExpression() {
			switch (Database) {
			case DatabaseType.MyAnimeList:
				return up => up.MALUsername;
			case DatabaseType.AniList:
				return up => up.AniListUsername;
			case DatabaseType.Kitsu:
				return up => up.KitsuUsername;
			case DatabaseType.MyFigureCollection:
				return up => up.MFCUsername;
			case DatabaseType.VNDb:
				return up => up.VNdbUsername;
			default:
				throw new ArgumentException(nameof(Database));
			}
		}

		protected async Task MyProfileAsync() {
			IUser user = Context.User;
			UserProfile userProfile;
			using (var db = GetDb<TriggerDbContext>())
				userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
			string username = GetUsername(userProfile);
			if (username == null) {
				await ReplyAsync($"You have not registered your {DatabaseName} username.\n" +
					$"Use the command `{Command} assign <{Command}User>` to register your username.").ConfigureAwait(false);
				return;
			}
			await ReplyEmbedAsync(username, user).ConfigureAwait(false);
		}
		protected async Task UserProfileAsync(IUser user) {
			UserProfile userProfile;
			using (var db = GetDb<TriggerDbContext>())
				userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
			string username = GetUsername(userProfile);
			if (username == null) {
				string name = user.GetName(Context.Guild, true);
				await ReplyAsync($"{name} has not registered their {DatabaseName} username.").ConfigureAwait(false);
				return;
			}
			await ReplyEmbedAsync(username, user).ConfigureAwait(false);
		}
		protected async Task<RuntimeResult> LookupProfileAsync(string input) {
			string username = Reader.ParseUsername(input);
			if (username == null) {
				return EmoteResults.FromInvalidArgument();
			}
			await ReplyEmbedAsync(username, null).ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}
		protected async Task<RuntimeResult> AssignAsync(string input) {
			string username = Reader.ParseUsername(input);
			if (username == null) {
				return EmoteResults.FromInvalidArgument();
			}
			using (var db = GetDb<TriggerDbContext>()) {
				UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
				SetUsername(userProfile, username);
				db.ModifyOnly(userProfile, GetExpression());
				await db.SaveChangesAsync().ConfigureAwait(false);
				return EmoteResults.FromSuccess();
			}
		}
		protected async Task<RuntimeResult> UnassignAsync() {
			using (var db = GetDb<TriggerDbContext>()) {
				UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
				string username = GetUsername(userProfile);
				if (username == null) {
					await ReplyAsync($"You do not have a registered {DatabaseName} username to unassign").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
				SetUsername(userProfile, null);
				db.ModifyOnly(userProfile, GetExpression());
				await db.SaveChangesAsync().ConfigureAwait(false);
				return EmoteResults.FromSuccess();
			}
		}

		protected async Task ReplyEmbedAsync(string username, IUser user) {
			try {
				using (Context.Channel.EnterTypingState()) {
					IDatabaseUserProfile profile = await Reader.QueryProfileAsync(username).ConfigureAwait(false);
					DatabaseInfo info = Database.ToInfo();
					var embed = new EmbedBuilder {
						Color = info.Color,
						Title = $"{profile.Username}'s {info.DefaultName} Profile",
						Description = profile.Description,
						Url = profile.Url,
						ThumbnailUrl = profile.AvatarUrl,
					};
					embed.WithFooter(info.FullName, info.ImageUrl);
					foreach (IDatabaseUserList list in profile.Lists) {
						var fields = list.GetFields(InfoLevel.Compact);
						string value = string.Join("\n", fields);
						embed.AddField(list.Title, value, true);
					}
					await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
				}
			} catch (HttpStatusException ex) {
				if (ex.StatusCode == HttpStatusCode.NotFound) {
					await ReplyAsync($"Could not find the {DatabaseName} profile for **{username}**").ConfigureAwait(false);
				}
				else {
					await ReplyAsync($"An error occurred while trying to get the {DatabaseName} profile for **{username}**").ConfigureAwait(false);
				}
			}
		}
	}
}
