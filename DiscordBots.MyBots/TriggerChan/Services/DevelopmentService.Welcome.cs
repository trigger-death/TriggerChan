using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.Extensions;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	partial class DevelopmentService {
		private class Welcome : Embeddable {
			[JsonProperty(PropertyName = "rules")]
			public string[] Rules { get; set; }
			[JsonProperty(PropertyName = "channels")]
			public ChannelItem[] Channels { get; set; }
			[JsonProperty(PropertyName = "links")]
			public string Links { get; set; }

			public override Embed Build(DiscordSocketClient client, IConfigurationRoot config, ulong guildId) {
				EmbedBuilder embed = BuildBase(client, config, guildId);
				BuildRules(embed);
				BuildChannels(embed);
				embed.AddField("Links", Links);
				return embed.Build();
			}
			private void BuildRules(EmbedBuilder embed) {
				embed.PaginateField($"The {Rules.Length} Pledges", Rules.Select((item, i) => $"**`{(i + 1)})`** {item}"));
				//PagifyField(embed, $"The {Rules.Length} Pledges", Rules.Select((item, i) => $"**`{(i + 1)})`** {item}"));
				//if (Rules.Length != 0)
				//	embed.AddField($"The {Rules.Length} Pledges", string.Join("\n", Rules.Select((item, i) => $"**`{(i + 1)})`** {item}")));
			}
			private void BuildChannels(EmbedBuilder embed) {
				embed.PaginateField("Channels of Note", Channels.Select(item => item.Build()));
				//PagifyField(embed, "Channels of Note", Channels.Select(item => item.Build()));
				//if (Channels.Length != 0)
				//	embed.AddField($"Channels of Note", string.Join("\n", Channels.Select(item => item.Build())));
			}
		}
		private class ChannelItem {
			[JsonProperty(PropertyName = "id")]
			public ulong Id { get; set; }
			[JsonProperty(PropertyName = "desc")]
			public string Description { get; set; }

			public string Build() {
				return $"• <#{Id}> **-** {Description}";
			}
		}
	}
}
