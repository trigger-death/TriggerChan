using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	partial class ProfilesModule {
		
		[Group("tz"), Alias("timezone")]
		[Usage("[<user>|assign <iana|abbreviation>|unassign]")]
		[Summary("Lookup a user's current time or register your timezone")]
		[Remarks("Use [Time Zone Converter](http://www.timezoneconverter.com/cgi-bin/findzone) to help find your timezone IANA Id")]
		public class TimeZoneGroup : TimeZoneModule {

			private readonly TimeZoneService timeZones;
			private readonly ConfigParserService configParser;

			public TimeZoneGroup(TriggerServiceContainer services,
								 TimeZoneService timeZones,
								 ConfigParserService configParser)
				: base(services, timeZones, configParser)
			{
				this.timeZones = timeZones;
				this.configParser = configParser;
			}
			
			[Name("tz")]
			[Command("")]
			[Example("Displays the current time of your timezone registed with me")]
			public Task MyTimeZone() {
				return MyTimeZoneAsync();
			}

			[Name("tz <user>")]
			[Command("")]
			[Example("@​trigger_death", "Displays the current time of *trigger_death's* timezone registed with me and compares it to your time (if you have one registered)")]
			[Usage("<user>")]
			public Task UserTimeZone(IUser user) {
				return UserTimeZoneAsync(user);
			}

			[Name("tz assign <iana|abbreviation>")]
			[Command("assign")]
			[Example("America/New York", "Registers your timezone with me as *New York*")]
			[Example("ET", "Registers your timezone with me using the abbreviation *ET*")]
			[Usage("<iana|abbreviation>")]
			[Remarks("Use " + @"<http://www.timezoneconverter.com/cgi-bin/findzone>" + " to help find your timezone name")]
			public Task<RuntimeResult> AssignTimeZone([Remainder] string input) {
				return AssignTimeZoneAsync(input);
			}

			[Name("tz unassign")]
			[Command("unassign")]
			[Example("Unregisters your timezone with me")]
			public Task<RuntimeResult> UnassignTimeZone() {
				return UnassignTimeZoneAsync();
			}
		}
		[Group("tzd"), Alias("timezonediv")]
		[Usage("[<user>]")]
		[Summary("Lookup a user's current time and display it with a Divergence Meter")]
		public class TimeZoneDivergenceGroup : TimeZoneModule {

			private readonly TimeZoneService timeZones;
			private readonly ConfigParserService configParser;
			private readonly DivergenceService divergence;

			public TimeZoneDivergenceGroup(TriggerServiceContainer services,
										   TimeZoneService timeZones,
										   ConfigParserService configParser,
										   DivergenceService divergence)
				: base(services, timeZones, configParser)
			{
				this.timeZones = timeZones;
				this.configParser = configParser;
				this.divergence = divergence;
			}

			[Name("tzd")]
			[Command("")]
			[Example("Displays the current time of your timezone registed with me")]
			public Task MyTimeZone() {
				return MyTimeZoneAsync();
			}

			[Name("tzd <user>")]
			[Command("")]
			[Example("@​trigger_death", "Displays the current time of *trigger_death's* timezone registed with me and compares it to your time (if you have one registered)")]
			[Usage("<user>")]
			public Task UserTimeZone(IUser user) {
				return UserTimeZoneAsync(user);
			}

			protected override async Task ReplyTimeZoneAsync(IUser user, TimeZoneInfo timeZone, TimeZoneInfo yourTimeZone = null) {
				DateTime date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
				string time = $"{date.TimeOfDay:hh\\:mm\\:ss}";

				using (var bitmap = divergence.Draw(time, true))
					await ReplyBitmapAsync(bitmap, "TimeZoneDivergence.png").ConfigureAwait(false);
				await base.ReplyTimeZoneAsync(user, timeZone, yourTimeZone).ConfigureAwait(false);
			}
		}
	}
}
