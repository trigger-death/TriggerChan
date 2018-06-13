using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class GuildUser {
		[Key]
		[Column(Order = 1)]
		public ulong UserId { get; set; }
		[Key]
		[Column(Order = 2)]
		public ulong GuildId { get; set; }

		public bool IsBotOwner { get; set; }

		public string MALUsername { get; set; }
		public string MFCUsername { get; set; }
		public string SerealizedTimeZone { get; set; }

		[NotMapped]
		public TimeZoneInfo TimeZone {
			get {
				if (SerealizedTimeZone == null)
					return null;
				else
					return TimeZoneInfo.FromSerializedString(SerealizedTimeZone);
			}
			set {
				if (value == null)
					SerealizedTimeZone = null;
				else
					SerealizedTimeZone = value.ToSerializedString();
			}
		}
	}
}
