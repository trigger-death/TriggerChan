using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
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
			if (time.Seconds > 0) {
				string plural = (time.Seconds != 1 ? "s" : "");
				text += string.Format(" {0:%s} sec{1}", time, plural);
			}
			return text.Trim();
		}
		public static string ToPlayString(this TimeSpan time) {
			string str = time.ToString(@"m\:ss");
			if (str.StartsWith(':'))
				str = "0" + str;
			return str;
		}
	}
}
