using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Annie {
	public static class Signatures {
		public const string jsvarStr = @"[a-zA-Z_\$][a-zA-Z_0-9]*";
		public const string reverseStr = @":function\(a\)\{" +
			@"(?:return )?a\.reverse\(\)" +
			@"\}";
		public const string sliceStr = @":function\(a,b\)\{" +
			@"return a\.slice\(b\)" +
			@"\}";
		public const string spliceStr = @":function\(a,b\)\{" +
			@"a\.splice\(0,b\)" +
			@"\}";
		public const string swapStr = @":function\(a,b\)\{" +
			@"var c=a\[0\];a\[0\]=a\[b(?:%a\.length)?\];a\[b(?:%a\.length)?\]=c(?:;return a)?" +
			@"\}";

		public static readonly Regex actionsObjRegexp = new Regex(
			$@"var ({jsvarStr})=\{{((?:(?:{jsvarStr}{reverseStr}|{jsvarStr}{sliceStr}|{jsvarStr}{spliceStr}|{jsvarStr}{swapStr}),?\n?)+)\}};");
		public static readonly Regex actionsFuncRegexp = new Regex(
			$@"function(?: {jsvarStr})?\(a\)\{{"+
				$@"a=a\.split\(""""\);\s*"+
				$@"((?:(?:a=)?{jsvarStr}\.{jsvarStr}\(a,\d+\);)+)"+
				$@"return a\.join\(""""\)"+
				$@"\}}");
		/*public static readonly Regex actionsFuncRegexp2 = new Regex(
			@"function(?: %s)?\(a\)\{" +
				@"a=a\.split\(""\);\s*" +
				@"((?:(?:a=)?%s\.%s\(a,\d+\);)+)" +
				@"return a\.join\(""\)" +
				@"\}");
		public const string actionsFuncRegexpPattern1 =
			@"function(?: %s)?\(a\)\{" +
				@"a=a\.split\(""\);\s*" +
				@"((?:(?:a=)?%s\.%s\(a,\d+\);)+)" +
				@"return a\.join\(""\)" +
				@"\}";
		public const string actionsFuncRegexpPattern2 = 
			@"function(?: %s)?\(a\)\{" +
				@"a=a\.split\(""\);\s*" +
				@"((?:(?:a=)?%s\.%s\(a,\d+\);)+)" +
				@"return a\.join\(""\)" +
				@"\}";*/
		/*public static readonly string actionsFuncRegexpPattern1 =
			$@"function(?: {jsvarStr})?\(a\)\{{"+
				$@"a=a\.split\(""\);\s*"+
				$@"((?:(?:a=)?{jsvarStr}\.{jsvarStr}\(a,\d+\);)+)"+
				$@"return a\.join\(""\)"+
				$@"\}}";
		public static readonly string actionsFuncRegexpPattern3 = 
			$@"function(?: {jsvarStr})?\(a\)\{{" +
				@"a=a\.split\(""""\);\s*" +
				$@"((?:(?:a=)?{jsvarStr}\.{jsvarStr}\(a,\d+\);)+)" +
				@"return a\.join\(""""\)" +
				$@"\}}";*/
		public static readonly Regex reverseRegexp = new Regex(
			$@"(?m)(?:^|,)({jsvarStr}){reverseStr}", RegexOptions.Multiline);
		public static readonly Regex sliceRegexp = new Regex(
			$@"(?m)(?:^|,)({jsvarStr}){sliceStr}", RegexOptions.Multiline);
		public static readonly Regex spliceRegexp = new Regex(
			$@"(?m)(?:^|,)({jsvarStr}){spliceStr}", RegexOptions.Multiline);
		public static readonly Regex swapRegexp = new Regex(
			$@"(?m)(?:^|,)({jsvarStr}){swapStr}", RegexOptions.Multiline);


		public static string[] getSigTokens(string html) {
			string[] objResult = actionsObjRegexp.MatchToSubGroupValues(html);
			string[] funcResult = actionsFuncRegexp.MatchToSubGroupValues(html);

			if (objResult.Length < 2 || funcResult.Length < 1)
				throw new Exception("error parsing signature tokens");
			string obj = objResult[0].ReplaceDollar();
			string objBody = objResult[1].ReplaceDollar();
			string funcBody = funcResult[0].ReplaceDollar();

			string reverseKey = null, sliceKey = null, spliceKey = null, swapKey = null;
			string result;
			
			if ((result = reverseRegexp.MatchToFirstSubGroupValue(objBody)) != null)
				reverseKey = result.ReplaceDollar();
			if ((result = sliceRegexp.MatchToFirstSubGroupValue(objBody)) != null)
				sliceKey = result.ReplaceDollar();
			if ((result = spliceRegexp.MatchToFirstSubGroupValue(objBody)) != null)
				spliceKey = result.ReplaceDollar();
			if ((result = swapRegexp.MatchToFirstSubGroupValue(objBody)) != null)
				swapKey = result.ReplaceDollar();

			string[] keys = { reverseKey, sliceKey, spliceKey, swapKey };
			Regex regex;
			try {
				regex = new Regex($@"(?:a=)?{obj}\.({string.Join("|", keys)})\(a,(\d+)\)");
			} catch (Exception) {
				throw;
			}

			string[][] results = regex.MatchesToSubGroupValues(funcBody);
			List<string> tokens = new List<string>();
			foreach (string[] s in results) {
				if (s[0] == swapKey)
					tokens.Add($"w{s[1]}");
				else if (s[0] == reverseKey)
					tokens.Add($"r{s[1]}");
				else if (s[0] == sliceKey)
					tokens.Add($"s{s[1]}");
				else if (s[0] == spliceKey)
					tokens.Add($"p{s[1]}");
			}
			return tokens.ToArray();
		}

		/*public static string decipherTokens(string[] tokens, string sig) {
			int pos = 0;
			string[] sigSplit = sig.Split(new[] { "" }, StringSplitOptions.None);
			for (int i = 0, l = tokens.Length; i < l; i++) {
				string tok = tokens[i];
				if (tok.Length > 1) {
					pos = int.Parse(tok.Substring(1));
					pos ^= pos;
				}
				switch (tok[0]) {
				case 'r':
					Array.Reverse(sigSplit);
					break;
				case 'w':
					string swap = sigSplit[0];
					sigSplit[0] = sigSplit[pos];
					sigSplit[pos] = swap;
					break;
				case 's':
				case 'p':
					sigSplit = sigSplit.Skip(pos).ToArray();
					break;
				}
			}
			return string.Join("", sigSplit);
		}*/
		public static string decipherTokens(string[] tokens, string sig) {
			int pos = 0;
			char[] sigSplit = sig.ToCharArray();
			for (int i = 0; i < tokens.Length; i++) {
				string tok = tokens[i];
				if (tok.Length > 1) {
					pos = int.Parse(tok.Substring(1));
					//pos = pos;
				}
				switch (tok[0]) {
				case 'r':
					Array.Reverse(sigSplit);
					break;
				case 'w':
					char swap = sigSplit[0];
					sigSplit[0] = sigSplit[pos];
					sigSplit[pos] = swap;
					break;
				case 's':
				case 'p':
					sigSplit = sigSplit.Skip(pos).ToArray();
					break;
				}
			}
			return new string(sigSplit);
		}

		private static string[] MatchToSubGroupValues(this Regex regex, string input) {
			var match = regex.Match(input);
			if (match.Success)
				return match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).ToArray();
			return new string[0];
		}
		private static string[][] MatchesToSubGroupValues(this Regex regex, string input) {
			var matches = regex.Matches(input);
			string[][] results = new string[matches.Count][];
			for (int i = 0; i < matches.Count; i++) {
				Match match = matches[i];
				if (match.Success)
					results[i] = match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).ToArray();
				else
					results[i] = new string[0];
			}
			return results;
		}
		private static string MatchToFirstSubGroupValue(this Regex regex, string input) {
			var match = regex.Match(input);
			if (match.Success)
				return match.Groups.Cast<Group>().Skip(1).FirstOrDefault()?.Value;
			return null;
		}
		private static string ReplaceDollar(this string s) {
			return s.Replace("$", @"\$");
		}
	}
}