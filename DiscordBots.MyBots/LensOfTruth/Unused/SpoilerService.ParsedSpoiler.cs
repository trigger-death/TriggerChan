using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Discord;
using Discord.WebSocket;
using TriggersTools.DiscordBots.SpoilerBot.Utils;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService {
		/// <summary>
		/// A parsed spoiler that has yet to be presented.
		/// </summary>
		private class ParsedSpoiler {

			/// <summary>
			/// The channel the spoiler command originated from.
			/// </summary>
			public ISocketMessageChannel Channel { get; set; }
			/// <summary>
			/// The author of the spoiler.
			/// </summary>
			public IUser Author { get; set; }
			/// <summary>
			/// The original spoiler message.
			/// </summary>
			public IUserMessage Message { get; set; }

			/// <summary>
			/// Gets or sets the optional spoiler title.
			/// </summary>
			public string Title { get; set; }
			/// <summary>
			/// Gets or sets the spoiler content.
			/// </summary>
			public string Content { get; set; }

			/// <summary>
			/// True if the spoiler is waiting for an attachment to be sent to DM.
			/// </summary>
			//public bool WaitingForAttachment { get; set; }
			/// <summary>
			/// The Url of the attachment sent in DM.
			/// </summary>
			public string AttachmentUrl { get; set; }
			/// <summary>
			/// The list of Urls presented in the message content.
			/// </summary>
			public string[] Urls { get; set; }

			public Embed BuildEmbed(bool timedOut = false) {
				StringBuilder text = new StringBuilder();
				text.Append("Spoiler");
				if (AttachmentUrl != null && !AttachmentUrl.IsImageUrl())
					text.Append(" with Attachment");

				if (!string.IsNullOrWhiteSpace(Title)) {
					text.Append($": {Format.Sanitize(Title)}");
				}
				text.Append(" | ");

				if (timedOut)
					text.Append("Attachment timed out");
				else
					text.Append("React to hear");


				EmbedBuilder embed = new EmbedBuilder();
				if (timedOut)
					embed.WithColor(Color.Red);
				embed.WithAuthorUsername(Author);
				embed.WithTitle(text.ToString());
				return embed.Build();
			}

			//public Timer ExpireTimer { get; set; }
		}
	}
}
