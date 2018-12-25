using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	partial class DevelopmentService {
		private class DevStatus : Embeddable {
			[JsonProperty(PropertyName = "working")]
			public DevItem[] Working { get; set; }
			[JsonProperty(PropertyName = "backlog")]
			public DevItem[] Backlog { get; set; }
			[JsonProperty(PropertyName = "eternal")]
			public DevItem[] Eternal { get; set; }

			public override Embed Build(DiscordSocketClient client, IConfigurationRoot config, ulong guildId) {
				EmbedBuilder embed = BuildBase(client, config, guildId);
				BuildList("Currently Working On", Working, embed);
				BuildList("Backlog", Backlog, embed);
				BuildList("Eternal Backlog", Eternal, embed);
				return embed.Build();
			}
			private void BuildList(string name, DevItem[] list, EmbedBuilder embed) {
				if (list.Length != 0)
					embed.AddField($"{name}", string.Join("\n", list.Select(item => item.Build())));
			}
		}
		private class DevItem {
			[JsonProperty(PropertyName = "status")]
			public string Status { get; set; }
			[JsonProperty(PropertyName = "title")]
			public string Title { get; set; }
			[JsonProperty(PropertyName = "url")]
			public string Url { get; set; }
			[JsonProperty(PropertyName = "desc")]
			public string Description { get; set; }

			public string Build() {
				StringBuilder str = new StringBuilder();
				str.Append("• ");
				if (Status != null)
					str.Append($"`[{Status}]` ");
				if (Title != null) {
					if (Url != null)
						str.Append($"**[{Title}]({Url})**");
					else
						str.Append($"**{Title}**");
					if (Description != null) {
						str.AppendLine();
						//str.Append(".   ◦ ");
					}
					//str.Append(" **-** ");
				}
				if (Description != null)
					str.Append(Description);
				return str.ToString();
			}
		}
	}
}
