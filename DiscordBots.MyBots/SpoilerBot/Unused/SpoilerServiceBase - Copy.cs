using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Contexts;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Utils;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	/// <summary>
	/// A service for managing the creation and handling of spoilers.
	/// </summary>
	public abstract partial class SpoilerServiceBase : DiscordBotService {
		
		public static class SpoilerReactions {

			public static Emoji ViewSpoiler { get; } = new Emoji("🔍");
			public static Emoji Exception { get; } = new Emoji("⚠");
		}

		#region Fields

		/// <summary>
		/// The collection of tasks running to spoil and unspoil users.
		/// </summary>
		private readonly SpoilTaskCollection spoilerTasks = new SpoilTaskCollection();
		/// <summary>
		/// The color to use for embeds.
		/// </summary>
		private readonly Color embedColor;
		
		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SpoilerServiceBase"/>.
		/// </summary>
		public SpoilerServiceBase(DiscordBotServiceContainer services) : base(services) {
			Client.MessageDeleted += OnMessageDeleted;
			Client.ReactionAdded += OnReactionAdded;
			Client.ReactionRemoved += OnReactionRemoved;
			DiscordBot.DeletingEndUserData += OnDeletingEndUserData;
			embedColor = ColorUtils.Parse(Config["embed_color"]);
		}

		#endregion

		#region Abstract Methods

		protected abstract SpoilerDbContext GetDb<TDbContext>();

		#endregion

		#region Event Handlers

		/// <summary>
		/// Deactivates removed spoilers.
		/// </summary>
		private async Task OnDeletingEndUserData(EndUserDataEventArgs e) {
			if (e.Type == EndUserDataType.User) {
				using (var db = GetDb<SpoilerDbContext>()) {
					SpoilerUser user = await db.SpoilerUsers.FindAsync(e.Id).ConfigureAwait(false);
					if (user == null)
						return;
					await db.Entry(user).Collection(u => u.Spoilers).LoadAsync().ConfigureAwait(false);
					foreach (Spoiler spoiler in user.Spoilers) {
						await DeactivateSpoilerAsync(spoiler).ConfigureAwait(false);
					}
				}
			}
		}
		/// <summary>
		/// Checks if a spoiler was deleted and if so, removes it from the database.
		/// </summary>
		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel) {
			using (var db = GetDb<SpoilerDbContext>()) {
				// Cleanup deleted spoilers
				Spoiler spoiler = await FindSpoilerAsync(db, msg.Id).ConfigureAwait(false);
				if (spoiler != null) {
					Debug.WriteLine("SPOILED DELETED!");
					await RemoveSpoilerAsync(db, spoiler).ConfigureAwait(false);
					return;
				}
			}
		}
		/// <summary>
		/// Checks if a spoiler was reacted to.
		/// </summary>
		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
			var user = reaction.User.Value;
			var emote = reaction.Emote;

			// Ignore those filthy robots
			if (!user.IsBot && SpoilerReactions.ViewSpoiler.Equals(emote)) {
				await SpoilUserAsync(msg.Id, user, false).ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Checks if a spoiler was unreacted to.
		/// </summary>
		private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
			var user = reaction.User.Value;
			var emote = reaction.Emote;

			// Ignore those filthy robots
			if (!user.IsBot && SpoilerReactions.ViewSpoiler.Equals(emote)) {
				await UnspoilUserAsync(msg.Id, user, false).ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Checks the attachment wait context for an attachment.
		/// </summary>
		private async Task OnAttachmentWaitMessageReceivedAsync(SocketUserWaitContext sender, IUserMessage msg) {
			var wait = (SpoilerAttachmentWaitContext) sender;
			if (msg.Channel is IDMChannel) {
				string input = msg.Content.Trim().ToLower();
				if (input == "cancel") {
					await wait.CancelAsync().ConfigureAwait(false);
				}
				else if (msg.Attachments.Any()) {
					Spoiler spoiler = wait.Spoiler;
					try {
						spoiler.AttachmentUrl = msg.Attachments.First().Url;
						//spoiler.WaitingForAttachment = false;
						if (await wait.FinishAsync().ConfigureAwait(false)) {
							await PostSpoilerAsync(spoiler).ConfigureAwait(false);
							await wait.OutputChannel.SendMessageAsync($"**{wait.Name}:** Spoiler posted!").ConfigureAwait(false);
						}
					} catch (Exception) {
						await msg.AddReactionAsync(SpoilerReactions.Exception).ConfigureAwait(false);
						var dm = await spoiler.Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
						await dm.SendMessageAsync("**Failed Spoiler:**").ConfigureAwait(false);
						await dm.SendMessageAsync(spoiler.Context.Message.Content).ConfigureAwait(false);
					}
				}
			}
		}

		#endregion
		
		#region Run Spoiler

		/// <summary>
		/// Runs the spoiler command from the <see cref="Modules.SpoilerModule"/>.
		/// </summary>
		/// <param name="context">The context of the spoiler command.</param>
		/// <param name="content">The content of the spoiler command.</param>
		/// <param name="waitForAttachment">True if the spoiler is an upload spoiler.</param>
		/// <param name="raw">True if the spoiler is a raw spoiler by default.</param>
		public async Task<RuntimeResult> RunSpoilerAsync(DiscordBotCommandContext context, string content, bool waitForAttachment, bool raw) {
			// Immediately remove the user's message from view
			await context.Message.DeleteAsync().ConfigureAwait(false);

			try {
				Spoiler spoiler = await ParseSpoilerAsync(context, content, waitForAttachment, raw).ConfigureAwait(false);
				if (spoiler == null) {
					var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
					await dm.SendMessageAsync("**Failed Spoiler:**").ConfigureAwait(false);
					await dm.SendMessageAsync(context.Message.Content).ConfigureAwait(false);
					return DeletedResult.FromSuccess();
				}
				if (waitForAttachment) {
					var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
					SpoilerAttachmentWaitContext wait = new SpoilerAttachmentWaitContext(context, spoiler) {
						OutputChannel = dm,
					};
					if (await wait.StartAsync().ConfigureAwait(false)) {
						//wait.AttachmentMessage = await dm.SendMessageAsync($"**{wait.Name}:** Send your spoiler attachment here, or type `cancel` to stop").ConfigureAwait(false);

						//wait.Ended += OnAttachmentWaitEnded;
						wait.MessageReceived += OnAttachmentWaitMessageReceivedAsync;
					}
				}
				else {
					await PostSpoilerAsync(spoiler).ConfigureAwait(false);
				}
				return DeletedResult.FromSuccess();
			} catch (Exception ex) {
				var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
				await dm.SendMessageAsync("**Failed Spoiler:**").ConfigureAwait(false);
				await dm.SendMessageAsync(context.Message.Content).ConfigureAwait(false);
				return DeletedResult.FromError(ex);
			}
		}

		/// <summary>
		/// Parses the spoiler from the command. Returns null if the parse failed.
		/// </summary>
		/// <param name="context">The command context of the spoiler.</param>
		/// <param name="text">The text of the spoiler.</param>
		/// <param name="waitForAttachment">True if the spoiler is waiting for an upload.</param>
		/// <param name="raw">True if the spoiler is raw by default.</param>
		/// <returns>The created spoiler.</returns>
		private async Task<Spoiler> ParseSpoilerAsync(DiscordBotCommandContext context, string text, bool waitForAttachment = false, bool raw = false) {
			/*ISocketMessageChannel outChannel = context.Message.MentionedChannels.FirstOrDefault() as ISocketMessageChannel;

			if (outChannel != null) {
				string channelMention = $"#{outChannel.Name}";
				if (text.StartsWith(channelMention)) {
					text = text.Substring(channelMention.Length).TrimStart();
				}
			}*/

			string imageUrl = null;
			string title = string.Empty;
			List<CodeBlock> blocks = text.GetAllCodeBlocks();
			int left = text.IndexOfUnescaped(blocks, '{');
			int right = -1;
			if (left != -1) {
				right = text.IndexOfUnescaped(blocks, '}', left + 1);
				if (left < right) { // Implicit rightIndex != -1
					title = text.Substring(left + 1, right - 1 - left).Trim();
					if (string.IsNullOrWhiteSpace(title))
						title = string.Empty;
				}
			}
			text = text.Substring(right + 1).Trim();
			bool singleImage = false;
			List<StringUrl> urls = (!raw ? text.GetUrls() : new List<StringUrl>());
			if (!raw && !waitForAttachment) {
				if (urls.Count == 1) {
					StringUrl url = urls[0];
					if (url.BaseUrl.IsImageUrl() && url.Start == 0 && url.Length == text.Length) {
						imageUrl = urls[0].BaseUrl;
						text = string.Empty;
						singleImage = true;
						// Don't add these urls to the spoiler url list.
						urls.Clear();
					}
				}
			}
			if (!raw && !singleImage && urls.Count > 0) {
				for (int i = urls.Count - 1; i >= 0; i--) {
					StringUrl url = urls[i];
					//text = text.Remove(url.Start, url.Length).Insert(url.Start, $"[{(i + 1)}]");
					text = text.Remove(url.Start, url.Length).Insert(url.Start, $"[[{(i + 1)}]]({url.BaseUrl})");
				}
			}
			string spoilerContent = text;

			// Womp womp
			string label = FormatTitle(title, waitForAttachment, true);
			if (label.Length > 256) {
				var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
				await dm.SendMessageAsync($"Formatted title is `{label.Length}` characters. The max is `256`").ConfigureAwait(false);
				return null;
			}
			if (!raw && spoilerContent.Length > 1024) {
				raw = true;
				imageUrl = null;
				urls.Clear();
				//await context.Channel.SendMessageAsync($"Formatted content is `{spoilerContent.Length}` characters. The max is `1024`. Use `spoiler raw` instead").ConfigureAwait(false);
				//return null;
			}
			if (!waitForAttachment && string.IsNullOrWhiteSpace(spoilerContent)) {
				var dm = await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
				await dm.SendMessageAsync($"Spoiler has no content!").ConfigureAwait(false);
				return null;
			}

			Spoiler spoiler = new Spoiler {
				AuthorId = context.User.Id,
				ChannelId = context.Channel.Id,
				GuildId = context.Guild?.Id ?? 0,

				Command = context.Message.Content,
				Title = title,
				Content = spoilerContent,
				AttachmentUrl = imageUrl,
				RawDisplay = raw,

				Context = context,
			};
			spoiler.Urls.AddRange(urls.Select(url => url.Url));
			return spoiler;
		}
		/// <summary>
		/// Deactivates a "dead" spoiler for when the associated End User Data is removed.
		/// </summary>
		/// <param name="spoiler">The spoiler to deactivate.</param>
		private async Task DeactivateSpoilerAsync(Spoiler spoiler) {
			try {
				if (Client.GetChannel(spoiler.ChannelId) is ITextChannel channel &&
					await channel.GetMessageAsync(spoiler.MessageId).ConfigureAwait(false) is IUserMessage message) {
					await message.ModifyAsync(m => {
						m.Embed = BuildEmbed(spoiler, message.Embeds.First(), true);
					}).ConfigureAwait(false);
					await message.RemoveReactionAsync(SpoilerReactions.ViewSpoiler, Client.CurrentUser).ConfigureAwait(false);
				}
			} catch { }
		}

		#endregion
		
		#region Public Edit Spoiler

		/// <summary>
		/// Deletes the spoiler with the specified Id. Optionally DMs the spoiler command to the user.
		/// </summary>
		/// <param name="id">The Id of the spoiler.</param>
		/// <param name="dmCommand">True if the executed command should be DMed to the user.</param>
		public async Task<bool> DeleteSpoilerAsync(ulong id, bool dmCommand = false) {
			using (var db = GetDb<SpoilerDbContext>()) {
				Spoiler spoiler = await FindSpoilerAsync(db, id).ConfigureAwait(false);
				if (spoiler == null)
					return false;

				try {
					if (Client.GetChannel(spoiler.ChannelId) is ITextChannel channel) {
						IMessage message = await channel.GetMessageAsync(spoiler.MessageId).ConfigureAwait(false);
						if (message != null)
							await message.DeleteAsync().ConfigureAwait(false);
					}
				} catch { }

				if (dmCommand) {
					var dm = await Client.GetUser(spoiler.AuthorId).GetOrCreateDMChannelAsync().ConfigureAwait(false);
					await dm.SendMessageAsync($"**Spoiler: {Format.Sanitize(spoiler.Title)} Deleted!**" +
						$"Here is the command text if you are correcting a mistake.").ConfigureAwait(false);
					await dm.SendMessageAsync(spoiler.Command).ConfigureAwait(false);
				}

				await RemoveSpoilerAsync(db, spoiler).ConfigureAwait(false);
				return true;
			}
		}

		/// <summary>
		/// Renames the spoiler's title.
		/// </summary>
		/// <param name="db">The database to work with.</param>
		/// <param name="spoiler">The spoiler to rename.</param>
		/// <param name="newTitle">The new title.</param>
		public async Task RenameSpoilerAsync(SpoilerDbContext db, Spoiler spoiler, string newTitle) {
			try {
				if (Client.GetChannel(spoiler.ChannelId) is ITextChannel channel &&
					await channel.GetMessageAsync(spoiler.MessageId).ConfigureAwait(false) is IUserMessage message)
				{
					spoiler.Title = newTitle;
					await message.ModifyAsync(m => {
						m.Embed = BuildEmbed(spoiler, message.Embeds.First());
					}).ConfigureAwait(false);
					await db.SaveChangesAsync().ConfigureAwait(false);
				}
			} catch { }
		}

		#endregion

		#region Public Database Access
		
		/// <summary>
		/// Finds the spoiler with the specified message Id.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <returns>The found spoiler, or null.</returns>
		public async Task<Spoiler> FindSpoilerAsync(ulong messageId) {
			using (var db = GetDb<SpoilerDbContext>())
				return await FindSpoilerAsync(db, messageId).ConfigureAwait(false);
		}
		/// <summary>
		/// Finds the spoiler with the specified message Id.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <returns>The found spoiler, or null.</returns>
		public Task<Spoiler> FindSpoilerAsync(SpoilerDbContext db, ulong messageId) {
			return db.Spoilers.FindAsync(messageId);
		}
		/// <summary>
		/// Finds the spoiled user with the specified message and user Id.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="userId">The user Id of the spoiled user.</param>
		/// <returns>The found spoiled user, or null.</returns>
		public async Task<SpoiledUser> FindSpoiledUserAsync(ulong messageId, ulong userId) {
			using (var db = GetDb<SpoilerDbContext>())
				return await db.SpoiledUsers.FindAsync(messageId, userId).ConfigureAwait(false);
		}
		/// <summary>
		/// Finds the spoiled user with the specified message and user Id.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="userId">The user Id of the spoiled user.</param>
		/// <returns>The found spoiled user, or null.</returns>
		public Task<SpoiledUser> FindSpoiledUserAsync(SpoilerDbContext db, ulong messageId, ulong userId) {
			return db.SpoiledUsers.FindAsync(messageId, userId);
		}
		/// <summary>
		/// Gets the last spoiler written by the specified author in the specified channel.
		/// </summary>
		/// <param name="authorId">The Id of the author who posted the spoiler.</param>
		/// <param name="channelId">The channel the spoiler was posted in.</param>
		public async Task<Spoiler> FindLastSpoilerByUserAsync(ulong authorId, ulong channelId) {
			using (var db = GetDb<SpoilerDbContext>())
				return await FindLastSpoilerByUserAsync(db, authorId, channelId).ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the last spoiler written by the specified author in the specified channel.
		/// </summary>
		/// <param name="db">The database to look in.</param>
		/// <param name="authorId">The Id of the author who posted the spoiler.</param>
		/// <param name="channelId">The channel the spoiler was posted in.</param>
		public async Task<Spoiler> FindLastSpoilerByUserAsync(SpoilerDbContext db, ulong authorId, ulong channelId) {
			SpoilerUser user = db.SpoilerUsers.Find(authorId);
			if (user == null)
				return null;
			await db.Entry(user).Collection(e => e.Spoilers).LoadAsync().ConfigureAwait(false);
			if (!user.Spoilers.Any())
				return null;

			Spoiler lastSpoiler = null;
			foreach (Spoiler spoiler in user.Spoilers) {
				if (spoiler.ChannelId == channelId && (lastSpoiler == null ||
					spoiler.TimeStamp > lastSpoiler.TimeStamp))
					lastSpoiler = spoiler;
			}
			return lastSpoiler;
		}
		/// <summary>
		/// Gets the count of the number of active spoilers.
		/// </summary>
		/// <returns>The number of active spoilers.</returns>
		public async Task<long> GetSpoilerCountAsync() {
			using (var db = GetDb<SpoilerDbContext>())
				return await db.Spoilers.LongCountAsync().ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the count of the number of spoiled users.
		/// </summary>
		/// <returns>The number of spoiled users.</returns>
		public async Task<long> GetSpoiledUserCountAsync() {
			using (var db = GetDb<SpoilerDbContext>())
				return await db.SpoiledUsers.LongCountAsync().ConfigureAwait(false);
		}

		#endregion

		#region Private Database

		/// <summary>
		/// Adds the parsed spoiler to the database.
		/// </summary>
		/// <param name="messageId">The new message Id of the spoiler.</param>
		/// <param name="spoiler">The spoiler to add.</param>
		private async Task AddSpoilerAsync(ulong messageId, Spoiler spoiler) {
			using (var db = GetDb<SpoilerDbContext>()) {
				spoiler.MessageId = messageId;
				spoiler.TimeStamp = DateTime.UtcNow;
				await db.EnsureEndUserData(spoiler).ConfigureAwait(false);
				await db.Spoilers.AddAsync(spoiler).ConfigureAwait(false);
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Removes the spoiler from the database.
		/// </summary>
		/// <param name="id">The Id of the spoiler.</param>
		private async Task<bool> RemoveSpoilerAsync(ulong id) {
			using (var db = GetDb<SpoilerDbContext>()) {
				Spoiler spoiler = db.Spoilers.Find(id);
				if (spoiler == null)
					return false;
				await RemoveSpoilerAsync(db, spoiler).ConfigureAwait(false);
				return true;
			}
		}
		/// <summary>
		/// Removes the spoiler from the database.
		/// </summary>
		/// <param name="db">The database to remove the spoiler from.</param>
		/// <param name="spoiler">The spoiler to remove.</param>
		private async Task RemoveSpoilerAsync(SpoilerDbContext db, Spoiler spoiler) {
			await db.Entry(spoiler).Collection(s => s.SpoiledUsers).LoadAsync().ConfigureAwait(false);
			spoiler.SpoiledUsers.Clear();
			//db.SpoiledUsers.RemoveRange(spoiler.SpoiledUsers);

			db.Spoilers.Remove(spoiler);
			await db.SaveChangesAsync().ConfigureAwait(false);
		}
		
		#endregion
		
		#region Private Format

		/// <summary>
		/// Formats the spoiler title with no message after the title part.
		/// </summary>
		/// <param name="spoiler">The spoiler with the title to format.</param>
		/// <returns>The formatted title string.</returns>
		private string FormatTitle(Spoiler spoiler) {
			bool withAttachment = (spoiler.AttachmentUrl != null && !spoiler.AttachmentUrl.IsImageUrl());
			return FormatTitle(spoiler.Title, withAttachment, null);
		}
		/// <summary>
		/// Formats the spoiler title.
		/// </summary>
		/// <param name="spoiler">The spoiler with the title to format.</param>
		/// <param name="deactivated">
		/// True if the spoiler is deactivated. Null if there is no message after the title.
		/// </param>
		/// <returns>The formatted title string.</returns>
		private string FormatTitle(Spoiler spoiler, bool? deactivated) {
			bool withAttachment = (spoiler.AttachmentUrl != null && !spoiler.AttachmentUrl.IsImageUrl());
			return FormatTitle(spoiler.Title, withAttachment, deactivated);
		}
		/// <summary>
		/// Formats the spoiler title.
		/// </summary>
		/// <param name="title">The title to use.</param>
		/// <param name="withAttachment">True if the spoiler has an attachment.</param>
		/// <param name="deactivated">
		/// True if the spoiler is deactivated. Null if there is no message after the title.
		/// </param>
		/// <returns>The formatted title string.</returns>
		private string FormatTitle(string title, bool withAttachment, bool? deactivated) {
			StringBuilder str = new StringBuilder();
			str.Append("Spoiler");
			if (withAttachment)
				str.Append(" with Attachment");

			if (!string.IsNullOrWhiteSpace(title))
				str.Append($": {Format.Sanitize(title)}");

			if (deactivated.HasValue) {
				str.Append(" | ");

				if (deactivated.Value)
					str.Append("`Deactivated` 🗑️");
				else
					str.Append("React to hear");
			}
			return str.ToString();
		}

		/// <summary>
		/// Formats the message to post the Urls in.
		/// </summary>
		/// <param name="urls">The collection of urls to post.</param>
		/// <returns>The formatted Urls string.</returns>
		private string FormatUrlsMessage(IEnumerable<string> urls) {
			var numberedUrls = urls.Select((s, i) => $"`[{(i + 1)}]`: {s}");
			return string.Join("\n", numberedUrls);
		}

		/// <summary>
		/// Builds the embed for the posted public spoiler.
		/// </summary>
		/// <param name="spoiler">The spoiler to build the embed for.</param>
		/// <param name="oldEmbed">The old embed if the spoiler is being edited.</param>
		/// <param name="deactivated">True if the spoiler is deactivated.</param>
		/// <returns>The built embed.</returns>
		private Embed BuildEmbed(Spoiler spoiler, IEmbed oldEmbed = null, bool deactivated = false) {
			/*StringBuilder text = new StringBuilder();
			text.Append("Spoiler");
			if (spoiler.AttachmentUrl != null && !spoiler.AttachmentUrl.IsImageUrl())
				text.Append(" with Attachment");

			if (!string.IsNullOrWhiteSpace(spoiler.Title)) {
				text.Append($": {Format.Sanitize(spoiler.Title)}");
			}
			text.Append(" | ");

			if (deactivated)
				text.Append("`Deactivated` 🗑️");
			else
				text.Append("React to hear");*/

			EmbedBuilder embed = new EmbedBuilder();
			embed.WithColor(embedColor);
			if (oldEmbed != null) {
				EmbedAuthor oldAuthor = oldEmbed.Author.Value;
				embed.WithAuthor(oldAuthor.Name, oldAuthor.ProxyIconUrl, oldAuthor.Url);
			}
			else if (spoiler.Context != null) {
				embed.WithAuthorUsername(spoiler.Context.User);
			}
			embed.WithTitle(FormatTitle(spoiler, deactivated));
			return embed.Build();
		}

		#endregion

		#region Private Post Spoiler

		/// <summary>
		/// Posts the spoiler to the channel the command was executed in.
		/// </summary>
		/// <param name="spoiler">The spoiler to post.</param>
		private async Task PostSpoilerAsync(Spoiler spoiler) {
			ISocketMessageChannel channel = spoiler.Context.Channel as ISocketMessageChannel;
			var message = await channel.SendMessageAsync(embed: BuildEmbed(spoiler)).ConfigureAwait(false);
			await message.AddReactionAsync(SpoilerReactions.ViewSpoiler).ConfigureAwait(false);
			await AddSpoilerAsync(message.Id, spoiler).ConfigureAwait(false);
		}
		/// <summary>
		/// Posts the content of the spoiler to the specified user.
		/// </summary>
		/// <param name="user">The user to post to.</param>
		/// <param name="spoiler">The spoiler to post.</param>
		/// <param name="spoiledUser">The spoiled user being kept track of.</param>
		private async Task PostSpoilerContentAsync(IUser user, Spoiler spoiler, SpoiledUser spoiledUser) {
			string label = FormatTitle(spoiler);
			/*string label = "Spoiler";
			if (!string.IsNullOrEmpty(spoiler.Title))
				label += $": {Format.Sanitize(spoiler.Title)}";*/
			IUser author = null;
			try {
				if (spoiler.AuthorId != 0)
					author = Client.GetUser(spoiler.AuthorId);
			} catch { }
			
			EmbedBuilder embed = new EmbedBuilder();
			if (author != null)
				embed.WithAuthorUsername(author);
			embed.WithColor(embedColor);
			embed.WithTimestamp(spoiler.TimeStamp.ToLocalTime());
			string content = spoiler.Content;
			/*List<StringUrl> urls = content.GetUrls();
			if (urls.Count > 0) {
				if (urls.Count == 1 &&)
			}*/
			if (spoiler.RawDisplay || string.IsNullOrWhiteSpace(spoiler.Content))
				embed.WithTitle(label);
			else
				embed.AddField(label, content);
			if (spoiler.AttachmentUrl != null && !spoiler.RawDisplay) {
				if (spoiler.AttachmentUrl.IsImageUrl())
					embed.WithImageUrl(spoiler.AttachmentUrl);
				else
					embed.AddField("Attachment:", spoiler.AttachmentUrl);
			}
			spoiledUser.DMMessageIdA = (await user.SendMessageAsync("", false, embed.Build()).ConfigureAwait(false)).Id;

			if (spoiler.RawDisplay) {
				/*if (author != null)
					label = $"**{Format.Sanitize(author.Username)}'s {label}** *({spoiler.TimeStamp:dd MMMM yyyy HH:mm:ss})*";
				spoiledUser.DMMessageIdA = (await user.SendMessageAsync(label).ConfigureAwait(false)).Id;*/
				if (!string.IsNullOrWhiteSpace(spoiler.Content))
					spoiledUser.DMMessageIdB = (await user.SendMessageAsync(spoiler.Content).ConfigureAwait(false)).Id;
				if (spoiler.AttachmentUrl != null)
					spoiledUser.DMMessageIdC = (await user.SendMessageAsync(spoiler.AttachmentUrl).ConfigureAwait(false)).Id;

				return;
			}
			else {
				spoiledUser.DMMessageIdB = 0;
				if (spoiler.Urls.Count > 0)
					spoiledUser.DMMessageIdB = (await user.SendMessageAsync(FormatUrlsMessage(spoiler.Urls)).ConfigureAwait(false)).Id;
			}
		}

		#endregion
	}
}
