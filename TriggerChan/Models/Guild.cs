using Discord;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public class Guild : SettingsBase {

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong GuildId { get => Id; set { Id = value; } }

		//public List<ulong> PublicRoles { get; set; }

		[NotMapped]
		public override SettingsType Type => SettingsType.Guild;

		public bool CleanupOnLeave { get; set; }

		public string PublicRolesStr { get; set; }

		public bool IsRolePublic(ulong id) {
			return PublicRolesStr.IndexOf($"|{id}|") != -1;
		}

		public bool SetRolePrivate(ulong id) {
			int index = PublicRolesStr.IndexOf($"|{id}|");
			if (index != -1) {
				PublicRolesStr = PublicRolesStr.Remove(index + 1, id.ToString().Length);
			}
			return index != -1;
		}

		public bool SetRolePublic(ulong id) {
			int index = PublicRolesStr.IndexOf($"|{id}|");
			if (index == -1) {
				PublicRolesStr += id.ToString() + "|";
			}
			return index == -1;
		}

		//public List<GuildChannel> GuildChannels { get; set; }
	}

	public class LocalGuild : LocalSettingsBase {
		public ulong GuildId { get => Id; set { Id = value; } }

		public override SettingsType Type => SettingsType.Guild;

		public Process YouTubeProcess { get; set; }

		public AsciifyParameters Asciify { get; set; }

		public bool IsAsciifying {
			get {
				if (Asciify != null) {
					return (!Asciify.Task?.IsCompleted ?? true);
				}
				return false;
			}
		}
	}

	public class AsciifyParameters {
		public ITextChannel Channel { get; set; }
		public IUser User { get; set; }
		public IMessage Message { get; set; }
		public IMessage AttachmentMessage { get; set; }
		public bool Delete { get; set; }
		public IAttachment Attachment { get; set; }

		public int Smoothness { get; set; }
		public float Scale { get; set; }
		
		public DateTime TimeStamp { get; set; }
		public Task Task { get; set; }
		public Timer ExpireTimer { get; set; }

		public Embed BuildTimeoutEmbed() {
			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(Color.Red);
			embed.WithAuthor(User);
			embed.WithTitle("Asciify command attachment timed out");
			return embed.Build();
		}
	}
}
