using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.Utils;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.SpoilerBot.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Services;

namespace TriggersTools.DiscordBots.SpoilerBot.Modules {
	[Name("Admin")]
	[Summary("Administrative control commands")]
	[RequiresUserPermission(GuildPermission.Administrator, Group = "manager")]
	[RequiresManager(Group = "manager")]
	[RequiresContext(ContextType.Guild)]
	[AllowBots(false)]
	[IsLockable(false)]
	public class AdminModule : DiscordBotModule {

		//private const int ColorSize = 32;

		#region Fields
			
		private readonly HelpService help;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="AdminModule"/>.
		/// </summary>
		public AdminModule(DiscordBotServiceContainer services,
						   HelpService help)
			: base(services)
		{
			this.help = help;
		}

		#endregion

		[Group("prefix")]
		[ModulePriority(1)]
		[Usage("[newPrefix|reset]")]
		[Summary("Gets or sets the bot's command prefix")]
		public class PrefixGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="PrefixGroup"/>.
			/// </summary>
			public PrefixGroup(DiscordBotServiceContainer services) : base(services) { }

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
		public class SpellCheckGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="SpellCheckGroup"/>.
			/// </summary>
			public SpellCheckGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("spellcheck")]
			[Command("")]
			[Example("Gets if `spoiler` command spellcheck is enabled")]
			public async Task GetSpellCheck() {
				bool state = true;
				using (var db = GetDb<SpoilerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					if (guild != null)
						state = guild.SpellCheck;
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
				using (var db = GetDb<SpoilerDbContext>()) {
					Guild guild = await db.GetOrAddGuild(Context).ConfigureAwait(false);
					if (guild == null)
						return NormalResult.FromSuccess();
					guild.SpellCheck = newState;
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
			string source = $"{Context.User.Username}#{Context.User.Discriminator}";
			string origin = $"  Guild: {Context.Guild?.Id}{Environment.NewLine}" +
							$"Channel: {Context.Channel.Id}{Environment.NewLine}" +
							$"   User: {Context.User.Id}";
			string logText = $"{Environment.NewLine}{origin}{Environment.NewLine}{message}";
			await Logging.LogAsync(new LogMessage(LogSeverities.Report, source, logText), logFile: true, noticeFile: true).ConfigureAwait(false);
			string channelStr = Config["ids:discord:report_channel"];
			if (channelStr != null && ulong.TryParse(channelStr, out ulong channelId)) {
				if (Client.GetChannel(channelId) is IMessageChannel channel) {
					EmbedBuilder embed = new EmbedBuilder {
						//Title = "Bug Report",
						Color = ColorUtils.Parse(Config["embed_color"]),
					};
					embed.AddField("Report Origin", $"Guild: `{Context.Guild?.Id}`\n" +
													$"Channel: `{Context.Channel.Id}`\n" +
													$"User: `{Context.User.Id}`");
					embed.AddField("Report Message", message);
					embed.WithFooter(Context.Guild.Name, Context.Guild.IconUrl);
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
		public class AllowBotsGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="AllowBotsGroup"/>.
			/// </summary>
			public AllowBotsGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("allowbots")]
			[Command("")]
			[Example("Gets if gets are allowed to use the `spoiler` command")]
			public async Task GetAllowBots() {
				bool state = false;
				using (var db = GetDb<SpoilerDbContext>()) {
					Guild guild = await db.FindGuildAsync(Context).ConfigureAwait(false);
					if (guild != null)
						state = guild.AllowBots;
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
				using (var db = GetDb<SpoilerDbContext>()) {
					Guild guild = await db.GetOrAddGuild(Context).ConfigureAwait(false);
					if (guild == null)
						return NormalResult.FromSuccess();
					guild.AllowBots = newState;
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
		public class LockGroup : DiscordBotModule {
			
			#region Constructors

			/// <summary>
			/// Constructs the <see cref="LockGroup"/>.
			/// </summary>
			public LockGroup(DiscordBotServiceContainer services) : base(services){ }

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
		public class UnlockGroup : DiscordBotModule {
			
			#region Constructors

			/// <summary>
			/// Constructs the <see cref="UnlockGroup"/>.
			/// </summary>
			public UnlockGroup(DiscordBotServiceContainer services) : base(services) { }

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
		public class LockedGroup : DiscordBotModule {

			#region Fields

			private readonly HelpService help;

			#endregion

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="LockedGroup"/>.
			/// </summary>
			public LockedGroup(DiscordBotServiceContainer services,
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
		public class ManagerRoleGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="ManagerRoleGroup"/>.
			/// </summary>
			public ManagerRoleGroup(DiscordBotServiceContainer services) : base(services) { }

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
	}
}
