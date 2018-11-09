using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Extensions {
	public static class TimeSpanExtensions {

		public static string ToDHMSString(this TimeSpan time) {
			string text = "";
			if (time.Days > 0) {
				string plural = (time.Days != 1 ? "s" : "");
				text += string.Format(" {0:%d} day{1}", time, plural);
			}
			if (time.Hours > 0) {
				string plural = (time.Hours != 1 ? "s" : "");
				text += string.Format(" {0:%h} hour{1}", time, plural);
			}
			if (time.Minutes > 0) {
				string plural = (time.Minutes != 1 ? "s" : "");
				text += string.Format(" {0:%m} min{1}", time, plural);
			}
			if (time.Seconds > 0 || time.TotalSeconds <= 1) {
				string plural = (time.Seconds != 1 ? "s" : "");
				text += string.Format(" {0:%s} sec{1}", time, plural);
			}
			return text.TrimStart();
		}
		public static string ToPlayString(this TimeSpan time) {
			string str = time.ToString(@"m\:ss");
			if (str.StartsWith(":"))
				str = "0" + str;
			return str;
		}

		public static string ToNamedSuffixString(this DateTime date) {
			return $"{date:MMM} {date:%d}{GetDaySuffix(date)}, {date:yyyy}";
		}

		public static string GetDaySuffix(this DateTime date) {
			switch (date.Day) {
			case 1:
			case 21:
			case 31:
				return "st";
			case 2:
			case 22:
				return "nd";
			case 3:
			case 23:
				return "rd";
			default:
				return "th";
			}
		}
	}
}
