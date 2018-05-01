using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class Group : ChannelBase {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong GroupId { get => Id; set { Id = value; } }

		[NotMapped]
		public override SettingsType Type => SettingsType.Group;
	}

	public class LocalGroup : LocalChannelBase {
		public ulong GroupId { get => Id; set { Id = value; } }

		public override SettingsType Type => SettingsType.Group;
	}
}
