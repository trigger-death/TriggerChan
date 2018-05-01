using Discord;
using Discord.Commands;
using MALClient.Models.Models;
using MALClient.XShared.Comm.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Information")]
	[IsLockable(true)]
	public class InformationModule : BotModuleBase {


		[Command("mal")]
		[Summary("Gets your MAL profile URL registed with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMyMAL() {
			IUser user = Context.User;
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.MALUsername == null) {
				await ReplyAsync($"You have not registered your MAL account");
			}
			else {
				await WriteMALProfileEmbed(name, gUser.MALUsername);
			}
		}

		public async Task WriteMALProfileEmbed(string username, string malUser) {
			const string profileUrl = @"https://myanimelist.net/profile/";
			//await ReplyAsync($"{name}'s MAL: <{profileUrl}{username}>");
			var query = new ProfileQuery(malUser);
			ProfileData profile = await query.GetProfileData();
			if (profile == null) {
				await ReplyAsync($"Could not find MAL profile for {Format.Sanitize(malUser)}");
				return;
			}
			var embed = new EmbedBuilder() {
				Url = $"{profileUrl}{malUser}",
				ThumbnailUrl = profile.User.ImgUrl,
				Title = $"{malUser}'s MAL Profile",
				Color = new Color(114, 137, 218),
			};
			string desc = "";
			desc += $"Days: {profile.AnimeDays.ToString("#,##0.#")}\n";
			desc += $"Mean Score: {profile.AnimeMean.ToString("0.00")}\n";
			desc += $"Completed: {profile.AnimeCompleted.ToString("#,##0")}\n";
			desc += $"Episodes: {profile.AnimeEpisodes.ToString("#,###,##0")}\n";
			embed.AddField($"Anime", desc, true);

			desc = "";
			desc += $"Days: {profile.MangaDays.ToString("#,##0.#")}\n";
			desc += $"Mean Score: {profile.MangaMean.ToString("0.00")}\n";
			desc += $"Completed: {profile.MangaCompleted.ToString("#,##0")}\n";
			desc += $"Volumes: {profile.MangaVolumes.ToString("#,###,##0")}\n";
			desc += $"Chapters: {profile.MangaChapters.ToString("#,###,##0")}\n";
			embed.AddField($"Manga", desc, true);


			await ReplyAsync("", false, embed.Build());
		}

		[Command("mal")]
		[Summary("Gets the usere's MAL profile URL registed with the bot")]
		[Parameters("<user>")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMAL(IUser user) {
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.MALUsername == null) {
				await ReplyAsync($"{name} has not registered their MAL account");
			}
			else {
				await WriteMALProfileEmbed(name, gUser.MALUsername);
			}
		}

		[Command("mal profile")]
		[Summary("Display the MAL user's profile")]
		[Parameters("<malUser>")]
		public async Task GetMALProfile(string url) {
			url = url.Trim();
			if (url.StartsWith('<') && url.EndsWith('>')) {
				url = url.Substring(1, url.Length - 2);
			}
			RegexOptions options = RegexOptions.IgnoreCase;
			string pattern = @"(^https?\:\/\/myanimelist\.net\/profile\/|^)((\-|\w)+)$";
			Regex regex = new Regex(pattern, options);
			if (!regex.IsMatch(url)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid url or profile name";
				return;
			}
			string username = url.Split('/').Last();
			await WriteMALProfileEmbed(username, username);
		}

		[Command("mal assign")]
		[Summary("Registers your MAL profile with the bot")]
		[Parameters("<profile url/username>")]
		[RequireContext(ContextType.Guild)]
		public async Task AssignMAL(string url) {
			url = url.Trim();
			if (url.StartsWith('<') && url.EndsWith('>')) {
				url = url.Substring(1, url.Length - 2);
			}
			RegexOptions options = RegexOptions.IgnoreCase;
			string pattern = @"(^https?\:\/\/myanimelist\.net\/profile\/|^)((\-|\w)+)$";
			Regex regex = new Regex(pattern, options);
			if (!regex.IsMatch(url)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid url or profile name";
				return;
			}
			string username = url.Split('/').Last();
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(Context);
				gUser.MALUsername = username;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("mal unassign")]
		[Summary("Unregisters your MAL profile with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task UnassignMAL() {
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(Context);
				if (gUser.MALUsername == null) {
					await ReplyAsync("You do not have a registered MAL to unassign");
					return;
				}
				gUser.MALUsername = null;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("mal usercount")]
		[Summary("Gets the number of users that have registered MAL profiles")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMALUserCount() {
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				int count = 0;
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MALUsername != null)
						count++;
				}
				await ReplyAsync($"**Registered MAL Users:** {count}");
			}
		}

		[Command("mal users")]
		[Summary("Lists all MAL users registered with the bot in a direct message")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task GetMALUsers() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Title = $"Users with registered MAL profiles on {Context.Guild.Name}",
			};
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				string description = "";
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MALUsername == null)
						continue;
					IGuildUser user = Context.Guild.GetUser(gUser.UserId);
					if (user != null)
						description += $"{user.GetName(Context.Guild)}\n";
				}
				IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
				if (string.IsNullOrEmpty(description)) {
					embed.Description = "No users have registered their MAL profile";
				}
				else {
					embed.Description = description;
				}
				await dm.SendMessageAsync("", false, embed.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
		}

		[Command("mal users url")]
		[Summary("Lists all MAL users registered with the bot and their profile URLs in a direct message")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task GetMALUserUrl() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Title = $"Users with registered MAL profiles on {Context.Guild.Name}",
			};
			const string profileUrl = @"https://myanimelist.net/profile/";
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				string description = "";
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MALUsername == null)
						continue;
					IGuildUser user = Context.Guild.GetUser(gUser.UserId);
					if (user != null)
						description += $"{user.GetName(Context.Guild)} **|** {profileUrl}{gUser.MALUsername}\n";
				}
				IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
				if (string.IsNullOrEmpty(description)) {
					embed.Description = "No users have registered their MAL profile";
				}
				else {
					embed.Description = description;
				}
				await dm.SendMessageAsync("", false, embed.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
		}
	}
}
