using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class GuildChannel : ChannelBase {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong ChannelId { get => Id; set { Id = value; } }
		[Required]
		public ulong GuildId { get; set; }

		[NotMapped]
		public override SettingsType Type => SettingsType.GuildChannel;
	}

	public class LocalGuildChannel : LocalChannelBase {
		public ulong ChannelId { get => Id; set { Id = value; } }

		public ulong GuildId { get; set; }

		public override SettingsType Type => SettingsType.GuildChannel;
	}
}
