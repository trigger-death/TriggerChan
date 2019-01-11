using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetDeploy {
	public class FilterRule {
		#region Constants

		private const string StartPattern = @"(?:^|/)";
		private const string EndPattern = @"(?:$|/)";
		private const string StartingSlash = @"^";
		private const string Question = @"[^/]";
		private const string SingleAsterisk = @"[^/]*?";
		private const string DoubleAsterisk = @".*?";

		#endregion

		#region Fields

		public string Pattern { get; }
		public bool Include { get; private set; }
		public bool Directory { get; private set; }
		public Regex Regex { get; private set; }

		#endregion

		#region Constructors

		private FilterRule(string pattern) {
			Pattern = pattern;
			Include = false;
			Directory = false;
		}
		private FilterRule(Regex regex, bool include = false, bool directory = false) {
			Pattern = null;
			Include = include;
			Directory = directory;
			Regex = regex;
		}

		#endregion

		#region Static Constructors

		public static FilterRule FromRegex(Regex regex, bool include = false, bool directory = false) {
			return new FilterRule(regex, include, directory);
		}

		#endregion

		#region Match

		public bool Match(string file) {
			return Regex.IsMatch(file);
		}

		#endregion

		#region Parsing

		public static bool TryParseLine(string line, out FilterRule rule) {
			rule = null;
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
				return false;

			// Trim trailing unescaped whitespace
			for (int i = line.Length - 1; i >= 0; i++) {
				if (!char.IsWhiteSpace(line[i]) || (i >= 1 && line[i - 1] == '\\')) {
					line = line.Substring(0, i + 1);
					break;
				}
			}

			rule = new FilterRule(line);

			StringBuilder pattern = new StringBuilder();

			int start = 0;
			int end = line.Length;
			if (line.StartsWith("!")) {
				rule.Include = true;
				start++;
			}
			if (line.StartsWith("/")) {
				pattern.Append(StartingSlash);
				start++;
			}
			if (line.EndsWith("/")) {
				rule.Directory = true;
				end--;
			}

			bool escape = false;
			for (int i = start; i < end; i++) {
				char c = line[i];

				if (escape) {
					pattern.Append(Regex.Escape(c.ToString()));
					escape = false;
					continue;
				}

				switch (c) {
				// Escape
				case '\\':
					escape = true;
					break;
				
				// Wildcards
				case '*':
					if (i + 1 < end && line[i + 1] == '*') {
						pattern.Append(DoubleAsterisk);
						i++;
					}
					else
						pattern.Append(SingleAsterisk);
					break;
				case '?':
					pattern.Append(Question);
					break;
				
				// Range
				case '[':
					i++;
					pattern.Append('[');
					if (i < end && line[i] == '^') {
						pattern.Append('^');
						i++;
					}
					for (; i < end; i++) {
						char rangeC = line[i];
						if (rangeC == ']') {
							pattern.Append(']');
							break;
						}
						pattern.Append(Regex.Escape(rangeC.ToString()));
					}
					if (pattern[pattern.Length - 1] != ']') {
						// We failed to end the range, parse failed.
						rule = null;
						return false;
					}
					break;
			
				// Everything Else
				default:
					pattern.Append(Regex.Escape(c.ToString()));
					break;
				}
			}
			if (escape) {
				// We failed to escape something, parse failed.
				rule = null;
				return false;
			}

			string p = pattern.ToString();
			if (!p.StartsWith(StartingSlash) && !p.StartsWith(DoubleAsterisk)) {
				pattern.Insert(0, StartPattern);
			}
			pattern.Append(EndPattern);

			rule.Regex = new Regex(pattern.ToString());

			return true;
		}

		#endregion

		#region ToString

		public override string ToString() => Pattern;

		#endregion
	}
}
