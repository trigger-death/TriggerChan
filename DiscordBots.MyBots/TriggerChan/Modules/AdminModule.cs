using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.Utils;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Reactions;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Admin")]
	[Summary("Administrative control commands")]
	[RequiresUserPermission(GuildPermission.Administrator, Group = "manager")]
	[RequiresManager(Group = "manager")]
	[RequiresContext(ContextType.Guild)]
	[AllowBots(false)]
	[IsLockable(false)]
	public class AdminModule : TriggerModule {

		//private const int ColorSize = 32;

		#region Fields
			
		private readonly HelpService help;
		private readonly ConfigParserService configParser;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="AdminModule"/>.
		/// </summary>
		public AdminModule(TriggerServiceContainer services,
						   ConfigParserService configParser,
						   HelpService help)
			: base(services)
		{
			this.help = help;
			this.configParser = configParser;
		}

		#endregion

		[Group("prefix")]
		[ModulePriority(1)]
		[Usage("[newPrefix|reset]")]
		[Summary("Gets or sets the bot's command prefix")]
		public class PrefixGroup : TriggerModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="PrefixGroup"/>.
			/// </summary>
			public PrefixGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("prefix")]
			[Command("")]
			[Priority(1)]
			[Example("Displays the bot's command prefix")]
			public async Task GetPrefix() {
				string prefix = await Contexting.GetPrefixAsync(Context).ConfigureAwait(false);
				await ReplyAsync($"**Prefix:** `{Format.Sanitize(prefix)}`").ConfigureAwait(false);
			}

			[Name("prefix <newPrefix>")]
			[Command("")]
			[Priority(1)]
			[Example("s/", "Sets the bot's command prefix to `s/`")]
			public async Task SetPrefix(string prefix) {
				bool? result = await Contexting.SetPrefixAsync(Context, prefix).ConfigureAwait(false);
				if (result.HasValue) {
					await ReplyAsync($"**New Prefix:** `{Format.Sanitize(prefix)}`").ConfigureAwait(false);
				}
			}
			[Name("prefix reset")]
			[Command("reset")]
			[Priority(2)]
			[Example("Resets the bot's command prefix to the default prefix")]
			public async Task ResetPrefix() {
				bool? result = await Contexting.ResetPrefixAsync(Context).ConfigureAwait(false);
				if (result.HasValue) {
					await ReplyAsync($"**New Prefix:** `{Format.Sanitize(Contexting.DefaultPrefix)}`").ConfigureAwait(false);
				}
			}
		}

		[Name("adminsay <text>")]
		[Command("adminsay")]
		[Summary("Make the bot say something")]
		[Usage("<text...>")]
		[Example("Hello World!", "Makes the bot send a message with *Hello World!*")]
		[IsLockable(true, true)]
		public Task SayAdmin([Remainder] string text) => ReplyAsync(text);

		[Group("spellcheck")]
		[Usage("[on|off]")]
		[Summary("Gets or sets if spellcheck is enabled for the `spoiler` command")]
		[Remarks("If a match is found, the command will be deleted and the user will be notified that they failed to spell `spoiler` correctly")]
		public class SpellCheckGroup : TriggerModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="SpellCheckGroup"/>.
			/// </summary>
			public SpellCheckGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("spellcheck")]
			[Command("")]
			[Example("Gets if `spoiler` command spellcheck is enabled")]
			public async Task GetSpellCheck() {
				bool state = true;
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					if (guild != null)
						state = guild.SpellCheckSpoilers;
				}
				await ReplyAsync($"**Spellcheck:** `{(state ? "on" : "off")}`").ConfigureAwait(false);
			}

			[Name("spellcheck <on|off>")]
			[Command("")]
			[Example("off", "Turns `spoiler` command spellchecking off")]
			[RequiresContext(ContextType.Guild)]
			public async Task<RuntimeResult> SetSpellCheck(string onOff) {
				onOff = onOff.ToLower();
				if (onOff != "on" && onOff != "off")
					return EmoteResults.FromInvalidArgument();
				bool newState = (onOff.ToLower() == "on");
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.GetOrAddGuild(Context).ConfigureAwait(false);
					if (guild == null)
						return NormalResult.FromSuccess();
					guild.SpellCheckSpoilers = newState;
					await db.SaveChangesAsync().ConfigureAwait(false);
					await ReplyAsync($"**New Spellcheck:** `{(newState ? "on" : "off")}`").ConfigureAwait(false);
				}
				return NormalResult.FromSuccess();
			}
		}

		[Name("report <message>")]
		[Command("report")]
		[Summary("Leave a message in the logs for reports.")]
		[Usage("<message...>")]
		[Example("A detailed explanation of the error", "Write a Report of *A detailed explanation of the error* to the log file")]
		[Remarks("It is helpful if you include a screenshot or, instead, write an issue on the GitHub Repo (which can be found with the about command)\nAbuse of this command will result in this bot blacklisting your guild")]
		public async Task<RuntimeResult> ReportBug([Remainder] string message) {
			string source = $"{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator}";
			string origin = $"  Guild: {Context.Guild?.Id} {Format.Sanitize(Context.Guild.Name)}{Environment.NewLine}" +
							$"Channel: {Context.Channel.Id} {Format.Sanitize(Context.Channel.Name)}{Environment.NewLine}" +
							$"   User: {Context.User.Id} {source}";
			string logText = $"{Environment.NewLine}{origin}{Environment.NewLine}{message}";
			await Logging.LogAsync(new LogMessage(LogSeverities.Report, source, logText), logFile: true, noticeFile: true).ConfigureAwait(false);
			string channelStr = Home["channels:reports"];
			if (channelStr != null && ulong.TryParse(channelStr, out ulong channelId)) {
				if (Client.GetChannel(channelId) is IMessageChannel channel) {
					EmbedBuilder embed = new EmbedBuilder {
						//Title = "Bug Report",
						Color = configParser.EmbedColor,
					};
					embed.AddField("Report Origin", $"Guild: `{Context.Guild?.Id}`\n" +
													$"Channel: `{Context.Channel.Id}`\n" +
													$"User: `{Context.User.Id}`");
					embed.AddField("Report Message", message);
					embed.WithFooter($"{Context.Guild.Name} | {Context.Channel.Name}", Context.Guild.IconUrl);
					embed.WithTimestamp(Context.Message.CreatedAt);
					embed.WithAuthor(Context.User);
					await channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
				}
			}
			return EmoteResults.FromSuccess();
		}
		
		[Group("allowbots")]
		[Usage("[on|off]")]
		[Summary("Gets or sets if bots are allowed to use the spoiler command")]
		public class AllowBotsGroup :TriggerModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="AllowBotsGroup"/>.
			/// </summary>
			public AllowBotsGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("allowbots")]
			[Command("")]
			[Example("Gets if gets are allowed to use the `spoiler` command")]
			public async Task GetAllowBots() {
				bool state = false;
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					if (guild != null)
						state = guild.AllowBotSpoilers;
				}
				await ReplyAsync($"**Allow Bots:** `{(state ? "on" : "off")}`").ConfigureAwait(false);
			}

			[Name("allowbots <on|off>")]
			[Command("")]
			[Example("on", "Allows bots to use the `spoiler` command")]
			public async Task<RuntimeResult> SetAllowBots(string onOff) {
				onOff = onOff.ToLower();
				if (onOff != "on" && onOff != "off")
					return EmoteResults.FromInvalidArgument();
				bool newState = (onOff.ToLower() == "on");
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.GetOrAddGuild(Context).ConfigureAwait(false);
					if (guild == null)
						return NormalResult.FromSuccess();
					guild.AllowBotSpoilers = newState;
					await db.SaveChangesAsync().ConfigureAwait(false);
					await ReplyAsync($"**New Allow Bots:** `{(newState ? "on" : "off")}`").ConfigureAwait(false);
				}
				return NormalResult.FromSuccess();
			}
		}

		[IsLockable(false)]
		[Group("lock")]
		[Usage("lock <cmd|command|mod|module> <commandName|moduleName>")]
		[Summary("Locks commands or modules so that they cannot be used by anyone")]
		[Remarks("Locked commands and modules cannot be used or viewed by anyone other than admins. They can only be view with explicit *lock* variants of commands.")]
		public class LockGroup : TriggerModule {
			
			#region Constructors

			/// <summary>
			/// Constructs the <see cref="LockGroup"/>.
			/// </summary>
			public LockGroup(TriggerServiceContainer services) : base(services){ }

			#endregion

			[Name("lock command <commandName>")]
			[Command("command"), Alias("cmd")]
			[Example("ocarina", "Locks the `ocarina` command and prevents others from using it")]
			public async Task<RuntimeResult> LockCommand([Remainder] string commandName) {
				CommandDetails command = this.ResolveCommand(commandName);
				if (command == null)
					await ReplyAsync($"Command `{Format.Sanitize(commandName)}` does not exist").ConfigureAwait(false);
				bool? result = await Contexting.LockCommandAsync(Context, command).ConfigureAwait(false);
				if (result.HasValue) {
					return EmoteResults.FromLocked();
				}
				await ReplyAsync($"Command `{Format.Sanitize(commandName)}` cannot be locked in this context.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("lock module <moduleName>")]
			[Command("module"), Alias("mod")]
			[Example("fun", "Locks the `Fun` module and prevents others from using any of its commands")]
			public async Task<RuntimeResult> LockModule([Remainder] string moduleName) {
				ModuleDetails module = this.ResolveModule(moduleName);
				if (module == null)
					await ReplyAsync($"Module `{Format.Sanitize(moduleName)}` does not exist").ConfigureAwait(false);
				bool? result = await Contexting.LockModuleAsync(Context, module).ConfigureAwait(false);
				if (result.HasValue) {
					return EmoteResults.FromLocked();
				}
				await ReplyAsync($"Module `{Format.Sanitize(moduleName)}` cannot be locked in this context.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}

		[IsLockable(false)]
		[Group("unlock")]
		[Usage("unlock <cmd|command|mod|module> <commandName|moduleName>")]
		[Summary("Unlocks commands or modules so that they cann be used by others")]
		public class UnlockGroup : TriggerModule {
			
			#region Constructors

			/// <summary>
			/// Constructs the <see cref="UnlockGroup"/>.
			/// </summary>
			public UnlockGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("unlock command <commandName>")]
			[Command("command"), Alias("cmd")]
			[Example("command ocarina", "Locks the `ocarina` command and allows others to use it")]
			public async Task<RuntimeResult> UnlockCommand([Remainder] string commandName) {
				CommandDetails command = this.ResolveCommand(commandName);
				if (command == null)
					await ReplyAsync($"Command `{Format.Sanitize(commandName)}` does not exist").ConfigureAwait(false);
				bool? result = await Contexting.UnlockCommandAsync(Context, command).ConfigureAwait(false);
				if (result.HasValue) {
					return EmoteResults.FromUnlocked();
				}
				await ReplyAsync($"Command `{Format.Sanitize(commandName)}` cannot be unlocked in this context.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}

			[Name("unlock module <moduleName>")]
			[Command("module"), Alias("mod")]
			[Example("module fun", "Unlocks the `Fun` module and allows others to use its commands")]
			public async Task<RuntimeResult> UnlockModule([Remainder] string moduleName) {
				ModuleDetails module = this.ResolveModule(moduleName);
				if (module == null)
					await ReplyAsync($"Module `{Format.Sanitize(moduleName)}` does not exist").ConfigureAwait(false);
				bool? result = await Contexting.UnlockModuleAsync(Context, module).ConfigureAwait(false);
				if (result.HasValue) {
					return EmoteResults.FromUnlocked();
				}
				await ReplyAsync($"Module `{Format.Sanitize(moduleName)}` cannot be unlocked in this context.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
		}

		/*[Name("locked")]
		[Command("locked")]
		[Summary("List all locked commands and modules")]
		public async Task Locked() {
			await ReplyAsync(embed: await help.BuildLockedListAsync(Context, Commands.CommandSet).ConfigureAwait(false)).ConfigureAwait(false);
		}*/

		[Group("locked")]
		[Usage("[help <commandName|module <moduleName>>]")]
		[Summary("List or get information on locked commands and modules")]
		public class LockedGroup : TriggerModule {

			#region Fields

			private readonly HelpService help;

			#endregion

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="LockedGroup"/>.
			/// </summary>
			public LockedGroup(TriggerServiceContainer services,
							   HelpService help)
				: base(services)
			{
				this.help = help;
			}

			#endregion

			[Name("locked")]
			[Command("")]
			[Priority(0)]
			[Example("List all locked commands and modules")]
			public async Task Locked() {
				await ReplyAsync(embed: await help.BuildLockedListAsync(Context, Commands.CommandSet).ConfigureAwait(false)).ConfigureAwait(false);
			}

			[Name("locked help <commandName>")]
			[Command("help")]
			[Priority(0)]
			[Example("adminsay", "Explain the usage of the `adminsay` command whether it's locked or not")]
			public async Task LockedHelpSearchCommand([Remainder] string commandName) {
				commandName = string.Join(" ", commandName.Trim().Split(' '));
				CommandDetails command = Commands.CommandSet.FindCommand(commandName);
				if (command == null)
					await ReplyAsync($"No command with the name `{commandName}` exists").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildCommandHelpAsync(Context, command).ConfigureAwait(false)).ConfigureAwait(false);
			}

			[Name("locked help module <moduleName>")]
			[Command("help module")]
			[Priority(1)]
			[Example("fun", "Summarize the `Fun` command module whether it's locked or not")]
			public async Task LockedHelpSearchGroup([Remainder] string moduleName) {
				moduleName = string.Join(" ", moduleName.Trim().Split(' '));
				ModuleDetails module = Commands.CommandSet.FindModule(moduleName);
				if (module == null)
					await ReplyAsync($"No module with the name `{moduleName}` exists").ConfigureAwait(false);
				else
					await ReplyAsync(embed: await help.BuildModuleHelpAsync(Context, module, true).ConfigureAwait(false)).ConfigureAwait(false);
			}
		}

		[Name("lockable")]
		[Command("lockable")]
		[Summary("List all commands and modules that can be locked and unlocked")]
		public Task Lockable() {
			return ReplyAsync(embed: help.BuildLockableList(Context, Commands.CommandSet));
		}

		[IsLockable(false)]
		[Name("manager")]
		[Group("manager")]
		[Usage("[newRole|reset]")]
		[Summary("Gets or sets the role that gives users administrative access to this bot")]
		[RequiresUserPermission(GuildPermission.Administrator)] // This is here to prevent managers from using the role
		public class ManagerRoleGroup : TriggerModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="ManagerRoleGroup"/>.
			/// </summary>
			public ManagerRoleGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("manager")]
			[Command("")]
			[Example("Gets the current bot manager role")]
			public async Task GetManagerRole() {
				ulong roleId = await Contexting.GetManagerRoleIdAsync(Context).ConfigureAwait(false);
				if (roleId == 0)
					await ReplyAsync("**Manager Role:** No role is set").ConfigureAwait(false);
				else {
					try {
						IRole role = Context.Guild.GetRole(roleId);
						if (role != null)
							await ReplyAsync($"**Manager Role:** `{Format.Sanitize(role.Name)}`").ConfigureAwait(false);
					} catch { }
				}
			}

			[Name("manager <newRole>")]
			[Command("")]
			[Example("Moderator", "Gives bot manager access to all users with the *Moderator* role")]
			public async Task SetManagerRole([Remainder] IRole role) {
				bool? result = await Contexting.SetManagerRoleIdAsync(Context, role.Id).ConfigureAwait(false);
				if (result.HasValue) {
					await ReplyAsync($"**New Manager Role:** `{Format.Sanitize(role.Name)}`").ConfigureAwait(false);
				}
			}

			[Name("manager reset")]
			[Command("reset")]
			[Example("Resets and removes the manager role")]
			public async Task ResetManagerRole() {
				bool? result = await Contexting.ResetManagerRoleIdAsync(Context).ConfigureAwait(false);
				if (result.HasValue) {
					await ReplyAsync($"**New Manager Role:** No role is set").ConfigureAwait(false);
				}
			}
		}

		[Group("pinreact"), Alias("pinreacts")]
		[Usage("[newCount]")]
		[Summary("Gets or sets the number of 📌 reacts required to pin a message")]
		[Remarks("Pinreact allows users to decide if a message should be pinned without the need of a moderator.\n" +
			"If enough users react to a message with 📌, then I'll pin the message to the channel.")]
		[ModulePriority(1)]
		public class PinReactGroup : TriggerModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="PinReactGroup"/>.
			/// </summary>
			public PinReactGroup(TriggerServiceContainer services) : base(services) { }

			#endregion

			[Name("pinreact")]
			[Command("")]
			[Example("Gets the number of pin reacts required to pin a message")]
			public async Task GetPinReact() {
				int count = 0;
				using (var db = GetDb<TriggerDbContext>())
					count = (await db.FindGuildAsync(Context).ConfigureAwait(false)).PinReactCount;
				if (count == 0) {
					await ReplyAsync("**Pin-React:** disabled").ConfigureAwait(false);
				}
				else {
					await ReplyAsync($"**Pin-React Requires:** {count} {TriggerReactions.PinMessage}(s)").ConfigureAwait(false);
				}
			}

			[Name("pinreact <newCount>")]
			[Command("")]
			[Example("8", "Sets the number of 📌 reacts to pin a message to 8. Use 0 to disable Pin-React")]
			[Example("0", "Turns off Pin-React")]
			public async Task<RuntimeResult> SetPinReact(int count) {
				if (count < 0) {
					return EmoteResults.FromInvalidArgument("Count cannot be less than zero");
				}
				else {
					using (var db = GetDb<TriggerDbContext>()) {
						Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
						guild.PinReactCount = count;
						db.ModifyOnly(guild, g => g.PinReactCount);
						await db.SaveChangesAsync().ConfigureAwait(false);
						if (count == 0) {
							await ReplyAsync("**New Pin-React:** disabled").ConfigureAwait(false);
						}
						else {
							await ReplyAsync($"**New Pin-React Requires:** {count} {TriggerReactions.PinMessage}(s)").ConfigureAwait(false);
						}
						return NormalResult.FromSuccess();
					}
				}
			}
		}


		[Group("talkback")]
		[Usage("[<on|off>|reset|cooldown [none|[[hh:]mm:]ss]]")]
		[Summary("Gets or sets information about the Talk/React-Back feature")]
		[Remarks("Talk/React-Back allows me to respond to messages that contain certain key phrases. I have a cooldown per channel to prevent spam")]
		[ModulePriority(1)]
		public class TalkBackGroup : TriggerModule {

			private readonly TalkBackService talkBack;

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="TalkBackGroup"/>.
			/// </summary>
			public TalkBackGroup(TriggerServiceContainer services,
								 TalkBackService talkBack)
				: base(services)
			{
				this.talkBack = talkBack;
			}

			#endregion

			[Name("talkback")]
			[Command("")]
			[Priority(0)]
			[Example("Gets the current talkback status and cooldown")]
			public async Task GetTalkBack() {
				bool enabled;
				//TimeSpan cooldown;
				var (talk, react) = await talkBack.GetRemainingCooldownsAsync(Context).ConfigureAwait(false);
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					enabled = guild.TalkBackEnabled;
					//cooldown = guild.TalkBackCooldown;
				}
				await ReplyAsync($"**Talkback:** `{(enabled ? "on" : "off")}`\n" +
								 $"**Talk Cooldown:** {(talk.HasValue ? FormatCooldown(talk.Value) : "*ready*")}\n" +
								 $"**React Cooldown:** {(react.HasValue ? FormatCooldown(react.Value) : "*ready*")}").ConfigureAwait(false);
			}

			[Name("talkback <on|off>")]
			[Command("")]
			[Priority(1)]
			[Example("off", "Turns off the Talk/React-back so I won't respond to key phrases anymore")]
			public async Task<RuntimeResult> SetTalkBackEnabled(string onOff) {
				onOff = onOff.ToLower();
				if (onOff != "on" && onOff != "off")
					return EmoteResults.FromInvalidArgument("Not `on` or `off`");
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					guild.TalkBackEnabled = (onOff == "on");
					db.ModifyOnly(guild, g => g.TalkBackEnabled);
					await db.SaveChangesAsync().ConfigureAwait(false);
					await ReplyAsync($"**New Talkback:** `{(guild.TalkBackEnabled ? "on" : "off")}`").ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
			}

			[Name("talkback reset")]
			[Command("reset")]
			[Priority(2)]
			[Example("Resets the current cooldowns so that I can talk back to users again")]
			public async Task<RuntimeResult> ResetTalkBackCooldowns() {
				await talkBack.ResetRemainingCooldownAsync(Context).ConfigureAwait(false);
				return EmoteResults.FromSuccess();
				/*bool enabled;
				TimeSpan cooldown;
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					enabled = guild.TalkBackEnabled;
					cooldown = guild.TalkBackCooldown;
				}
				await ReplyAsync($"**Talkback Cooldown:** {FormatCooldown(cooldown)}").ConfigureAwait(false);*/
			}

			[Name("talkback cooldown")]
			[Command("cooldown")]
			[Priority(2)]
			[Example("Gets my cooldown setting for how long I have to wait before responding to users again")]
			public async Task GetTalkBackCooldown() {
				//bool enabled;
				TimeSpan cooldown;
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					//enabled = guild.TalkBackEnabled;
					cooldown = guild.TalkBackCooldown;
				}
				await ReplyAsync($"**Talkback Cooldown:** {(cooldown != TimeSpan.Zero ? FormatCooldown(cooldown) : "none")}").ConfigureAwait(false);
			}

			[Name("talkback cooldown <hh:mm:ss|none>")]
			[Command("cooldown")]
			[Priority(3)]
			[Example("30:01", "Sets my talkback cooldown to 30 minutes and 1 second")]
			[Example("1:00:00", "Sets my talkback cooldown to 1 hour")]
			[Example("none", "Turns off my talkback cooldown, now I can shower you with love all day")]
			public async Task<RuntimeResult> SetTalkBackCooldown(string time) {
				TimeSpan cooldown;
				if (time == "none") {
					cooldown = TimeSpan.Zero;
				}
				else {
					int colonCount = time.Split(':').Length - 1;
					// Add hours and minutes if not present, so that the format is exact
					for (int i = colonCount; i < 2; i++) {
						time = $"00:{time}";
					}
					if (!TimeSpan.TryParseExact(time, @"h\:m\:s", null, out cooldown))
						return EmoteResults.FromInvalidArgument();
				}
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					guild.TalkBackCooldown = cooldown;
					db.ModifyOnly(guild, g => g.TalkBackCooldown);
					await db.SaveChangesAsync().ConfigureAwait(false);
					await ReplyAsync($"**New Talkback Cooldown:** {(cooldown != TimeSpan.Zero ? FormatCooldown(cooldown) : "none")}").ConfigureAwait(false);
					//return EmoteResults.FromSuccess();
					return NormalResult.FromSuccess();
				}
			}

			private string FormatCooldown(TimeSpan cooldown) {
				StringBuilder str = new StringBuilder();
				// Minimum cooldown of 1 second.
				if (cooldown.TotalSeconds < 1)
					cooldown = TimeSpan.FromSeconds(1);
				if (cooldown.Days > 0)
					str.Append($" {cooldown:%d} day{Plural(cooldown.Days)}");
				if (cooldown.Hours > 0)
					str.Append($" {cooldown:%h} hour{Plural(cooldown.Hours)}");
				if (cooldown.Minutes > 0)
					str.Append($" {cooldown:%m} min{Plural(cooldown.Minutes)}");
				if (cooldown.Seconds > 0)
					str.Append($" {cooldown:%s} sec{Plural(cooldown.Seconds)}");
				return str.ToString().TrimStart();
			}

			private string Plural(int count) {
				return (count != 1 ? "s" : "");
			}
		}
	}
}
