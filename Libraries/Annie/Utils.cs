using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annie {
	public static class Utils {
		public const int MAXLENGTH = 80;
		public static string[] MatchOneOf(string text, params string[] patterns) {
			Regex re;
			string[] value;
			foreach (string pattern in patterns) {
				re = new Regex(pattern);
				value = re.MatchToGroupValues(text);
				if (value.Length > 0)
					return value;
			}
			return null;
		}
		private static string[] MatchToGroupValues(this Regex regex, string input) {
			var match = regex.Match(input);
			if (match.Success)
				return match.Groups.Cast<Group>().Select(g => g.Value).ToArray();
			return new string[0];
		}
	}
}
