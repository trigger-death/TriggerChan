using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.TriggerChan.Utils;
using TriggersTools.SteinsGate;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class TalkBackService : TriggerService {
		
		private readonly ConcurrentDictionary<ulong, ChannelTalkBackCooldowns> localGuildChannels = new ConcurrentDictionary<ulong, ChannelTalkBackCooldowns>();
		
		private ChannelTalkBackCooldowns GetChannelCooldowns(ulong id) {
			ChannelTalkBackCooldowns gChannel = new ChannelTalkBackCooldowns() { Id = id };
			return localGuildChannels.GetOrAdd(id, gChannel);
		}

		private readonly Random random;
		private readonly ConfigParserService configParser;
		private readonly Emote TriggyEmote = Emote.Parse("<:trigger_chan:506519254884024340>");

		public TalkBackService(TriggerServiceContainer services,
							   ConfigParserService configParser,
							   Random random)
			: base(services)
		{
			this.random = random;
			this.configParser = configParser;
		}
		private class ChannelTalkBackCooldowns {

			public ulong Id { get; set; }

			public Stopwatch TalkBackTimer { get; set; }
			public Stopwatch ReactBackTimer { get; set; }
		}

		public async Task<(TimeSpan? talk, TimeSpan? react)> GetRemainingCooldownsAsync(ICommandContext context) {
			TimeSpan cooldown;
			using (var db = GetDb<TriggerDbContext>())
				cooldown = (await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false)).TalkBackCooldown;
			ChannelTalkBackCooldowns channel = GetChannelCooldowns(context.Channel.Id);
			TimeSpan? talk = channel.TalkBackTimer?.Elapsed;
			TimeSpan? react = channel.ReactBackTimer?.Elapsed;
			if (talk.HasValue && (!channel.TalkBackTimer.IsRunning || talk.Value >= cooldown))
				talk = null;
			else
				talk = cooldown - talk;
			if (react.HasValue && (!channel.ReactBackTimer.IsRunning || react.Value >= cooldown))
				react = null;
			else
				react = cooldown - react;
			return (talk, react);
		}

		public Task ResetRemainingCooldownAsync(ICommandContext context) {
			ChannelTalkBackCooldowns channel = GetChannelCooldowns(context.Channel.Id);
			channel.TalkBackTimer = null;
			channel.ReactBackTimer = null;
			return Task.FromResult<object>(null);
		}


		private Regex[] ILoveRegex;
		private readonly Regex TriggyRegex = new Regex(@"\btriggy\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
		
		public override void Initialize() {
			Client.MessageReceived += OnMessageReceived;
			Client.ReactionAdded += OnReactionAdded;
			InitILoveTriggerChanRegex();
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
			if (!(channel is IGuildChannel guildChannel)) return;
			if (reaction.Emote.Equals(TriggerReactions.PinMessage)) {
				IUserMessage message = null;
				if (reaction.Message.IsSpecified && reaction.Message.Value != null)
					message = reaction.Message.Value;
				else
					message = await channel.GetMessageAsync(msg.Id).ConfigureAwait(false) as IUserMessage;
				if (!message.IsPinned) {
					int pinCount;
					using (var db = GetDb<TriggerDbContext>())
						pinCount = (await db.FindGuildAsync(guildChannel.GuildId).ConfigureAwait(false)).PinReactCount;
					//int pinCount = (await contexting.FindGuildAsync(guildChannel.GuildId, true).ConfigureAwait(false)).PinReactCount;
					if (pinCount <= 0)
						return;
					int newCount = message.Reactions[TriggerReactions.PinMessage].ReactionCount;
					if (newCount >= pinCount) {
						await message.PinAsync().ConfigureAwait(false);
					}
				}
			}
		}

		public async Task<int> GetPinReactCount(SocketCommandContext context) {
			using (var db = GetDb<TriggerDbContext>())
				return (await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false)).PinReactCount;
			//return (await contexting.FindGuildAsync(context.Guild.Id, true).ConfigureAwait(false)).PinReactCount;
		}

		public async Task SetPinReactCount(SocketCommandContext context, int count) {
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);

				if (guild.PinReactCount != count) {
					guild.PinReactCount = count;
					db.ModifyOnly(guild, g => g.PinReactCount);
					await db.SaveChangesAsync().ConfigureAwait(false);
				}
			}
		}

		public void ResetTalkbackCooldown(SocketCommandContext context) {
			ChannelTalkBackCooldowns channel = GetChannelCooldowns(context.Channel.Id);
			channel.TalkBackTimer.Reset();
			channel.ReactBackTimer.Reset();
		}

		public async Task<TimeSpan> GetTalkBackCooldown(SocketCommandContext context) {
			using (var db = GetDb<TriggerDbContext>())
				return (await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false)).TalkBackCooldown;
			//return (await contexting.FindGuildAsync(context.Guild.Id, true).ConfigureAwait(false)).TalkBackCooldown;
		}

		public async Task SetTalkBackCooldown(SocketCommandContext context, TimeSpan time) {
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);

				if (guild.TalkBackCooldown != time) {
					guild.TalkBackCooldown = time;
					db.ModifyOnly(guild, g => g.TalkBackCooldown);
					await db.SaveChangesAsync().ConfigureAwait(false);
				}
			}
		}

		public async Task<bool> GetTalkBack(SocketCommandContext context) {
			using (var db = GetDb<TriggerDbContext>())
				return (await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false)).TalkBackEnabled;
			//return (await contexting.FindGuildAsync(context.Guild.Id, true).ConfigureAwait(false)).TalkBackEnabled;
		}
		
		public async Task<bool> SetTalkBack(SocketCommandContext context, bool enabled) {
			using (var db = GetDb<TriggerDbContext>()) {
				Guild guild = await db.FindGuildAsync(context.Guild.Id).ConfigureAwait(false);

				if (guild.TalkBackEnabled != enabled) {
					guild.TalkBackEnabled = enabled;
					db.ModifyOnly(guild, g => g.TalkBackEnabled);
					await db.SaveChangesAsync().ConfigureAwait(false);
					return true;
				}
				return false;
			}
		}

		private void InitILoveTriggerChanRegex() {
			List<string> names = new List<string>();
			if (Config["username"] != null) {
				names.Add(Config["username"]);
				names.Add("@" + Config["username"]);
			}
			if (Config["client_id"] != null) {
				names.Add($"<@{Config["client_id"]}>");
			}
			const string W = @"(\s|^)+";
			names.AddRange(new string[] {
				$"triggerchan",
				$"trigger_chan",
				$"trigger-chan",
				$"trigger chan",
				$"triggychan",
				$"triggy_chan",
				$"triggy-chan",
				$"triggy chan",
				$"triggy"
			});

			string name = "(";
			for (int i = 0; i < names.Count; i++) {
				if (i != 0)
					name += "|";
				name += Regex.Escape(names[i]);
			}
			name += ")";

			const string I = @"i";
			const string Really = @"(re*a*l+y+)";
			const string Love = @"(l+o+v+e+s*|l+u+v+s*|.?❤.?|\<3+|le?i+e?k+e*s*|daisuki|daisuke|suki)";
			const string Daisuki = @"(daisuki|daisuke|suki|.?❤.?|\<3+)";
			const string Heart = @"(.?❤.?)";
			const string Dont = @"(?<!(do?n'?t|do?e?sn'?t|donn?ot|do not))";
			const string You = @"(yo+u+|u+)";
			const string Begin = @"(\s|^)";
			const string End = @"(\s|$)";

			string[] patterns = new string[] {
				$"{Dont}{W}{Love}{W}({You}{W})?{name}{End}",
				$"{Begin}{name}{W}{I}{W}({Really}{W})*{Love}{W}{You}{End}",
				$"{Dont}{W}{Heart}{W}{name}{End}",
				$"{Begin}{name}{W}{Heart}{End}",
				$"{Begin}{Daisuki}{W}{name}{End}",
				$"{Begin}{name}{W}{Daisuki}{End}",
				$"{Client.CurrentUser.Username}",
				$"{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}"
			};

			ILoveRegex = new Regex[patterns.Length];
			RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
			for (int i = 0; i < patterns.Length; i++) {
				Debug.WriteLine(patterns[i]);
				ILoveRegex[i] = new Regex(patterns[i], options);
			}
		}

		private async Task OnMessageReceived(SocketMessage s) {
			if (!(s is SocketUserMessage msg)) return; // Ensure the message is from a user/bot
			if (!(s.Channel is IGuildChannel)) return; // Only talkback in guilds
			if (msg.Author.IsBot) return;

			var context = new DiscordBotCommandContext(Services, Client, msg);
			
			if (await GetTalkBack(context).ConfigureAwait(false)) {
				// Don't reactback if we already responded with talkback
				if (!await ILoveTriggerChan(msg).ConfigureAwait(false))
					await ReactToTriggy(msg).ConfigureAwait(false);
			}
		}

		private async Task ReactToTriggy(SocketUserMessage msg) {
			if (!(msg.Channel is IGuildChannel guildChannel)) return;

			var context = new SocketCommandContext(Client, msg);
			string text = msg.Resolve(userHandling: TagHandling.FullNameNoPrefix);

			ChannelTalkBackCooldowns channel = GetChannelCooldowns(msg.Channel.Id);
			channel.ReactBackTimer = channel.ReactBackTimer ?? new Stopwatch();
			TimeSpan cooldown;
			using (var db = GetDb<TriggerDbContext>())
				cooldown = (await db.FindGuildAsync(guildChannel.GuildId).ConfigureAwait(false)).TalkBackCooldown;
			if (channel.ReactBackTimer.IsRunning && channel.ReactBackTimer.Elapsed < cooldown)
				return;
			// Make sure mentioning triggy is somewhat meaningful. Just stating the name is boring
			if (TriggyRegex.IsMatch(text) && text.Length >= 24) {
				await msg.AddReactionAsync(TriggyEmote).ConfigureAwait(false);
				channel.ReactBackTimer.Restart();
			}
		}


		private async Task<bool> ILoveTriggerChan(SocketUserMessage msg) {
			if (!(msg.Channel is IGuildChannel guildChannel)) return false;

			var context = new SocketCommandContext(Client, msg);

			ChannelTalkBackCooldowns channel = GetChannelCooldowns(msg.Channel.Id);
			channel.TalkBackTimer = channel.TalkBackTimer ?? new Stopwatch();
			TimeSpan cooldown;
			using (var db = GetDb<TriggerDbContext>())
				cooldown = (await db.FindGuildAsync(guildChannel.GuildId).ConfigureAwait(false)).TalkBackCooldown;

			if (channel.TalkBackTimer.IsRunning && channel.TalkBackTimer.Elapsed < cooldown)
				return false;

			// Message Length Cap?
			// Tradeoff: People can make long rants about their love without facing the consequences
			//if (msg.Content.Length > 500)
			//	return;


			string text = msg.Resolve(userHandling: TagHandling.FullNameNoPrefix)
				.Replace('.', ' ')
				.Replace(',', ' ')
				.Replace(';', ' ')
				.Replace('!', ' ')
				.Replace('?', ' ')
				.Replace('-', ' ')
				.Replace('_', ' ')
				.Replace('\n', ' ')
				.Replace('\r', ' ');

			//foreach (char c in text)
			//	Debug.Write(((int) c) + "|");
			//Debug.WriteLine("");
			//Debug.WriteLine(text);

			int index = 0;
			foreach (Regex regex in ILoveRegex) {
				if (regex.IsMatch(text)) {
					// Rare talkbacks
					if (Random.Next(20) == 0)
						await msg.Channel.SendFileAsync(random.Choose(TriggerResources.TalkbackWellIDontLoveYouRare)).ConfigureAwait(false);
					else
						await msg.Channel.SendFileAsync(random.Choose(TriggerResources.TalkbackWellIDontLoveYou)).ConfigureAwait(false);
					channel.TalkBackTimer.Restart();
					return true;
				}
				index++;
			}
			return false;
		}
	}
}
