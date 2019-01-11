using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotNetDeploy;

namespace DotNetDeploy {
	public class Filter {
		#region Constants

		public static Filter EmptyIgnore => new Filter(false);
		public static Filter EmptyInclude => new Filter(true);

		#endregion

		#region Fields

		public IReadOnlyList<FilterRule> Rules { get; private set; }
		public bool Include { get; }

		#endregion

		#region Constructors

		private Filter(bool include) {
			Rules = new FilterRule[0].ToImmutableArray();
			Include = include;
		}
		private Filter(IEnumerable<FilterRule> rules, bool include) {
			Rules = rules.ToImmutableArray();
			Include = include;
		}

		#endregion

		#region Static Constructors

		public static Filter FromFile(string filePath, bool include) {
			string[] lines = File.ReadAllLines(filePath);
			List<FilterRule> rules = new List<FilterRule>();
			foreach (string line in lines) {
				if (FilterRule.TryParseLine(line, out FilterRule rule))
					rules.Add(rule);
			}
			return new Filter(rules, include);
		}

		#endregion

		#region Merge

		public void Merge(Filter ignore) {
			if (Include != ignore.Include)
				throw new InvalidOperationException("Cannot merge ignore files with different include settings!");
			Rules = Rules.Concat(ignore.Rules).ToImmutableArray();
		}

		#endregion

		#region Run
		
		public void Run(IEnumerable<FileData> files) {
			foreach (FileData file in files) {
				// Priority from highest to lowest
				bool isDirectoryExclude = false;
				//bool isForcedExclude = false;
				//bool isForcedInclude = false;
				bool isIncluded = !Include;

				foreach (FilterRule rule in Rules) {
					bool includeFlag = Include != rule.Include;
					if (rule.Match(file.RelativeName)) {
						if (rule.Directory) {
							if (!includeFlag) {
								isDirectoryExclude = true;
								break;
							}
							else {
								isIncluded = true;
							}
						}
						else {
							isIncluded = includeFlag;
						}
					}
				}
				if (isDirectoryExclude || !isIncluded) {
					file.Ignore = true;
				}
			}
		}

		#endregion
	}
}
