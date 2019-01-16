using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.Utils;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Superuser")]
	[Summary("You're not supposed to be here, you need to leave")]
	[RequiresSuperuser]
	[IsLockable(false)]
	public class SuperuserModule : DiscordBotModule {
		#region Fields

		private readonly DevelopmentService devService;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SuperuserModule"/>.
		/// </summary>
		public SuperuserModule(DiscordBotServiceContainer services,
							   DevelopmentService devService)
			: base(services)
		{
			this.devService = devService;
		}

		#endregion

		[Name("updatedevstatus")]
		[Command("updatedevstatus")]
		[RequiresHomeGuild]
		public async Task<RuntimeResult> UpdateDevStatus() {
			await devService.UpdateDevStatusAsync().ConfigureAwait(false);
			return EmoteResults.FromSuccess();
		}
		[Name("updatewelcome")]
		[Command("updatewelcome")]
		[RequiresHomeGuild]
		public async Task<RuntimeResult> UpdateWelcome() {
			await devService.UpdateWelcomeAsync().ConfigureAwait(false);
			return EmoteResults.FromSuccess();
		}

		[Name("shutdown")]
		[Command("shutdown")]
		[Summary("Shuts down the bot without restarting it")]
		public async Task Shutdown() {
			await ReplyAsync("`See you later...`").ConfigureAwait(false);
			await DiscordBot.ShutdownAsync(Context).ConfigureAwait(false);
		}

		[Name("restart")]
		[Command("restart")]
		[Summary("Restarts the bot to help fix any errors that have accumulated")]
		public async Task Restart() {
			await ReplyAsync("`Be right back...`").ConfigureAwait(false);
			await DiscordBot.RestartAsync(Context, "`Yahallo World!`").ConfigureAwait(false);
		}

		[Name("error")]
		[Command("error")]
		[Summary("Throw an error")]
		public Task Error() {
			throw new Exception();
		}

		[Name("reloadconfig")]
		[Command("reloadconfig")]
		[Summary("Reloads the configuration file. Services should respond to updated changes")]
		public async Task<RuntimeResult> ReloadConfig() {
			var message = await ReplyAsync("`Reloading Config...`").ConfigureAwait(false);
			await DiscordBot.ReloadConfigAsync().ConfigureAwait(false);
			await message.ModifyAsync(m => m.Content = "`Reloading Config... Reloaded!`").ConfigureAwait(false);
			return EmoteResults.FromSuccess();
		}

		[Name("status <status>")]
		[Command("status")]
		public async Task<RuntimeResult> SetStatus(ActivityType activity, [Remainder] string name) {
			await Client.SetGameAsync(name, type: activity).ConfigureAwait(false);
			return EmoteResults.FromSuccess();
		}

		[Name("visibility <visibility>")]
		[Command("visibility"), Alias("visible")]
		public async Task<RuntimeResult>SetVisibility(UserStatus status) {
			await Client.SetStatusAsync(status).ConfigureAwait(false);
			return EmoteResults.FromSuccess();
		}

		[Name("supersay <text>")]
		[Command("supersay")]
		[Usage("<text...>")]
		[Example("Hello World!", "Makes the bot send a message with *Hello World!*")]
		[Summary("Make the bot say something and then delete your command message")]
		public async Task SaySuperuser([Remainder] string text) {
			await Context.Message.DeleteAsync().ConfigureAwait(false);
			await ReplyAsync(text).ConfigureAwait(false);
			//return DeletedResult.FromSuccess();
		}

		[Name("react")]
		[Command("react")]
		[Summary("Reacts to this command")]
		public Task<RuntimeResult> React() {
			return Task.FromResult(EmoteResults.FromSuccess());
		}

		[Group("log")]
		[Usage("[warning|error] <message...>")]
		[Summary("Writes the text to the log files under the input category or *Notice*")]
		public class LogGroup : DiscordBotModule {
			
			#region Constructors

			/// <summary>
			/// Constructs the <see cref="LogGroup"/>.
			/// </summary>
			public LogGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("log <message>")]
			[Command("")]
			[Priority(0)]
			[Example("Check recent errors", "Write a Notice of *Check recent errors* to the log file")]
			public Task<RuntimeResult> LogNotice([Remainder] string message) {
				return LogAsync(message, LogSeverities.Notice);
			}
			[Name("log warning <message>")]
			[Command("warning")]
			[Priority(1)]
			[Example("That last thing was acting strange", "Write a Warning of *That last thing was acting strange* to the log file")]
			public Task<RuntimeResult> LogWarning([Remainder] string message) {
				return LogAsync(message, LogSeverity.Warning);
			}
			[Name("log error <message>")]
			[Command("error")]
			[Priority(1)]
			[Example("Something went very wrong", "Write an Error of *Something went very wrong* to the log file")]
			public Task<RuntimeResult> LogError([Remainder] string message) {
				return LogAsync(message, LogSeverity.Error);
			}

			private async Task<RuntimeResult> LogAsync(string message, LogSeverity severity) {
				await Logging.LogAsync(new LogMessage(severity, $"{Context.User.Username}#{Context.User.Discriminator}", message), logFile: true, noticeFile: true).ConfigureAwait(false);
				return EmoteResults.FromSuccess();
			}
		}

		[Group("erase")]
		[Usage("<user|guild> <id>")]
		[Summary("Erases End User Data from the database")]
		public class EraseGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="EraseGroup"/>.
			/// </summary>
			public EraseGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("erase user <userId>")]
			[Command("user")]
			[Example("497125828120018945", "Erases the user with this Id")]
			public async Task EraseUser(ulong id) {
				var wait = new EraseEndUserDataWaitContext(Context, EndUserDataType.User, id);
				await wait.StartAsync().ConfigureAwait(false);
			}

			[Name("erase guild <guildId>")]
			[Command("guild")]
			[Example("436949335947870238", "Erases the guild with this Id")]
			public async Task EraseGuild(ulong id) {
				var wait = new EraseEndUserDataWaitContext(Context, EndUserDataType.Guild, id);
				await wait.StartAsync().ConfigureAwait(false);
			}
		}

		[Group("botban")]
		[Usage("<user|guild> <id>")]
		[Summary("Ban the user or guild from using the bot")]
		public class BotBanGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="BotBanGroup"/>.
			/// </summary>
			public BotBanGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("botban user <userId>")]
			[Command("user")]
			[Example("497125828120018945", "Bans the user with this Id from using the bot")]
			public async Task<RuntimeResult> BotBanUser(ulong id) {
				using (var db = GetDb<TriggerDbContext>()) {
					User user = await db.FindUserAsync(id).ConfigureAwait(false);
					string[] owners = Config.GetArray("ids:discord:superuseres");
					if (user.Banned) {
						await ReplyAsync("This user is already banned.").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else if (owners.Any(idStr => ulong.Parse(idStr) == id)) {
						await ReplyAsync("You can't ban a superuser! *Baka.*").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else {
							user.Banned = true;
						db.ModifyOnly(user, u => u.Banned);
						await db.SaveChangesAsync().ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
			}

			[Name("botban guild <guildId>")]
			[Command("guild")]
			[Example("436949335947870238", "Bans the guild with this Id from using the bot")]
			public async Task<RuntimeResult> BotBanGuild(ulong id) {
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(id).ConfigureAwait(false);
					ulong homeGuild = ulong.Parse(Config["ids:discord:home:guild"]);
					ulong emoteGuild = ulong.Parse(Config["ids:discord:home:emote_guild"]);
					if (guild.Banned) {
						await ReplyAsync("This guild is already banned.").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else if (id == homeGuild || id == emoteGuild) {
						await ReplyAsync("You can't ban your home guilds! *Baka.*").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else {
						guild.Banned = true;
						db.ModifyOnly(guild, g => g.Banned);
						await db.SaveChangesAsync().ConfigureAwait(false);
						try {
							await Client.GetGuild(id).LeaveAsync().ConfigureAwait(false);
							return ReactionResult.FromSuccess(TriggerReactions.Success, new Emoji("🚪"));
						} catch { }
						return EmoteResults.FromSuccess();
					}
				}
			}
		}

		[Group("botunban")]
		[Usage("<user|guild> <id>")]
		[Summary("Unbans the user or guild from using the bot")]
		public class BotUnbanGroup : DiscordBotModule {

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="BotBanGroup"/>.
			/// </summary>
			public BotUnbanGroup(DiscordBotServiceContainer services) : base(services) { }

			#endregion

			[Name("botunban user <userId>")]
			[Command("user")]
			[Example("497125828120018945", "Unbans the user with this Id from using the bot")]
			public async Task<RuntimeResult> BotUnbanUser(ulong id) {
				using (var db = GetDb<TriggerDbContext>()) {
					User user = await db.FindUserAsync(id).ConfigureAwait(false);
					if (!user.Banned) {
						await ReplyAsync("This user is not banned.").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else {
						user.Banned = false;
						db.ModifyOnly(user, u => u.Banned);
						await db.SaveChangesAsync().ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
			}

			[Name("botunban guild <guildId>")]
			[Command("guild")]
			[Example("436949335947870238", "Unbans the guild with this Id from using the bot")]
			public async Task<RuntimeResult> BotUnbanGuild(ulong id) {
				using (var db = GetDb<TriggerDbContext>()) {
					Guild guild = await db.FindGuildAsync(id).ConfigureAwait(false);
					if (!guild.Banned) {
						await ReplyAsync("This guild is not banned.").ConfigureAwait(false);
						return NormalResult.FromSuccess();
					}
					else {
						guild.Banned = false;
						db.ModifyOnly(guild, g => g.Banned);
						await db.SaveChangesAsync().ConfigureAwait(false);
						return EmoteResults.FromSuccess();
					}
				}
			}
		}
	}
}
