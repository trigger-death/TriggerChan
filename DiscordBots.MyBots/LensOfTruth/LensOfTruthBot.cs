using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Configurations;
using TriggersTools.DiscordBots.Database.Model;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using Discord.Commands;
using TriggersTools.DiscordBots.Reactions;
using TriggersTools.DiscordBots.SpoilerBot.Reactions;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Utils;
using Discord.WebSocket;

namespace TriggersTools.DiscordBots.SpoilerBot {
	public class LensOfTruthBot : DiscordBot {
		
		public override IServiceCollection ConfigureServices(IServiceCollection services) {
			return services
				.AddSingleton<CommandHandlerService, SpoilerCommandHandlerService>()
				.AddSingleton<ILoggingService, DefaultLoggingService>()
				.AddSingleton<IContextingService, ContextingService>()
				//.AddSingleton<SpoilerSpellCheckService>()
				.AddSingleton<ResultHandlerService>()
				.AddSingleton<HelpService>()
				.AddSingleton<ReactionService, SpoilerReactions>()
				.AddSingleton<OcarinaService>()
				.AddSingleton<SpoilerService>()
				.AddSingleton<StatusRotationService>()
				.AddSingleton<Random>()
				;
		}

		public override IServiceCollection ConfigureDatabase(IServiceCollection services) {
			return services
				.AddEntityFrameworkNpgsql()
				.AddSingleton<IDbProvider, SpoilerDbProvider>()
				.AddDbContext<SpoilerDbContext>(ServiceLifetime.Transient)
				;
		}
		/*public override async Task<string> GetPrefixAsync(ICommandContext context) {
			using (var db = Services.GetDb<SpoilerDbContext>())
				return (await db.GetGuild(context).ConfigureAwait(false))?.Prefix ?? DefaultPrefix;
		}*/
		
		public override IConfigurationRoot LoadConfig() {
			var builder = new ConfigurationBuilder()			// Create a new instance of the config builder
				.SetBasePath(AppContext.BaseDirectory)			// Specify the default location for the config file
				.AddJsonFile("Config.Public.json")				// Add this (json encoded) file to the configuration
				.AddJsonFile(GetType(), "Config.Private.json");	// Add this embedded private (json encoded) file to the configuration
			return Config = builder.Build();								// Build the configuration
		}

		public override DiscordSocketConfig GetDiscordSocketConfig() {
			return ConfigUtils.Parse<DiscordSocketConfig>(Config, "config");
		}
		public override CommandServiceConfig GetCommandServiceConfig() {
			return ConfigUtils.Parse<CommandServiceConfig>(Config, "config");
		}

		public override ReliabilityConfig GetReliabilityConfig() {
			return ConfigUtils.Parse<ReliabilityConfig>(Config, "config");
		}

		/*public override Task<bool> IsLockedAsync(ICommandContext context, CommandInfo cmd) {
			CommandDetails command = Commands.CommandSet.FindCommand(cmd.GetDetailsName());
			return Services.GetService<ILockingService>().IsLockedAsync(context, command);
		}
		public override Task<bool> IsCommandLockedAsync(ICommandContext context, CommandInfo cmd) {
			CommandDetails command = Commands.CommandSet.FindCommand(cmd.GetDetailsName());
			return Services.GetService<ILockingService>().IsCommandLockedAsync(context, command);
		}
		public override Task<bool> IsModuleLockedAsync(ICommandContext context, ModuleInfo mod) {
			ModuleDetails module = Commands.CommandSet.FindModule(mod.GetDetailsName());
			return Services.GetService<ILockingService>().IsModuleLockedAsync(context, module);
		}*/
	}
}
