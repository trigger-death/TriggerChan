using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class DevelopmentService : TriggerService {

		#region Fields

		private readonly ulong devStatusChannelId;
		private readonly ulong welcomeChannelId;
		private readonly ulong guildId;

		#endregion

		#region Constructors

		public DevelopmentService(TriggerServiceContainer services)
			: base(services)
		{
			devStatusChannelId = ulong.Parse(Home["channels:dev_status"]);
			welcomeChannelId = ulong.Parse(Home["channels:welcome"]);
			guildId = ulong.Parse(Home["guild"]);
		}

		#endregion
		
		private T Load<T>() where T : Embeddable, new() {
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(new T().File));
		}
		/*private void Save<T>(T embeddable) where T : Embeddable, new() {
			File.WriteAllText(embeddable.File, JsonConvert.SerializeObject(embeddable));
		}*/

		public Task UpdateDevStatusAsync() => UpdateAsync<DevStatus>(devStatusChannelId);
		public Task UpdateWelcomeAsync() => UpdateAsync<Welcome>(welcomeChannelId);

		private async Task UpdateAsync<T>(ulong channelId) where T : Embeddable, new() {
			T embeddable = Load<T>();

			IMessageChannel channel = Client.GetChannel(channelId) as IMessageChannel;
			IUserMessage message = null;
			string content = embeddable.Content;
			Embed embed = embeddable.Build(Client, Config, guildId);
			ulong messageId = LoadMessageId(embeddable);
			if (messageId != 0) {
				try {
					message = await channel.GetMessageAsync(messageId).ConfigureAwait(false) as IUserMessage;
					try {
					await message.ModifyAsync(p => {
						p.Content = content;
						p.Embed = embed;
					}).ConfigureAwait(false);
					} catch { }
					return;
				} catch { }
			}
			message = await channel.SendMessageAsync(content, embed: embed).ConfigureAwait(false);
			SaveMessageId(embeddable, message.Id);
		}

		private ulong LoadMessageId<T>(T embeddable) where T : Embeddable, new() {
			if (File.Exists(embeddable.MessageIdFile))
				return ulong.Parse(File.ReadAllText(embeddable.MessageIdFile));
			return 0;
		}
		private void SaveMessageId<T>(T embeddable, ulong messageId) where T : Embeddable, new() {
			File.WriteAllText(embeddable.MessageIdFile, messageId.ToString());
		}

		private abstract class Embeddable {
			public string File => Path.Combine(AppContext.BaseDirectory, $"{GetType().Name}.json");
			public string MessageIdFile => Path.Combine(AppContext.BaseDirectory, $"{GetType().Name}.MessageId.json");

			//[JsonProperty(PropertyName = "message_id")]
			//public ulong MessageId { get; set; }

			[JsonProperty(PropertyName = "content")]
			public string Content { get; set; }
			[JsonProperty(PropertyName = "title")]
			public string Title { get; set; }
			[JsonProperty(PropertyName = "url")]
			public string Url { get; set; }
			[JsonProperty(PropertyName = "desc")]
			public string Description { get; set; }

			public Color Color { get; set; }
			[JsonProperty(PropertyName = "color")]
			private string ColorStr {
				get => $"#{Color.RawValue:X6}";
				set => Color = ColorUtils.Parse(value);
			}

			protected EmbedBuilder BuildBase(DiscordSocketClient client, IConfigurationRoot config, ulong guildId) {
				EmbedBuilder embed = new EmbedBuilder {
					//Title = Title,
					Description = Description,
					Color = ColorUtils.Parse(config["home_embed_color"]),
					//ThumbnailUrl = Thumbnail,
					Timestamp = DateTime.UtcNow,
					//Url = Url,
				};
				string iconUrl = client.GetGuild(guildId).IconUrl;
				embed.WithAuthor(Title, iconUrl, Url);
				embed.WithFooter("Last Updated");
				return embed;
			}

			public abstract Embed Build(DiscordSocketClient client, IConfigurationRoot config, ulong guildId);
		}
	}
}
