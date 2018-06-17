using Discord;
using Discord.Commands;
using MALClient.Models.Models;
using MALClient.XShared.Comm.Profile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.MFC;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;
using TriggersTools.SteinsGate;

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
				await ReplyAsync($"You have not registered your MAL account.\n" +
					$"Register with the command `t/mal assign YourMALUsername`.");
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
		[Summary("Gets the user's MAL profile URL registed with the bot")]
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

		[Command("mfc")]
		[Summary("Gets your MFC (MyFigureCollection) profile URL registed with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMyMFC() {
			IUser user = Context.User;
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.MFCUsername == null) {
				await ReplyAsync($"You have not registered your MFC account.\n" +
					$"Register with the command `t/mfc assign YourMFCUsername`.");
			}
			else {
				await WriteMFCProfileEmbed(name, gUser.MFCUsername);
			}
		}

		public async Task WriteMFCProfileEmbed(string username, string mfcUser) {
			const string profileUrl = @"https://myfigurecollection.net/profile/";
			var query = new ProfileQuery(mfcUser);
			MFCProfile profile;
			try {
				profile = await MFCProfile.LoadProfile(mfcUser);
			}
			catch (Exception ex) {
				await ReplyAsync($"Could not find MFC profile for {Format.Sanitize(mfcUser)}");
				return;
			}
			var embed = new EmbedBuilder() {
				Url = $"{profileUrl}{mfcUser}",
				ThumbnailUrl = profile.AvatarUrl,
				Title = $"{mfcUser}'s MFC Profile",
				Color = new Color(114, 137, 218),
			};
			if (!string.IsNullOrWhiteSpace(profile.Status)) {
				embed.WithDescription(profile.Status);
			}

			//if (profile.Any) {
				foreach (var cat in profile.Categories) {
					embed.AddField(cat.Type.ToString(), cat.ToString(), true);
				}
			/*}
			else {
				embed.
			}*/

			/*string desc = "";
			desc += $"Days: {profile.AnimeDays.ToString("#,##0.#")}\n";
			desc += $"Mean Score: {profile.AnimeMean.ToString("0.00")}\n";
			desc += $"Completed: {profile.AnimeCompleted.ToString("#,##0")}\n";
			desc += $"Episodes: {profile.AnimeEpisodes.ToString("#,###,##0")}\n";
			embed.AddField($"Figures", desc, true);

			desc = "";
			desc += $"Days: {profile.MangaDays.ToString("#,##0.#")}\n";
			desc += $"Mean Score: {profile.MangaMean.ToString("0.00")}\n";
			desc += $"Completed: {profile.MangaCompleted.ToString("#,##0")}\n";
			desc += $"Volumes: {profile.MangaVolumes.ToString("#,###,##0")}\n";
			desc += $"Chapters: {profile.MangaChapters.ToString("#,###,##0")}\n";
			embed.AddField($"Manga", desc, true);*/


			await ReplyAsync("", false, embed.Build());
		}

		[Command("mfc")]
		[Summary("Gets the user's MFC (MyFigureCollection) profile URL registed with the bot")]
		[Parameters("<user>")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMFC(IUser user) {
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.MFCUsername == null) {
				await ReplyAsync($"{name} has not registered their MFC account");
			}
			else {
				await WriteMFCProfileEmbed(name, gUser.MFCUsername);
			}
		}

		[Command("mfc profile")]
		[Summary("Display the MFC (MyFigureCollection) user's profile")]
		[Parameters("<mfcUser>")]
		public async Task GetMFCProfile(string url) {
			url = url.Trim();
			if (url.StartsWith('<') && url.EndsWith('>')) {
				url = url.Substring(1, url.Length - 2);
			}
			RegexOptions options = RegexOptions.IgnoreCase;
			string pattern = @"(^https?\:\/\/myfigurecollection\.net\/profile\/|^)((\-|\w)+)$";
			Regex regex = new Regex(pattern, options);
			if (!regex.IsMatch(url)) {
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.InvalidArgument;
				Context.ErrorReason = "Invalid url or profile name";
				return;
			}
			string username = url.Split('/').Last();
			await WriteMFCProfileEmbed(username, username);
		}

		[Command("mfc assign")]
		[Summary("Registers your MFC (MyFigureCollection) profile with the bot")]
		[Parameters("<profile url/username>")]
		[RequireContext(ContextType.Guild)]
		public async Task AssignMFC(string url) {
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
				gUser.MFCUsername = username;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("mfc unassign")]
		[Summary("Unregisters your MFC (MyFigureCollection) profile with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task UnassignMFC() {
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(Context);
				if (gUser.MFCUsername == null) {
					await ReplyAsync("You do not have a registered MFC to unassign");
					return;
				}
				gUser.MFCUsername = null;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("mfc usercount")]
		[Summary("Gets the number of users that have registered MFC (MyFigireCollection) profiles")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMFCUserCount() {
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				int count = 0;
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MFCUsername != null)
						count++;
				}
				await ReplyAsync($"**Registered MFC Users:** {count}");
			}
		}

		[Command("mfc users")]
		[Summary("Lists all MFC users registered with the bot in a direct message")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task GetMFCUsers() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Title = $"Users with registered MFC profiles on {Context.Guild.Name}",
			};
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				string description = "";
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MFCUsername == null)
						continue;
					IGuildUser user = Context.Guild.GetUser(gUser.UserId);
					if (user != null)
						description += $"{user.GetName(Context.Guild)}\n";
				}
				IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
				if (string.IsNullOrEmpty(description)) {
					embed.Description = "No users have registered their MFC profile";
				}
				else {
					embed.Description = description;
				}
				await dm.SendMessageAsync("", false, embed.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
		}

		[Command("mfc users url")]
		[Summary("Lists all MFC users registered with the bot and their profile URLs in a direct message")]
		[RequireUserPermissionOrBotOwner(GuildPermission.Administrator)]
		[RequireContext(ContextType.Guild)]
		public async Task GetMFCUserUrl() {
			var embed = new EmbedBuilder() {
				Color = new Color(114, 137, 218),
				Title = $"Users with registered MFC profiles on {Context.Guild.Name}",
			};
			const string profileUrl = @"https://myfigurecollection.net/profile/";
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				string description = "";
				foreach (GuildUser gUser in gUsers) {
					if (gUser.MFCUsername == null)
						continue;
					IGuildUser user = Context.Guild.GetUser(gUser.UserId);
					if (user != null)
						description += $"{user.GetName(Context.Guild)} **|** {profileUrl}{gUser.MFCUsername}\n";
				}
				IDMChannel dm = await Context.User.GetOrCreateDMChannelAsync();
				if (string.IsNullOrEmpty(description)) {
					embed.Description = "No users have registered their MFC profile";
				}
				else {
					embed.Description = description;
				}
				await dm.SendMessageAsync("", false, embed.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
		}

		[Command("tz"), Alias("timezone")]
		[Summary("Gets your timezone registed with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task GetMyTimeZone() {
			IUser user = Context.User;
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.SerealizedTimeZone == null) {
				await ReplyAsync($"You have not registered your timezone.\n" +
					$"Register with the command `t/timezone assign ianaID/abbreviation`.");
			}
			else {
				await WriteTimeZoneEmbed(name, gUser.TimeZone);
			}
		}


		[Command("tzd"), Alias("timezonedivergence")]
		[Summary("Gets your timezone registed with the bot and display it on a divergence meter")]
		public async Task GetMyTimeZoneDivergence() {
			IUser user = Context.User;
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.SerealizedTimeZone == null) {
				await ReplyAsync($"You have not registered your timezone.\n" +
					$"Register with the command `t/timezone assign ianaID/abbreviation`.");
			}
			else {
				await WriteTimeZoneDivergence(name, gUser.TimeZone);
			}
		}

		public async Task WriteTimeZoneDivergence(string username, TimeZoneInfo timeZone,
			TimeZoneInfo yourTimeZone = null)
		{
			try {
				DateTime date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
				//string time = $"{date:MM\\/dd\\/yy}\n{date.TimeOfDay:hh\\:mm\\:ss}";
				string time = $"{date.TimeOfDay:hh\\:mm\\:ss}";
				var args = new DivergenceArgs {
					Scale = DivergenceScale.Small,
					Escape = DivergenceEscape.NewLines,
					Authenticity = DivergenceAuthenticity.Lax,
				};
				using (var bitmap = Divergence.Draw(time, args))
				using (MemoryStream stream = new MemoryStream()) {
					bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					stream.Position = 0;
					await Context.Channel.SendFileAsync(stream, "TimeZoneDivergence.png");
				}
			}
			catch (Exception ex) {
				await ReplyAsync($"**Error:** {ex.Message}");
			}
			await WriteTimeZoneEmbed(username, timeZone, yourTimeZone);
		}

		public async Task WriteTimeZoneEmbed(string username, TimeZoneInfo timeZone, TimeZoneInfo yourTimeZone = null) {
			DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
			string text = $"**{username}'s time:** {time.ToString(@"M\/dd\, h\:mm tt")}";
			if (yourTimeZone != null) {
				TimeSpan offset = timeZone.GetUtcOffset(DateTime.UtcNow) - yourTimeZone.GetUtcOffset(DateTime.UtcNow);
				string offsetStr = $"{(int) offset.TotalDays} days, " + offset.ToString(@"h\:mm");
				if (offset.TotalDays < 1)
					offsetStr = offset.ToString(@"h\:mm");
				if (offsetStr.StartsWith('-'))
					offsetStr = offsetStr.Substring(1);
				if (offset > TimeSpan.Zero) {
					text += $"\n**{username} is ahead of you by:** {offsetStr}";
				}
				else if (offset < TimeSpan.Zero) {
					text += $"\n**{username} is behind you by:** {offsetStr}";
				}
				else {
					text += $"\n**{username} shares the same time as you**";
				}
			}

			await ReplyAsync(text);
		}

		[Command("tz"), Alias("timezone")]
		[Summary("Gets the user's timezone registed with the bot and compares it to your time (if you have it registered)")]
		[Parameters("<user>")]
		[RequireContext(ContextType.Guild)]
		public async Task GetTimeZone(IUser user) {
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.SerealizedTimeZone == null) {
				await ReplyAsync($"{name} has not registered their timezone");
			}
			else {
				GuildUser thisUser = await Settings.GetGuildUser(Context.Guild.Id, Context.User.Id);
				await WriteTimeZoneEmbed(name, gUser.TimeZone, thisUser?.TimeZone);
			}
		}

		[Command("tzd"), Alias("timezonedivergence")]
		[Summary("Gets the user's timezone registed with the bot and compares it to your time (if you have it registered) and display it on a divergence meter")]
		[Parameters("<user>")]
		[RequireContext(ContextType.Guild)]
		public async Task GetTimeZoneDivergence(IUser user) {
			GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
			string name = user.GetName(Context.Guild);
			if (gUser.SerealizedTimeZone == null) {
				await ReplyAsync($"{name} has not registered their timezone");
			}
			else {
				GuildUser thisUser = await Settings.GetGuildUser(Context.Guild.Id, Context.User.Id);
				await WriteTimeZoneDivergence(name, gUser.TimeZone, thisUser?.TimeZone);
			}
		}

		[Command("tz assign"), Alias("timezone assign")]
		[Summary("Registers your timezone with the bot")]
		[Parameters("<iana id/abbreviation>")]
		[RequireContext(ContextType.Guild)]
		[Remarks("Use " + @"<http://www.timezoneconverter.com/cgi-bin/findzone>" + " to help find your timezone name")]
		public async Task AssignTimeZone([Remainder]string input) {
			TimeZoneAmbiguities ambiguities;
			TimeZoneInfo timeZone = TimeZoneService.ParseTimeZone(input, out ambiguities);
			if (timeZone == null && ambiguities == null) {
				await ReplyAsync("No matching timezones found!\nUse " + @"<http://www.timezoneconverter.com/cgi-bin/findzone>" + " to help find your timezone name");
			}
			else if (ambiguities != null) {
				EmbedBuilder embed = new EmbedBuilder() {
					Title = "Abbreviation matches multiple timezones!\nPlease enter the same command but with the number of the correct timezone at the end."
				};
				StringBuilder str = new StringBuilder();

				int index = 1;
				foreach (var pair in ambiguities.Details) {
					DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pair.Value);
					str.Append($"{index}) {pair.Key} [{time.TimeOfDay}]\n");
					index++;
				}
				embed.WithDescription(str.ToString());
				IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();
				await channel.SendMessageAsync("", false, embed.Build());
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.DMSent;
			}
			else {
				using (var database = new BotDatabaseContext()) {
					GuildUser gUser = await Settings.GetGuildUser(Context);
					gUser.TimeZone = timeZone;
					database.GuildUsers.Update(gUser);
					await database.SaveChangesAsync();
					Context.IsSuccess = false;
					Context.CustomError = CustomCommandError.Success;
				}
			}
		}

		[Command("tz unassign"), Alias("timezone unassign")]
		[Summary("Unregisters your timezone with the bot")]
		[RequireContext(ContextType.Guild)]
		public async Task UnassignTimeZone() {
			using (var database = new BotDatabaseContext()) {
				GuildUser gUser = await Settings.GetGuildUser(Context);
				if (gUser.SerealizedTimeZone == null) {
					await ReplyAsync("You do not have a registered timezone to unassign");
					return;
				}
				gUser.TimeZone = null;
				database.GuildUsers.Update(gUser);
				await database.SaveChangesAsync();
				Context.IsSuccess = false;
				Context.CustomError = CustomCommandError.Success;
			}
		}

		[Command("tz usercount"), Alias("timezone usercount")]
		[Summary("Gets the number of users that have registered timezones")]
		[RequireContext(ContextType.Guild)]
		public async Task GetTimeZoneUserCount() {
			using (var database = new BotDatabaseContext()) {
				ulong guildId = Context.Guild.Id;
				var gUsers = database.GuildUsers.Where(u => u.GuildId == guildId);
				int count = 0;
				foreach (GuildUser gUser in gUsers) {
					if (gUser.SerealizedTimeZone != null)
						count++;
				}
				await ReplyAsync($"**Registered Timezones:** {count}");
			}
		}
	}
}
