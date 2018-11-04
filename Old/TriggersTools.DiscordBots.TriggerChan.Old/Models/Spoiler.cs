using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class Spoiler {
		[Key]
		public ulong MessageId { get; set; }

		public string Content { get; set; }
		public string Filename { get; set; }
		public string Url { get; set; }
		public DateTime TimeStamp { get; set; }
	}

	public class SpoiledUser {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public ulong DbId { get; set; }
		[Required]
		public ulong MessageId { get; set; }
		public ulong UserId { get; set; }
		public ulong UserMessageId { get; set; }
		public DateTime TimeStamp { get; set; }
	}
}
