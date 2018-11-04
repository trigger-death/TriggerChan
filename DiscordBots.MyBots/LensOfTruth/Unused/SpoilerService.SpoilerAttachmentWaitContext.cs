using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using TriggersTools.DiscordBots.Context;
using TriggersTools.DiscordBots.SpoilerBot.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService {
		private class SpoilerAttachmentWaitContext : DiscordWaitContext {

			public Spoiler Spoiler { get; }
			public IUserMessage AttachmentMessage { get; set; }

			public SpoilerAttachmentWaitContext(DiscordCommandContext context, Spoiler spoiler)
				: base(context, "Spoiler Attachment", TimeSpan.FromMinutes(5))
			{
				Spoiler = spoiler;
				OutputChannel = spoiler.Context.User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();
				Expired += OnExpired;
				Canceled += OnCanceled;
			}
			
			private async Task OnExpired(DiscordWaitContext arg) {
				await OutputChannel.SendMessageAsync($"**{Name}:** Timed out!").ConfigureAwait(false);
			}
			private async Task OnCanceled(DiscordWaitContext arg) {
				await OutputChannel.SendMessageAsync($"**{Name}:** Was canceled!").ConfigureAwait(false);
			}
		}
	}
}
