using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeZoneConverter;
using TimeZoneNames;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class TimeZoneAmbiguities {
		public string Abbreviation { get; set; }
		public HashSet<string> IanaTimeZones { get; }
		public bool IsAmbiguous { get { return IanaTimeZones.Count > 1; } }
		public string IanaTimeZone { get { return IanaTimeZones.First(); } }
		public List<TimeZoneInfo> TimeZones {
			get {
				List<TimeZoneInfo> timeZones = new List<TimeZoneInfo>();
				foreach (string iana in IanaTimeZones) {
					timeZones.Add(TZConvert.GetTimeZoneInfo(iana));
				}
				return timeZones;
			}
		}
		public TimeZoneInfo TimeZone {
			get { return TZConvert.GetTimeZoneInfo(IanaTimeZones.First()); }
		}

		public List<KeyValuePair<string, TimeZoneInfo>> Details {
			get {
				List<KeyValuePair<string, TimeZoneInfo>> timeZones = new List<KeyValuePair<string, TimeZoneInfo>>();
				foreach (string iana in IanaTimeZones) {
					timeZones.Add(new KeyValuePair<string, TimeZoneInfo>(iana, TZConvert.GetTimeZoneInfo(iana)));
				}
				return timeZones;
			}
		}

		public TimeZoneAmbiguities(string abbreviation) {
			Abbreviation = abbreviation;
			IanaTimeZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
	}

	public class TimeZoneService : BotServiceBase {

		private static Dictionary<string, TimeZoneAmbiguities> Abbreviations =
			new Dictionary<string, TimeZoneAmbiguities>(StringComparer.OrdinalIgnoreCase);


		public static TimeZoneInfo ParseTimeZone(string input, out TimeZoneAmbiguities ambiguities) {
			ambiguities = null;
			string[] parts = input.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length <= 2 && Abbreviations.TryGetValue(parts[0], out ambiguities)) {

				if (!ambiguities.IsAmbiguous) {
					TimeZoneInfo timeZone = ambiguities.TimeZone;
					ambiguities = null;
					return timeZone;
				}
				else if (parts.Length == 2 && int.TryParse(parts[1], out int index)) {
					if (index >= 1 && index <= ambiguities.IanaTimeZones.Count) {
						TimeZoneInfo timeZone = ambiguities.TimeZones.ElementAt(index - 1);
						ambiguities = null;
						return timeZone;
					}
				}
			}
			else {
				try {
					input = input.Trim().Replace("  ", "_").Replace(' ', '_');
					return TZConvert.GetTimeZoneInfo(input);
				}
				catch { }
			}
			return null;
		}

		public enum DaylightType {
			Generic,
			Standard,
			Daylight,
		}

		public static string GetDaylight(TimeZoneValues values, DaylightType type) {
			switch (type) {
			case DaylightType.Generic:
				return values.Generic;
			case DaylightType.Standard:
				return values.Standard;
			case DaylightType.Daylight:
				return values.Daylight;
			default:
				throw new ArgumentException();
			}
		}

		public static void AddAbbreviations(string iana) {
			var abbreviations = TZNames.GetAbbreviationsForTimeZone(iana, "en-US");
			var names = TZNames.GetNamesForTimeZone(iana, "en-US");
			for (DaylightType type = DaylightType.Generic; type <= DaylightType.Daylight; type++) {
				string abbreviation = GetDaylight(abbreviations, type);
				string name = GetDaylight(names, type);
				if (abbreviation == null || name == null)
					continue;
				TimeZoneAmbiguities ambiguities;
				if (!Abbreviations.TryGetValue(abbreviation, out ambiguities)) {
					ambiguities = new TimeZoneAmbiguities(abbreviation);
					Abbreviations.Add(abbreviation, ambiguities);
				}
				ambiguities.IanaTimeZones.Add(iana);
			}
		}

		static TimeZoneService() {
			foreach (string countryCode in TZNames.GetCountryNames("en-US").Keys) {
				foreach (string iana in TZNames.GetTimeZonesForCountry(countryCode, "en-US").Keys) {
					AddAbbreviations(iana);
				}
			}
		}
	}
}
