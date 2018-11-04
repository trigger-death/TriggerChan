using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers {
	public struct ParsedUsername {
		public static readonly ParsedUsername Failed = new ParsedUsername();
		
		public string Name { get; set; }
		public long? Id { get; set; }

		public bool IsId => (Name == null && Id.HasValue);
		public string Value => (Name ?? Id?.ToString());
		public bool Success => (Name != null || Id.HasValue);

		public override string ToString() => Value;
	}
}
