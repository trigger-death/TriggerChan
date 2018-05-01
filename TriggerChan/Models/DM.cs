using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class DM : ChannelBase {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong DMId { get => Id; set { Id = value; } }

		//public ulong UserId { get; set; }

		[NotMapped]
		public override SettingsType Type => SettingsType.DM;
	}

	public class LocalDM : LocalChannelBase {
		public ulong DMId { get => Id; set { Id = value; } }

		//public ulong UserId { get; set; }

		public override SettingsType Type => SettingsType.DM;
	}
}
