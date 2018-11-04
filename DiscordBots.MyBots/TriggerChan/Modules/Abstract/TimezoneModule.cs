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
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.TriggerChan.Profiles;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public abstract class TimeZoneModule : TriggerModule {

		private readonly TimeZoneService timeZones;
		private readonly ConfigParserService configParser;

		protected TimeZoneModule(TriggerServiceContainer services,
								 TimeZoneService timeZones,
								 ConfigParserService configParser)
			: base(services)
		{
			this.timeZones = timeZones;
			this.configParser = configParser;
		}

		protected async Task MyTimeZoneAsync() {
			IUser user = Context.User;
			UserProfile userProfile;
			using (var db = GetDb<TriggerDbContext>())
				userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
			if (userProfile.TimeZone == null) {
				await ReplyAsync($"You have not registered your timezone.\n" +
					$"Use the command `tz assign <iana/abbreviation>` to register your timezone.").ConfigureAwait(false);
			}
			else {
				await ReplyTimeZoneAsync(user, userProfile.TimeZone).ConfigureAwait(false);
			}
		}
		protected async Task UserTimeZoneAsync(IUser user) {
			UserProfile userProfile;
			UserProfile thisUserProfile;
			using (var db = GetDb<TriggerDbContext>()) {
				userProfile = await db.FindUserProfileAsync(user.Id).ConfigureAwait(false);
				thisUserProfile = await db.FindUserProfileAsync(Context.User.Id).ConfigureAwait(false);
			}
			if (userProfile.TimeZone == null) {
				string name = user.GetName(Context.Guild, true);
				await ReplyAsync($"{name} has not registered their timezone").ConfigureAwait(false);
			}
			else {
				await ReplyTimeZoneAsync(user, userProfile.TimeZone, thisUserProfile?.TimeZone).ConfigureAwait(false);
			}
		}

		protected async Task<RuntimeResult> AssignTimeZoneAsync(string input) {
			TimeZoneInfo timeZone = timeZones.ParseTimeZone(input, out TimeZoneAbbreviationMatches ambiguities);
			if (timeZone == null && ambiguities == null) {
				await ReplyAsync("No matching timezones found!\nUse <http://www.timezoneconverter.com/cgi-bin/findzone> " +
					"to help find your timezone name.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			else if (ambiguities != null) {
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
		protected async Task<RuntimeResult> UnassignTimeZoneAsync() {
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

		protected virtual async Task ReplyTimeZoneAsync(IUser user, TimeZoneInfo timeZone, TimeZoneInfo yourTimeZone = null) {
			DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
			string name = user.GetName(Context.Guild, true);
			string text = $"**{name}'s time:** {time.ToString(@"MMM dd, h\:mm")}";
			if (yourTimeZone != null) {
				TimeSpan offset = timeZone.GetUtcOffset(DateTime.UtcNow) - yourTimeZone.GetUtcOffset(DateTime.UtcNow);
				string offsetStr = $"{(int) Math.Abs(offset.TotalDays)} days, " + offset.ToString(@"h\:mm");
				if (offset.TotalDays < 1)
					offsetStr = offset.ToString(@"h\:mm");
				//if (offsetStr.StartsWith("-"))
				//	offsetStr = offsetStr.Substring(1);
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
}
