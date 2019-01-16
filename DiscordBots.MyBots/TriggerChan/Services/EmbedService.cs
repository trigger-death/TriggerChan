using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using TriggersTools.DiscordBots.Commands;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class EmbedService : TriggerService {
		#region Enums

		/// <summary>
		/// How arguments are allowed for a specified token.
		/// </summary>
		private enum ArgsMode {
			/// <summary>The token is not allowed to have arguments.</summary>
			None,
			/// <summary>The token is allowed to have arguments.</summary>
			Allow,
			/// <summary>The token is required to have arguments.</summary>
			Require,
		}

		#endregion

		#region Structs

		private struct ReplaceTokenInfo {

			public ArgsMode ArgsMode { get; }
			public bool Sanitize { get; }
			public ReplaceTokenAsync Command { get; }

			public ReplaceTokenInfo(ArgsMode argsMode, bool sanitize, ReplaceTokenAsync command) {
				ArgsMode = argsMode;
				Sanitize = sanitize;
				Command = command;
			}

			public async Task<string> ReplaceAsync(IDiscordBotCommandContext context, string args, bool noSanitize) {
				string value = await Command(context, args).ConfigureAwait(false);
				if (Sanitize && !noSanitize)
					value = Format.Sanitize(value);
				// Escape for Json strings
				value = JsonConvert.ToString(value);
				return value.Substring(1, value.Length - 2);
			}
		}

		#endregion

		#region Delegates

		/// <summary>
		/// Synchronously replaces a single token with the specified returned string.
		/// </summary>
		/// <param name="context">The context for creating the embed used to access certain variables.</param>
		/// <param name="args">The optional arguments for the token. Null if there are no arguments.</param>
		private delegate string ReplaceToken(IDiscordBotCommandContext context, string args);
		/// <summary>
		/// Asynchronously replaces a single token with the specified returned string.
		/// </summary>
		/// <param name="context">The context for creating the embed used to access certain variables.</param>
		/// <param name="args">The optional arguments for the token. Null if there are no arguments.</param>
		private delegate Task<string> ReplaceTokenAsync(IDiscordBotCommandContext context, string args);

		#endregion

		#region Constants

		private const string TokenPattern = @"""(?:[^\\""]|\\.)*(?<!\\)(?'replace'\$(?'nosan'!)?(?'token'\w+)(?::(?'args'(?:[^\\""]|\\.)*))?\$)(?:[^\\""]|\\.)*""";
		private static readonly Regex TokenRegex = new Regex(TokenPattern);

		#endregion

		#region Fields

		/// <summary>
		/// The dictionary containing all tokens and the delegate used to replace them.
		/// </summary>
		private readonly Dictionary<string, ReplaceTokenInfo> tokens;

		#endregion

		#region Constructors

		public EmbedService(TriggerServiceContainer services) : base(services) {
			tokens = new Dictionary<string, ReplaceTokenInfo>();
			AddToken("TIMESTAMP",   ArgsMode.None,    false, ReplaceTimestamp);
			AddToken("DATE",        ArgsMode.Allow,   false, ReplaceDate);

			AddToken("SERVERNAME",  ArgsMode.None,    true,  ReplaceServerName);
			AddToken("SERVERICON",  ArgsMode.None,    false, ReplaceServerIcon);

			AddToken("CHANNEL",     ArgsMode.Allow,   false, ReplaceChannel);
			AddToken("CHANNELNAME", ArgsMode.Allow,   true,  ReplaceChannelNameAsync);

			AddToken("USER",        ArgsMode.Allow,   false, ReplaceUser);
			AddToken("USERNAME",    ArgsMode.Allow,   true,  ReplaceUsernameAsync);
			AddToken("NICKNAME",    ArgsMode.Allow,   true,  ReplaceNicknameAsync);
			AddToken("DISCRIM",     ArgsMode.Allow,   false, ReplaceDiscriminatorAsync);
			AddToken("AVATAR",      ArgsMode.Allow,   false, ReplaceAvatarAsync);

			AddToken("BOTUSER",     ArgsMode.None,    false, ReplaceBotUser);
			AddToken("BOTUSERNAME", ArgsMode.None,    true,  ReplaceBotUsername);
			AddToken("BOTNICKNAME", ArgsMode.None,    true,  ReplaceBotNickname);
			AddToken("BOTDISCRIM",  ArgsMode.None,    false, ReplaceBotDiscriminator);
			AddToken("BOTAVATAR",   ArgsMode.None,    false, ReplaceBotAvatar);

			AddToken("ROLE",        ArgsMode.Require, false, ReplaceRole);
		}

		private void AddToken(string token, ArgsMode argsMode, bool sanitize, ReplaceToken command) {
			tokens.Add(token, new ReplaceTokenInfo(argsMode, sanitize, (c, a) => Task.FromResult(command(c, a))));
		}
		private void AddToken(string token, ArgsMode argsMode, bool sanitize, ReplaceTokenAsync command) {
			tokens.Add(token, new ReplaceTokenInfo(argsMode, sanitize, command));
		}

		#endregion

		#region ReplaceTokens

		public async Task<string> ReplaceTokensAsync(IDiscordBotCommandContext context, string json) {
			StringBuilder str = new StringBuilder(json);
			var matches = TokenRegex.Matches(json);
			for (int i = matches.Count - 1; i >= 0; i--) {
				Match match = matches[i];
				string token = match.Groups["token"].Value;
				string args = (match.Groups["args"].Success ? match.Groups["args"].Value : null);
				if (!tokens.TryGetValue(token, out var tokenInfo))
					throw new ArgumentException($"Unknown token \"{token}\"");
				if (tokenInfo.ArgsMode == ArgsMode.None && args != null)
					throw new ArgumentException($"Token \"{token}\": Does not allow arguments!");
				if (tokenInfo.ArgsMode == ArgsMode.Require && args == null)
					throw new ArgumentException($"Token \"{token}\": Requires arguments!");
				if (args != null && args.Length == 0)
					throw new ArgumentException($"Token \"{token}\": Cannot have empty arguments!");
				try {
					if (args != null) {
						// Unescape json args
						StringBuilder argsStr = new StringBuilder(args);
						var escapeMatches = Regex.Matches(args, @"\(?'char'.)");
						for (int j = matches.Count - 1; j >= 0; j--) {
							Match escapeMatch = escapeMatches[j];
							argsStr.Remove(match.Index, match.Length);
							argsStr.Insert(match.Index, match.Groups["char"].Value);
						}
						args = argsStr.ToString();
					}
					bool noSanitize = match.Groups["nosan"].Success;
					string value = await tokenInfo.ReplaceAsync(context, args, noSanitize).ConfigureAwait(false);
					Group group = match.Groups["replace"];
					str.Remove(group.Index, group.Length);
					str.Insert(group.Index, value);
				} catch (Exception ex) {
					throw new ArgumentException($"Token \"{token}\": {ex.Message}");
				}
			}
			return str.ToString();
		}

		#endregion

		#region Token Commands

		private static string ReplaceTimestamp(IDiscordBotCommandContext context, string args) {
			return string.Concat(DateTime.UtcNow.ToString("s"), "Z");
		}
		private static string ReplaceDate(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
			return DateTime.UtcNow.ToString(args);
		}
		private static string ReplaceServerName(IDiscordBotCommandContext context, string args) {
			return context.Guild.Name;
		}
		private static string ReplaceServerIcon(IDiscordBotCommandContext context, string args) {
			return context.Guild.IconUrl;
		}
		private static string ReplaceChannel(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return $"<#{context.Channel.Id}>";
			if (ulong.TryParse(args, out ulong id))
				return $"<#{id}>";
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static async Task<string> ReplaceChannelNameAsync(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return context.Channel.Name;
			if (ulong.TryParse(args, out ulong id)) {
				try {
					return (await context.Guild.GetTextChannelAsync(id).ConfigureAwait(false)).Name;
				} catch {
					throw new ArgumentException($"No channel with the Id {id}!");
				}
			}
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static string ReplaceUser(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return $"<@{context.Channel.Id}>";
			if (ulong.TryParse(args, out ulong id))
				return $"<@{id}>";
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static async Task<string> ReplaceUsernameAsync(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return context.User.Username;
			if (ulong.TryParse(args, out ulong id)) {
				try {
					return (await context.Guild.GetUserAsync(id).ConfigureAwait(false)).Username;
				} catch {
					throw new ArgumentException($"No user with the Id {id}!");
				}
			}
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static async Task<string> ReplaceNicknameAsync(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return context.User.GetName(context.Guild, false);
			if (ulong.TryParse(args, out ulong id)) {
				try {
					IGuildUser user = await context.Guild.GetUserAsync(id).ConfigureAwait(false);
					return user.GetName(context.Guild, false);
				} catch {
					throw new ArgumentException($"No user with the Id {id}!");
				}
			}
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static async Task<string> ReplaceDiscriminatorAsync(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return context.User.Discriminator;
			if (ulong.TryParse(args, out ulong id)) {
				try {
					return (await context.Guild.GetUserAsync(id).ConfigureAwait(false)).Discriminator;
				} catch {
					throw new ArgumentException($"No user with the Id {id}!");
				}
			}
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private static async Task<string> ReplaceAvatarAsync(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return context.User.GetAvatarUrl();
			if (ulong.TryParse(args, out ulong id)) {
				try {
					IGuildUser user = await context.Guild.GetUserAsync(id).ConfigureAwait(false);
					return user.GetAvatarUrl();
				} catch {
					throw new ArgumentException($"No user with the Id {id}!");
				}
			}
			throw new ArgumentException("Argument is not a valid Id!");
		}
		private string ReplaceBotUser(IDiscordBotCommandContext context, string args) {
			return $"<@{Client.CurrentUser.Id}>";
		}
		private string ReplaceBotUsername(IDiscordBotCommandContext context, string args) {
			return Client.CurrentUser.Username;
		}
		private string ReplaceBotNickname(IDiscordBotCommandContext context, string args) {
			return Client.CurrentUser.GetName(context.Guild, true);
		}
		private string ReplaceBotDiscriminator(IDiscordBotCommandContext context, string args) {
			return Client.CurrentUser.Discriminator;
		}
		private string ReplaceBotAvatar(IDiscordBotCommandContext context, string args) {
			return Client.CurrentUser.GetAvatarUrl();
		}
		private static string ReplaceRole(IDiscordBotCommandContext context, string args) {
			if (args == null)
				return $"<@&{context.Channel.Id}>";
			if (ulong.TryParse(args, out ulong id))
				return $"<@&{id}>";
			throw new ArgumentException("Argument is not a valid Id!");
		}

		#endregion
	}
}
