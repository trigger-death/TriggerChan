using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Info {
	[AttributeUsage(AttributeTargets.Method)]
	public class IsDuplicateAttribute : Attribute {

		public bool DifferentFunctionality { get; }

		public IsDuplicateAttribute(bool differentFunctionality) {
			DifferentFunctionality = differentFunctionality;
		}
	}

	/*[AttributeUsage(AttributeTargets.Method)]
	public class UsageAttribute : Attribute {
		public string Usage { get; }

		public UsageAttribute(string usage) {
			Usage = usage;
		}
	}*/

	[AttributeUsage(AttributeTargets.Method)]
	public class ParametersAttribute : Attribute {
		public string Parameters { get; }

		public ParametersAttribute(string parameters) {
			Parameters = parameters;
		}
	}
}
