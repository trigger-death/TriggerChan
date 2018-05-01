using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Services {

	public class ParsedSpoiler {

		public ISocketMessageChannel InputChannel { get; set; }
		public ISocketMessageChannel OutputChannel { get; set; }
		public IUser User { get; set; }
		public IUserMessage Message { get; set; }

		public string Header { get; set; }
		public string Content { get; set; }

		public bool WaitingForAttachment { get; set; }
		public IAttachment Attachment { get; set; }

		public Embed BuildEmbed(bool timedOut = false) {
			StringBuilder text = new StringBuilder();
			text.Append("Spoiler");
			if (Attachment != null)
				text.Append(" with Attachment");
			text.Append(": ");

			if (!string.IsNullOrWhiteSpace(Header))
				text.Append(Header + " | ");
			
			if (timedOut)
				text.Append("Attachment timed out");
			else
				text.Append("React to hear");


			EmbedBuilder embed = new EmbedBuilder();
			if (timedOut)
				embed.WithColor(Color.Red);
			embed.WithAuthor(User);
			embed.WithTitle(text.ToString());
			return embed.Build();
		}

		public Timer ExpireTimer { get; set; }
	}

	public class SpoilerService : BotServiceBase {

		private enum SpoilState {
			Spoil,
			Unspoil,
		}

		private class SpoilTask : BaseRef {
			public Task Task { get; set; } = null;
			public SpoilState State { get; set; }
			public SpoilState DesiredState { get; set; }

			public bool IsRunning {
				get { return Task != null && !Task.IsCompleted; }
			}

		}

		private class SpoilTaskCollection {
			private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SpoilTask>> spoilers;

			public SpoilTaskCollection() {
				spoilers = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SpoilTask>>();
			}

			public SpoilTask GetOrCreateAndRef(ulong spoilerId, ulong userId) {
				ConcurrentDictionary<ulong, SpoilTask> tasks;
				if (!spoilers.TryGetValue(spoilerId, out tasks)) {
					tasks = new ConcurrentDictionary<ulong, SpoilTask>();
					if (!spoilers.TryAdd(spoilerId, tasks))
						tasks = spoilers[spoilerId];
				}
				SpoilTask task;
				lock (tasks) {
					if (!tasks.TryGetValue(userId, out task)) {
						SpoilTask newTask = new SpoilTask();
						do {
							if (tasks.TryAdd(userId, newTask)) {
								task = newTask;
								Console.WriteLine("Ref Created");
								break;
							}
						} while (!tasks.TryGetValue(userId, out task));
					}
					task.AddRef();
				}
				return task;
			}

			public bool DerefAndRemove(ulong spoilerId, ulong userId, SpoilTask task) {
				task.RemoveRef();
				if (!task.IsUnused)
					return false;
				ConcurrentDictionary<ulong, SpoilTask> tasks;
				if (!spoilers.TryGetValue(spoilerId, out tasks))
					return false;
				lock (tasks) {
					if (task.IsUnused) {
						if (!tasks.TryRemove(userId, out task))
							return false;
						else
							Console.WriteLine("Ref removed");
						if (tasks.Count == 0)
							if (spoilers.TryRemove(spoilerId, out _))
								Console.WriteLine("Spoiler tasks removed");
						return true;
					}
				}
				return false;
			}
		}

		private class WaitingSpoilerCollection {

			private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ParsedSpoiler>> channels;

			public WaitingSpoilerCollection() {
				channels = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ParsedSpoiler>>();
			}

			public bool Add(ParsedSpoiler spoiler) {
				ConcurrentDictionary<ulong, ParsedSpoiler> users;
				if (!channels.TryGetValue(spoiler.InputChannel.Id, out users)) {
					users = new ConcurrentDictionary<ulong, ParsedSpoiler>();
					if (!channels.TryAdd(spoiler.InputChannel.Id, users))
						users = channels[spoiler.InputChannel.Id];
				}
				ParsedSpoiler oldSpoiler;
				if (users.TryRemove(spoiler.User.Id, out oldSpoiler))
					oldSpoiler.ExpireTimer?.Dispose();
				if (users.TryAdd(spoiler.User.Id, spoiler)) {
					spoiler.ExpireTimer = new Timer(OnExpire, spoiler, TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(-1));
					return true;
				}
				return false;
			}

			private void OnExpire(object state) {
				ParsedSpoiler spoiler = (ParsedSpoiler) state;
				Remove(spoiler.InputChannel.Id, spoiler.User.Id);
				Expired?.Invoke(spoiler);
			}

			public ParsedSpoiler Get(ulong channelId, ulong userId) {
				ConcurrentDictionary<ulong, ParsedSpoiler> users;
				if (!channels.TryGetValue(channelId, out users))
					return null;
				ParsedSpoiler spoiler;
				users.TryGetValue(userId, out spoiler);
				return spoiler;
			}

			public ParsedSpoiler Remove(ulong channelId, ulong userId) {
				ConcurrentDictionary<ulong, ParsedSpoiler> users;
				if (!channels.TryGetValue(channelId, out users))
					return null;
				ParsedSpoiler spoiler;
				users.TryGetValue(userId, out spoiler);
				if (users.TryRemove(userId, out spoiler))
					spoiler.ExpireTimer?.Dispose();
				if (users.Count == 0)
					channels.TryRemove(userId, out _);
				return spoiler;
			}

			public event Action<ParsedSpoiler> Expired;
		}

		private class AttachmentSpoiler {

			public IUser User { get; set; }
			public ITextChannel Channel { get; set; }

			public TimeSpan TimeStamp { get; set; }
		}
		
		private DiscordSocketClient discord;

		private SpoilTaskCollection spoilerTasks;

		private WaitingSpoilerCollection waitingSpoilers;

		public SpoilerService(DiscordSocketClient discord) {
			this.discord = discord;
			//this.database = database;

			discord.ReactionAdded += OnReactionAdded;
			discord.ReactionRemoved += OnReactionRemoved;
			discord.MessageReceived += OnMessageReceived;
			discord.MessageDeleted += OnMessageDeleted;
			spoilerTasks = new SpoilTaskCollection();
			waitingSpoilers = new WaitingSpoilerCollection();
			waitingSpoilers.Expired += OnSpoilerExpired;
		}

		private async Task OnMessageReceived(SocketMessage s) {
			var msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
			if (msg == null)
				return;
			if (s.Author.IsBot)
				return;     // Ignore bots and self when checking commands

			// Only for handling attachment additions
			if (!s.Attachments.Any())
				return;
			
			ParsedSpoiler spoiler = waitingSpoilers.Remove(s.Channel.Id, s.Author.Id);
			if (spoiler != null) {
				// Immediately remove the user's message from view
				try {
					await msg.DeleteAsync();

					spoiler.Attachment = s.Attachments.First();
					spoiler.WaitingForAttachment = false;
					await WriteSpoiler(spoiler);
				}
				catch (HttpException ex) {
					Console.WriteLine(ex.ToString());
				}
			}
		}

		private async Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2) {
			using (var database = new BotDatabaseContext()) {
				// Cleanup deleted spoilers
				Spoiler spoiler = await GetSpoiler(database, arg1.Id);
				if (spoiler != null) {
					Console.WriteLine("SPOILED DELETED!");
					/*foreach (SpoiledUser spoiledUser in spoiler.SpoiledUsers) {
						database.SpoiledUsers.Remove(spoiledUser);
					}
					spoiler.SpoiledUsers.Clear();*/
					database.Spoilers.Remove(spoiler);
					await database.SaveChangesAsync();
					return;
				}
				/*SpoiledUser user = await GetSpoiledUserByUserMessage(arg1.Id);
				if (user != null && user.IsSpoiled) {
					Console.WriteLine("SPOILED USER MESSAGE DELETED!");
					user.IsSpoiled = false;
					database.SpoiledUsers.Update(user);
					await database.SaveChangesAsync();
					return;
				}*/
			}
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			//Console.WriteLine("OnReactionAdded START");
			IMessage message;
			if (arg3.Message.IsSpecified && arg3.Message.Value != null)
				message = arg3.Message.Value;
			else
				message = await arg2.GetMessageAsync(arg1.Id);
			var user = arg3.User.Value;
			var emote = arg3.Emote;

			// Ignore those filthy robots
			if (!user.IsBot && BotReactions.ViewSpoiler.Equals(emote)) {
				await SpoilUser(message.Id, user);
				//await message.RemoveReactionAsync(EmojiList.ViewSpoiler, user);
			}
			//Console.WriteLine("OnReactionAdded END");
		}

		private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			//Console.WriteLine("OnReactionRemoved START");
			IMessage message;
			if (arg3.Message.IsSpecified && arg3.Message.Value != null)
				message = arg3.Message.Value;
			else
				message = await arg2.GetMessageAsync(arg1.Id);
			var user = arg3.User.Value;
			var emote = arg3.Emote;

			// Ignore those filthy robots
			if (!user.IsBot && BotReactions.ViewSpoiler.Equals(emote)) {
				await UnspoilUser(message.Id, user);
				//await message.RemoveReactionAsync(EmojiList.ViewSpoiler, user);
			}
			//Console.WriteLine("OnReactionRemoved END");
		}

		public async Task<Spoiler> GetSpoiler(BotDatabaseContext database, ulong messageId, bool track = false) {
			if (track)
				return database.Spoilers.FirstOrDefault(
					s => s.MessageId == messageId);
			return await database.Spoilers.AsNoTracking().FirstOrDefaultAsync(
				s => s.MessageId == messageId);
		}

		public async void OnSpoilerExpired(ParsedSpoiler spoiler) {
			await spoiler.InputChannel.SendMessageAsync("", false, spoiler.BuildEmbed(true));
		}

		public async Task SpoilUser(ulong messageId, IUser user, bool force = false) {
			try {
				Spoiler spoiler;
				using (var database = new BotDatabaseContext())
					spoiler = await GetSpoiler(database, messageId, true);
				if (spoiler == null)
					return;
				SpoilTask task = spoilerTasks.GetOrCreateAndRef(messageId, user.Id);
				lock (task) {
					Console.WriteLine("SPOIL TASK LOCK");
					task.DesiredState = SpoilState.Spoil;
					if (!task.IsRunning || force) {
						task.Task = null;
						task.State = SpoilState.Spoil;
						Console.WriteLine("SPOIL TASK");
						task.Task = Task.Run(() => SpoilUserTask(spoiler, task, messageId, user))
							.ContinueWith(async (t) => {
								bool revert;
								lock (task) {
									revert = task.DesiredState == SpoilState.Unspoil;
									// Remove the task
									if (!revert)
										task.Task = null;
								}
								if (revert)
									await UnspoilUser(messageId, user, true);
								spoilerTasks.DerefAndRemove(messageId, user.Id, task);
							});
					}
					else {
						spoilerTasks.DerefAndRemove(messageId, user.Id, task);
					}
					Console.WriteLine("SPOIL TASK UNLOCK");
				}
			}
			catch (AggregateException) {
				Console.WriteLine();
			}
			catch (TaskCanceledException) { }
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}

		public async Task UnspoilUser(ulong messageId, IUser user, bool force = false) {
			try {
				Spoiler spoiler;
				using (var database = new BotDatabaseContext())
					spoiler = await GetSpoiler(database, messageId, true);
				if (spoiler == null)
					return;
				SpoilTask task = spoilerTasks.GetOrCreateAndRef(messageId, user.Id);
				lock (task) {
					Console.WriteLine("UNSPOIL TASK LOCK");

					task.DesiredState = SpoilState.Unspoil;
					if (!task.IsRunning || force) {
						task.Task = null;
						task.State = SpoilState.Unspoil;
						Console.WriteLine("UNSPOIL TASK");
						task.Task = Task.Run(() => UnspoilUserTask(spoiler, task, messageId, user))
							.ContinueWith(async (t) => {
								bool revert;
								lock (task) {
									revert = task.DesiredState == SpoilState.Spoil;
									// Remove the task
									if (!revert)
										task.Task = null;
								}
								if (revert)
									await SpoilUser(messageId, user, true);
								spoilerTasks.DerefAndRemove(messageId, user.Id, task);
							});
					}
					else {
						spoilerTasks.DerefAndRemove(messageId, user.Id, task);
					}
					Console.WriteLine("UNSPOIL TASK UNLOCK");
				}
			}
			catch (AggregateException) {
				Console.WriteLine();
			}
			catch (TaskCanceledException) { }
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}
		
		private async Task SpoilUserTask(Spoiler spoiler, SpoilTask task, ulong messageId, IUser user) {
			using (var database = new BotDatabaseContext()) {
				try {
					SpoiledUser spoiledUser = await GetSpoiledUser(database, messageId, user.Id, true);
					bool wasNull = spoiledUser == null;
					if (spoiledUser == null) {
						Console.WriteLine("FIRST SPOIL");
						//SpoiledUser last = await database.SpoiledUsers.AsNoTracking().LastOrDefaultAsync();
						//ulong dbId = (last != null ? last.DbId + 1 : 0);
						spoiledUser = new SpoiledUser() {
							//DbId = dbId,
							MessageId = messageId,
							UserId = user.Id,
							TimeStamp = spoiler.TimeStamp,
						};
						await database.SpoiledUsers.AddAsync(spoiledUser);
					}
					else {
						Console.WriteLine("RESPOIL");
					}
					IMessage message = null;
					if (spoiledUser.UserMessageId != 0) {
						var channel = await user.GetOrCreateDMChannelAsync();
						message = await channel.GetMessageAsync(spoiledUser.UserMessageId);
					}
					if (message == null) {
						if (spoiler.Url != null) {
							using (HttpClient client = new HttpClient())
							using (Stream stream = await client.GetStreamAsync(spoiler.Url)) {
								spoiledUser.UserMessageId = (await user.SendFileAsync(stream, spoiler.Filename, spoiler.Content)).Id;
							}
						}
						else {
							spoiledUser.UserMessageId = (await user.SendMessageAsync(spoiler.Content)).Id;
						}
						Console.WriteLine("SPOIL SUCCESS");
					}
					else {
						await message.DeleteAsync();
						Console.WriteLine("SPOIL FAILED");
					}
					if (!wasNull)
						database.SpoiledUsers.Update(spoiledUser);
					await database.SaveChangesAsync();
				}
				catch (TaskCanceledException) {
					Console.WriteLine("SPOIL CANCEL");
				}
				catch (TimeoutException) {
					Console.WriteLine("SPOIL TIMEOUT");
				}
				catch (HttpException) {
					Console.WriteLine("SPOIL HttpException");
				}
				catch (AggregateException) {
					Console.WriteLine("SPOIL AGGREGATE");
				}
				catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}
				Console.WriteLine("SPOIL TASK END");
			}
		}

		private async Task UnspoilUserTask(Spoiler spoiler, SpoilTask task, ulong messageId, IUser user) {
			using (var database = new BotDatabaseContext()) {
				try {
					SpoiledUser spoiledUser = await GetSpoiledUser(database, messageId, user.Id, true);
					if (spoiledUser != null) {
						Console.WriteLine("UNSPOIL");
						IMessage message = null;
						if (spoiledUser.UserMessageId != 0) {
							var channel = await user.GetOrCreateDMChannelAsync();
							message = await channel.GetMessageAsync(spoiledUser.UserMessageId);
						}
						if (message != null) {
							await message.DeleteAsync();
							Console.WriteLine("UNSPOIL SUCCESS");
							spoiledUser.UserMessageId = 0;
						}
						else {
							Console.WriteLine("UNSPOIL FAILED");
						}
						database.SpoiledUsers.Update(spoiledUser);
						await database.SaveChangesAsync();
					}
				}
				catch (TaskCanceledException) {
					Console.WriteLine("UNSPOIL CANCEL");
				}
				catch (TimeoutException) {
					Console.WriteLine("UNSPOIL TIMEOUT");
				}
				catch (HttpException) {
					Console.WriteLine("UNSPOIL HttpException");
				}
				catch (AggregateException) {
					Console.WriteLine("UNSPOIL AGGREGATE");
				}
				catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}
				Console.WriteLine("UNSPOIL TASK END");
			}
		}

		public async Task<SpoiledUser> GetSpoiledUser(BotDatabaseContext database, ulong messageId, ulong userId, bool track = false) {
			if (track) {
				return database.SpoiledUsers.FirstOrDefault(
					u => u.MessageId == messageId && u.UserId == userId);
			}
			return await database.SpoiledUsers.AsNoTracking().FirstOrDefaultAsync(
				u => u.MessageId == messageId && u.UserId == userId);
		}
		
		public async void AddSpoiler(ulong messageId, ParsedSpoiler spoiler) {
			using (var database = new BotDatabaseContext()) {
				await database.Spoilers.AddAsync(new Spoiler() {
					MessageId = messageId,
					Content = spoiler.Content,
					Filename = spoiler.Attachment?.Filename,
					Url = spoiler.Attachment?.Url,
					TimeStamp = DateTime.UtcNow,
				});
				await database.SaveChangesAsync();
			}
		}

		public async Task<ParsedSpoiler> ParseSpoiler(SocketCommandContext context, string text, bool waitForAttachment = false) {
			// Immediately remove the user's message from view
			await context.Message.DeleteAsync();

			ISocketMessageChannel outChannel = context.Message.MentionedChannels.FirstOrDefault() as ISocketMessageChannel;

			if (outChannel != null) {
				string channelMention = $"#{outChannel.Name}";
				if (text.StartsWith(channelMention)) {
					text = text.Substring(channelMention.Length).TrimStart();
				}
			}

			Attachment attach = context.Message.Attachments.FirstOrDefault();
			string label = "Spoiler";
			if (attach != null)
				label += " with Attachment";
			label += ": ";
			string header = "";
			List<CodeBlock> blocks = text.GetAllCodeBlocks();
			int left = text.IndexOfUnescaped(blocks, '{');
			int right = -1;
			if (left != -1) {
				right = text.IndexOfUnescaped(blocks, '}', left + 1);
				if (left < right) { // Implicit rightIndex != -1
					header = text.Substring(left + 1, right - 1 - left).Trim();
					if (!string.IsNullOrWhiteSpace(header))
						label += $"{header} | ";
				}
			}
			label += "React to hear";
			string spoilerContent = text.Substring(right + 1).TrimStart();

			ParsedSpoiler spoiler = new ParsedSpoiler() {
				InputChannel = context.Channel,
				OutputChannel = outChannel,
				User = context.User,
				Message = context.Message,
				WaitingForAttachment = waitForAttachment && (attach == null),
				Attachment = attach,
				Header = header,
				Content = spoilerContent,
			};

			return spoiler;



			/*EmbedBuilder embed = new EmbedBuilder();
			embed.WithAuthor(context.User);
			embed.WithTitle(label);
			var message = await ReplyAsync("", false, embed.Build());
			await message.AddReactionAsync(BotEmoji.ViewSpoiler);
			spoilers.AddSpoiler(message.Id, message.Channel.Id, spoilerContent, attach);*/
		}

		public async Task WriteSpoiler(ParsedSpoiler spoiler) {
			ISocketMessageChannel channel = spoiler.OutputChannel ?? spoiler.InputChannel;
			var message = await channel.SendMessageAsync("", false, spoiler.BuildEmbed());
			await message.AddReactionAsync(BotReactions.ViewSpoiler);
			AddSpoiler(message.Id, spoiler);
		}

		public void WaitForAttachment(ParsedSpoiler spoiler) {
			waitingSpoilers.Add(spoiler);
		}
	}
}
