using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Contexts {
	internal class SpoilerAttachmentWaitContext : DiscordBotUserWaitContext {

		public Spoiler Spoiler { get; }
		public IUserMessage StartMessage { get; private set; }

		public SpoilerAttachmentWaitContext(DiscordBotCommandContext context, Spoiler spoiler)
			: base(context, "Spoiler Attachment", TimeSpan.FromMinutes(4))
		{
			Spoiler = spoiler;
			OutputChannel = spoiler.Context.User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult();
			Started += OnStarted;
			Ended += OnEnded;
			Expired += OnExpired;
			Canceled += OnCanceled;
		}

		private async Task OnStarted(SocketUserWaitContext arg) {
			StartMessage = await OutputChannel.SendMessageAsync($"**{Name}:** Send your spoiler attachment here, or type `cancel` to stop").ConfigureAwait(false);
		}
		private async Task OnEnded(SocketUserWaitContext arg) {
			try {
				await StartMessage.DeleteAsync().ConfigureAwait(false);
			} catch { }
		}
		private async Task OnExpired(SocketUserWaitContext arg) {
			await OutputChannel.SendMessageAsync($"**{Name}:** Timed out!").ConfigureAwait(false);
		}
		private async Task OnCanceled(SocketUserWaitContext arg) {
			await OutputChannel.SendMessageAsync($"**{Name}:** Was canceled!").ConfigureAwait(false);
		}
	}
}
