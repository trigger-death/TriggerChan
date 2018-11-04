using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.TriggerChan.MFC;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.SteinsGate;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Information")]
	[IsLockable(true)]
	public class InformationModule : TriggerModule {
		
		private readonly ConfigParserService configParser;

		public InformationModule(TriggerServiceContainer services,
								 ConfigParserService configParser)
			: base(services)
		{
			this.configParser = configParser;
		}

		/*[Name("mal")]
		[Group("mal")]
		public class MALGroup : TriggerModule {
			
			public MALGroup(TriggerServiceContainer services) : base(services) { }

			[Name("mal")]
			[Command("")]
			[Example("Gets your MAL profile registed with the bot")]
			public async Task GetMyMAL() {
				IUser user = Context.User;
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				//UserProfile gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.MALUsername == null) {
					await ReplyAsync($"You have not registered your MAL account.\n" +
						$"Register with the command `mal assign YourMALUsername`.").ConfigureAwait(false);
				}
				else {
					await WriteMALProfileEmbed(name, userProfile.MALUsername).ConfigureAwait(false);
				}
			}

			[Name("mal <user>")]
			[Command("")]
			[Summary("Gets the user's MAL profile URL registed with the bot")]
			[Usage("<user>")]
			public async Task GetUserMAL(IUser user) {
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.MALUsername == null) {
					await ReplyAsync($"{name} has not registered their MAL account").ConfigureAwait(false);
				}
				else {
					await WriteMALProfileEmbed(name, userProfile.MALUsername).ConfigureAwait(false);
				}
			}
			[Name("mal profile <malUser>")]
			[Command("profile")]
			[Summary("Display the MAL user's profile")]
			[Usage("<malUser>")]
			public async Task<RuntimeResult> GetMALProfile(string url) {
				string username = ParseMALUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				await WriteMALProfileEmbed(username, username).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("mal assign <malUser>")]
			[Command("assign")]
			[Summary("Registers your MAL profile with the bot")]
			[Usage("<malUser>")]
			public async Task<RuntimeResult> AssignMAL(string url) {
				string username = ParseMALUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					userProfile.MALUsername = username;
					db.ModifyOnly(userProfile, up => up.MALUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("mal unassign")]
			[Command("unassign")]
			[Summary("Unregisters your MAL profile with the bot")]
			public async Task<RuntimeResult> UnassignMAL() {
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					if (userProfile.MALUsername == null) {
						await ReplyAsync("You do not have a registered MAL to unassign").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					userProfile.MALUsername = null;
					db.ModifyOnly(userProfile, up => up.MALUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("mal usercount")]
			[Command("usercount")]
			[Summary("Gets the number of users that have registered MAL profiles")]
			public async Task GetMALUserCount() {
				using (var db = GetDb<TriggerDbContext>()) {
					var userProfiles = db.UserProfiles.Where(up => up.MALUsername != null);
					await ReplyAsync($"**Registered MAL Users:** {userProfiles.Count()}").ConfigureAwait(false);
				}
			}

			private string ParseMALUserName(string url) {
				url = url.Trim();
				if (url.StartsWith("<") && url.EndsWith(">")) {
					url = url.Substring(1, url.Length - 2);
				}
				const string pattern = @"(?:^https?\:\/\/myanimelist\.net\/profile\/|^)((?:\-|\w)+)\/?$";
				RegexOptions options = RegexOptions.IgnoreCase;
				Regex regex = new Regex(pattern, options);
				Match match = regex.Match(url);
				if (match.Success && match.Groups.Count > 0)
					return match.Groups[0].Value;
				return null;
			}

			private async Task WriteMALProfileEmbed(string username, string malUser) {
				using (Context.Channel.EnterTypingState()) {
					const string profileUrl = @"https://myanimelist.net/profile/";
					//await ReplyAsync($"{name}'s MAL: <{profileUrl}{username}>");
					var query = new ProfileQuery(malUser);
					ProfileData profile = await query.GetProfileData().ConfigureAwait(false);
					if (profile == null) {
						await ReplyAsync($"Could not find MAL profile for {Format.Sanitize(malUser)}").ConfigureAwait(false);
						return;
					}
					var embed = new EmbedBuilder() {
						Url = $"{profileUrl}{malUser}",
						ThumbnailUrl = profile.User.ImgUrl,
						Title = $"{malUser}'s MAL Profile",
						Color = new Color(114, 137, 218),
					};
					embed.WithFooter("MyAnimeList.net", @"https://myanimelist.cdn-dena.com/img/sp/icon/apple-touch-icon-256.png");
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


					await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
				}
			}
		}*/
		
		
		/*[Name("ani")]
		[Group("ani")]
		public class AniListGroup : TriggerModule {
			
			public AniListGroup(TriggerServiceContainer services) : base(services) { }

			[Name("ani")]
			[Command("")]
			[Example("Gets your AniList profile registed with the bot")]
			public async Task GetMyAniList() {
				IUser user = Context.User;
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				//UserProfile gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.AniListUsername == null) {
					await ReplyAsync($"You have not registered your AniList account.\n" +
						$"Register with the command `ani assign YourAniListUsername`.").ConfigureAwait(false);
				}
				else {
					await WriteAniListProfileEmbed(name, userProfile.AniListUsername).ConfigureAwait(false);
				}
			}

			[Name("ani <user>")]
			[Command("")]
			[Summary("Gets the user's AniList profile URL registed with the bot")]
			[Usage("<user>")]
			public async Task GetUserAniList(IUser user) {
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.AniListUsername == null) {
					await ReplyAsync($"{name} has not registered their AniList account").ConfigureAwait(false);
				}
				else {
					await WriteAniListProfileEmbed(name, userProfile.AniListUsername).ConfigureAwait(false);
				}
			}
			[Name("ani profile <aniListUser>")]
			[Command("profile")]
			[Summary("Display the AniList user's profile")]
			[Usage("<aniListUser>")]
			public async Task<RuntimeResult> GetAniListProfile(string url) {
				string username = ParseAniListUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				await WriteAniListProfileEmbed(username, username).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("ani assign <aniListUser>")]
			[Command("assign")]
			[Summary("Registers your AniList profile with the bot")]
			[Usage("<aniListUser>")]
			public async Task<RuntimeResult> AssignAniList(string url) {
				string username = ParseAniListUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					userProfile.AniListUsername = username;
					db.ModifyOnly(userProfile, up => up.AniListUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("ani unassign")]
			[Command("unassign")]
			[Summary("Unregisters your AniList profile with the bot")]
			public async Task<RuntimeResult> UnassignAniList() {
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					if (userProfile.AniListUsername == null) {
						await ReplyAsync("You do not have a registered AniList to unassign").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					userProfile.AniListUsername = null;
					db.ModifyOnly(userProfile, up => up.AniListUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("ani usercount")]
			[Command("usercount")]
			[Summary("Gets the number of users that have registered AniList profiles")]
			public async Task GetAniListUserCount() {
				using (var db = GetDb<TriggerDbContext>()) {
					var userProfiles = db.UserProfiles.Where(up => up.AniListUsername != null);
					await ReplyAsync($"**Registered AniList Users:** {userProfiles.Count()}").ConfigureAwait(false);
				}
			}
			
			private string ParseAniListUserName(string url) {
				url = url.Trim();
				if (url.StartsWith("<") && url.EndsWith(">")) {
					url = url.Substring(1, url.Length - 2);
				}
				const string pattern = @"(?:^https?\:\/\/anilist\.co\/user\/|^)((?:\-|\w)+)\/?$";
				RegexOptions options = RegexOptions.IgnoreCase;
				Regex regex = new Regex(pattern, options);
				Match match = regex.Match(url);
				if (match.Success && match.Groups.Count > 0)
					return match.Groups[0].Value;
				return null;
			}

			private async Task WriteAniListProfileEmbed(string username, string aniUser) {
				using (Context.Channel.EnterTypingState()) {
					//string profileUrl = @"https://anilist.co/user/";
					AniListProfile profile = null;
					try {
						profile = await AniListProfile.LoadProfile(aniUser).ConfigureAwait(false);
					} catch { }
					if (profile == null) {
						await ReplyAsync($"Could not find AniList profile for {Format.Sanitize(aniUser)}").ConfigureAwait(false);
						return;
					}
					var embed = new EmbedBuilder() {
						Url = profile.ProfileUrl,
						ThumbnailUrl = profile.AvatarUrl,
						Title = $"{aniUser}'s AniList Profile",
						Color = new Color(2, 169, 255),
						//Color = new Color(18, 25, 35),
					};
					embed.WithFooter("AniList.co", @"https://anilist.co/img/icons/android-chrome-512x512.png");
					string desc = "";
					desc += $"Days: {profile.AnimeList.DaysSpent.ToString("#,##0.#")}\n";
					desc += $"Mean Score: {profile.AnimeList.MeanScore.ToString("0.00")}\n";
					desc += $"Completed: {profile.AnimeList.Completed.ToString("#,##0")}\n";
					desc += $"Episodes: {profile.AnimeList.Episodes.ToString("#,###,##0")}\n";
					embed.AddField($"Anime", desc, true);

					desc = "";
					desc += $"Mean Score: {profile.MangaList.MeanScore.ToString("0.00")}\n";
					desc += $"Completed: {profile.MangaList.Completed.ToString("#,##0")}\n";
					desc += $"Volumes: {profile.MangaList.Volumes.ToString("#,###,##0")}\n";
					desc += $"Chapters: {profile.MangaList.Episodes.ToString("#,###,##0")}\n";
					embed.AddField($"Manga", desc, true);


					await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
				}
			}
		}


		[Name("mfc")]
		[Group("mfc")]
		public class MFCGroup : TriggerModule {

			public MFCGroup(TriggerServiceContainer services) : base(services) { }

			[Name("mfc")]
			[Command("")]
			[Example("Gets your MFC profile registed with the bot")]
			public async Task GetMyMFC() {
				IUser user = Context.User;
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				//UserProfile gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.MFCUsername == null) {
					await ReplyAsync($"You have not registered your MFC account.\n" +
						$"Register with the command `mfc assign YourMFCUsername`.").ConfigureAwait(false);
				}
				else {
					await WriteMFCProfileEmbed(name, userProfile.MFCUsername).ConfigureAwait(false);
				}
			}

			[Name("mfc <user>")]
			[Command("")]
			[Summary("Gets the user's MFC profile URL registed with the bot")]
			[Usage("<user>")]
			public async Task GetUserMFC(IUser user) {
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.MFCUsername == null) {
					await ReplyAsync($"{name} has not registered their MFC account").ConfigureAwait(false);
				}
				else {
					await WriteMFCProfileEmbed(name, userProfile.MFCUsername).ConfigureAwait(false);
				}
			}
			[Name("mfc profile <mfcUser>")]
			[Command("profile")]
			[Summary("Display the MFC user's profile")]
			[Usage("<mfcUser>")]
			public async Task<RuntimeResult> GetMFCProfile(string url) {
				string username = ParseMFCUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				await WriteMFCProfileEmbed(username, username).ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("mfc assign <mfcUser>")]
			[Command("assign")]
			[Summary("Registers your MFC profile with the bot")]
			[Usage("<mfcUser>")]
			public async Task<RuntimeResult> AssignMFC(string url) {
				string username = ParseMFCUserName(url);
				if (username == null) {
					return EmoteResults.FromInvalidArgument();
				}
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					userProfile.MFCUsername = username;
					db.ModifyOnly(userProfile, up => up.MFCUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("mfc unassign")]
			[Command("unassign")]
			[Summary("Unregisters your MFC profile with the bot")]
			public async Task<RuntimeResult> UnassignMFC() {
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					if (userProfile.MFCUsername == null) {
						await ReplyAsync("You do not have a registered MFC to unassign").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					userProfile.MFCUsername = null;
					db.ModifyOnly(userProfile, up => up.MFCUsername);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("mfc usercount")]
			[Command("usercount")]
			[Summary("Gets the number of users that have registered MFC profiles")]
			public async Task GetMFCUserCount() {
				using (var db = GetDb<TriggerDbContext>()) {
					var userProfiles = db.UserProfiles.Where(up => up.MFCUsername != null);
					await ReplyAsync($"**Registered MFC Users:** {userProfiles.Count()}").ConfigureAwait(false);
				}
			}
			
			private string ParseMFCUserName(string url) {
				url = url.Trim();
				if (url.StartsWith("<") && url.EndsWith(">")) {
					url = url.Substring(1, url.Length - 2);
				}
				//const string pattern = @"(?:^https?\:\/\/myanimelist\.net\/profile\/|^)((?:\-|\w)+)\/?$";
				const string pattern = @"(?:^https?\:\/\/myfigurecollection\.net\/profile\/|^)((?:\-|\w)+)\/?$";
				RegexOptions options = RegexOptions.IgnoreCase;
				Regex regex = new Regex(pattern, options);
				Match match = regex.Match(url);
				if (match.Success && match.Groups.Count > 0)
					return match.Groups[0].Value;
				return null;
			}

			public async Task WriteMFCProfileEmbed(string username, string mfcUser) {
				using (Context.Channel.EnterTypingState()) {
					const string profileUrl = @"https://myfigurecollection.net/profile/";
					var query = new ProfileQuery(mfcUser);
					MFCProfile profile;
					try {
						profile = await MFCProfile.LoadProfile(mfcUser).ConfigureAwait(false);
					} catch (Exception) {
						await ReplyAsync($"Could not find MFC profile for {Format.Sanitize(mfcUser)}").ConfigureAwait(false);
						return;
					}
					var embed = new EmbedBuilder() {
						Url = $"{profileUrl}{mfcUser}",
						ThumbnailUrl = profile.AvatarUrl,
						Title = $"{mfcUser}'s MFC Profile",
						Color = new Color(87, 161, 82),
					};
					embed.WithFooter("MyFigureCollection.net", @"https://i.imgur.com/GW1AdaV.jpg");
					if (!string.IsNullOrWhiteSpace(profile.Status)) {
						embed.WithDescription(profile.Status);
					}

					//if (profile.Any) {
					foreach (var cat in profile.Categories) {
						embed.AddField(cat.Type.ToString(), cat.ToString(), true);
					}


					await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
				}
			}
		}*/

		[Name("tz")]
		[Group("tz"), Alias("timezone")]
		public class TimeZoneGroup : TriggerModule {

			private readonly TimeZoneService timeZones;
			private readonly ConfigParserService configParser;

			public TimeZoneGroup(TriggerServiceContainer services,
								 TimeZoneService timeZones,
								 ConfigParserService configParser)
				: base(services)
			{
				this.timeZones = timeZones;
				this.configParser = configParser;
			}
			
			[Name("tz")]
			[Command("")]
			[Summary("Gets your timezone registed with me")]
			public async Task GetMyTimeZone() {
				IUser user = Context.User;
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				//GuildUser gUser = await Settings.GetGuildUser(Context.Guild.Id, user.Id);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.TimeZone == null) {
					await ReplyAsync($"You have not registered your timezone.\n" +
						$"Register with the command `t/timezone assign ianaID/abbreviation`.").ConfigureAwait(false);
				}
				else {
					await WriteTimeZoneEmbed(name, userProfile.TimeZone).ConfigureAwait(false);
				}
			}

			[Name("tz <user>")]
			[Command("")]
			[Summary("Gets the user's timezone registed with me and compares it to your time (if you have one registered)")]
			[Usage("<user>")]
			public async Task GetTimeZone(IUser user) {
				UserProfile userProfile;
				UserProfile thisUserProfile;
				using (var db = GetDb<TriggerDbContext>()) {
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
					thisUserProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
				}
				string name = user.GetName(Context.Guild, true);
				if (userProfile.TimeZone == null) {
					await ReplyAsync($"{name} has not registered their timezone").ConfigureAwait(false);
				}
				else {
					await WriteTimeZoneEmbed(name, userProfile.TimeZone, thisUserProfile?.TimeZone).ConfigureAwait(false);
				}
			}

			[Name("tz assign <iana/abbreviation>")]
			[Command("assign")]
			[Summary("Registers your timezone with me")]
			[Usage("<iana/abbreviation>")]
			[Remarks("Use " + @"<http://www.timezoneconverter.com/cgi-bin/findzone>" + " to help find your timezone name")]
			public async Task<RuntimeResult> AssignTimeZone([Remainder] string input) {
				TimeZoneInfo timeZone = timeZones.ParseTimeZone(input, out TimeZoneAbbreviationMatches ambiguities);
				if (timeZone == null && ambiguities == null) {
					await ReplyAsync("No matching timezones found!\nUse " + @"<http://www.timezoneconverter.com/cgi-bin/findzone>" + " to help find your timezone name").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
				else if (ambiguities != null) {
					/*DateTime now = DateTime.UtcNow;
					EmbedBuilder embed = new EmbedBuilder() {
						Title = "Abbreviation matches multiple timezones!\nPlease enter the number of the correct timezone in this channel",
						Color = configParser.EmbedColor,
					};
					StringBuilder str = new StringBuilder();

					int index = 1;
					foreach (var ianaTimeZone in ambiguities.TimeZones) {
						DateTime time = TimeZoneInfo.ConvertTimeFromUtc(now, ianaTimeZone.TimeZone);
						str.Append($"`[{index}]` {ianaTimeZone.Iana} [${time.TimeOfDay:hh\\:mm\\:ss}]\n");
						index++;
					}
					embed.WithDescription(str.ToString());
					var dm = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
					await dm.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);*/
					TimeZoneAbbreviationWaitContext wait = new TimeZoneAbbreviationWaitContext(Context, ambiguities, configParser);
					await wait.StartAsync().ConfigureAwait(false);
					return EmoteResults.FromDMSent();
				}
				else {
					using (var db = GetDb<TriggerDbContext>()) {
						UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
						userProfile.TimeZone = timeZone;
						db.ModifyOnly(userProfile, up => up.TimeZone);
						await db.SaveChangesAsync().ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
			}

			[Name("tz unassign")]
			[Command("unassign")]
			[Summary("Unregisters your timezone with me")]
			public async Task<RuntimeResult> UnassignTimeZone() {
				using (var db = GetDb<TriggerDbContext>()) {
					UserProfile userProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
					if (userProfile.TimeZone == null) {
						await ReplyAsync("You do not have a registered timezone to unassign").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					userProfile.TimeZone = null;
					db.ModifyOnly(userProfile, up => up.TimeZone);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return EmoteResults.FromSuccess();
				}
			}

			[Name("tz usercount")]
			[Command("usercount")]
			[Summary("Gets the number of users that have registered timezones")]
			public async Task GetTimeZoneUserCount() {
				using (var db = GetDb<TriggerDbContext>()) {
					var userProfiles = db.UserProfiles.Where(up => up.TimeZone != null);
					await ReplyAsync($"**Registered Timezones:** {userProfiles.Count()}").ConfigureAwait(false);
				}
			}


			private async Task WriteTimeZoneDivergence(string username, TimeZoneInfo timeZone,
				TimeZoneInfo yourTimeZone = null) {
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
						await Context.Channel.SendFileAsync(stream, "TimeZoneDivergence.png").ConfigureAwait(false);
					}
				} catch (Exception ex) {
					await ReplyAsync($"**Error:** {ex.Message}").ConfigureAwait(false);
				}
				await WriteTimeZoneEmbed(username, timeZone, yourTimeZone).ConfigureAwait(false);
			}

			private async Task WriteTimeZoneEmbed(ICommandContext context, IUser user, TimeZoneInfo timeZone, TimeZoneInfo yourTimeZone = null) {
				DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
				string name = user.GetName(context.Guild, true);
				string text = $"**{name}'s time:** {time.ToString(@"MMM dd, h\:mm")}";
				if (yourTimeZone != null) {
					TimeSpan offset = timeZone.GetUtcOffset(DateTime.UtcNow) - yourTimeZone.GetUtcOffset(DateTime.UtcNow);
					string offsetStr = $"{(int) offset.TotalDays} days, " + offset.ToString(@"h\:mm");
					if (offset.TotalDays < 1)
						offsetStr = offset.ToString(@"h\:mm");
					if (offsetStr.StartsWith("-"))
						offsetStr = offsetStr.Substring(1);
					if (offset > TimeSpan.Zero) {
						text += $"\n**{name} is ahead of you by:** {offsetStr}";
					}
					else if (offset < TimeSpan.Zero) {
						text += $"\n**{name} is behind you by:** {offsetStr}";
					}
					else {
						text += $"\n**{name} shares the same time as you**";
					}
				}

				await ReplyAsync(text).ConfigureAwait(false);
			}
		}

		[Name("tzd")]
		[Group("tzd")]
		public class TimeZoneDivergenceGroup : TriggerModule {

			public TimeZoneDivergenceGroup(TriggerServiceContainer services) : base(services) { }

			[Name("tzd")]
			[Command("")]
			[Summary("Gets your timezone registed with the bot and display it on a divergence meter")]
			public async Task GetMyTimeZoneDivergence() {
				IUser user = Context.User;
				UserProfile userProfile;
				using (var db = GetDb<TriggerDbContext>())
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				string name = user.GetName(Context.Guild, true);
				if (userProfile.TimeZone == null) {
					await ReplyAsync($"You have not registered your timezone.\n" +
						$"Register with the command `t/timezone assign ianaID/abbreviation`.").ConfigureAwait(false);
				}
				else {
					await WriteTimeZoneDivergence(name, userProfile.TimeZone).ConfigureAwait(false);
				}
			}


			[Name("tzd <user>")]
			[Command("")]
			[Summary("Gets the user's timezone registed with the bot and compares it to your time (if you have it registered) and display it on a divergence meter")]
			[Usage("<user>")]
			public async Task GetTimeZoneDivergence(IUser user) {
				UserProfile userProfile;
				UserProfile thisUserProfile;
				using (var db = GetDb<TriggerDbContext>()) {
					userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
					thisUserProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
				}
				string name = user.GetName(Context.Guild, true);
				if (userProfile.TimeZone == null) {
					await ReplyAsync($"{name} has not registered their timezone").ConfigureAwait(false);
				}
				else {
					await WriteTimeZoneDivergence(name, userProfile.TimeZone, thisUserProfile?.TimeZone).ConfigureAwait(false);
				}
			}

			private async Task WriteTimeZoneDivergence(string username, TimeZoneInfo timeZone,
				TimeZoneInfo yourTimeZone = null) {
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
						await Context.Channel.SendFileAsync(stream, "TimeZoneDivergence.png").ConfigureAwait(false);
					}
				} catch (Exception ex) {
					await ReplyAsync($"**Error:** {ex.Message}").ConfigureAwait(false);
				}
				await WriteTimeZoneEmbed(username, timeZone, yourTimeZone).ConfigureAwait(false);
			}

			private async Task WriteTimeZoneEmbed(string username, TimeZoneInfo timeZone, TimeZoneInfo yourTimeZone = null) {
				DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
				string text = $"**{username}'s time:** {time.ToString(@"M\/dd\, h\:mm tt")}";
				if (yourTimeZone != null) {
					TimeSpan offset = timeZone.GetUtcOffset(DateTime.UtcNow) - yourTimeZone.GetUtcOffset(DateTime.UtcNow);
					string offsetStr = $"{(int) offset.TotalDays} days, " + offset.ToString(@"h\:mm");
					if (offset.TotalDays < 1)
						offsetStr = offset.ToString(@"h\:mm");
					if (offsetStr.StartsWith("-"))
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

				await ReplyAsync(text).ConfigureAwait(false);
			}
		}



		
	}
}
