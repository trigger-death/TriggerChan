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
using Discord.Commands;
using TriggersTools.DiscordBots.Reactions;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Utils;
using Discord.WebSocket;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.SpoilerBot.Modules;
using Victoria;

namespace TriggersTools.DiscordBots.TriggerChan {
	public class TriggerChanBot : DiscordBot {
		
		public override IServiceCollection ConfigureServices(IServiceCollection services) {
			return services
				.AddSingleton<CommandHandlerService, SpoilerCommandHandlerService>()
				.AddSingleton<ILoggingService, DefaultLoggingService>()
				.AddSingleton<IContextingService, ContextingService>()
				//.AddSingleton<SpoilerSpellCheckService>()
				.AddSingleton<MALApiDowntimeService>()
				.AddSingleton<DivergenceService>()
				.AddSingleton<EmotePreviewService>()
				.AddSingleton<ResultHandlerService>()
				.AddSingleton<HelpService>()
				.AddSingleton<ReactionService, TriggerReactions>()
				.AddSingleton<OcarinaService>()
				.AddSingleton<ISpoilerService, SpoilerService<TriggerDbContext>>()
				.AddSingleton<StatusRotationService>()
				.AddSingleton<ConfigParserService>()
				.AddSingleton<RolesService>()
				//.AddSingleton<AudioService>()
				.AddSingleton<DanbooruService>()
				.AddSingleton<TalkBackService>()
				.AddSingleton<TimeZoneService>()
				.AddSingleton<AsciifyService>()
				.AddSingleton<DatabaseProfileService>() // Database user profiles (MAL, AniList, VNDb, etc)

				// Audio
				.AddSingleton<Lavalink>()
				.AddSingleton<LavaAudioService>()
				//.AddSingleton<AudioService>()

				.AddSingleton<Random>()
				;
		}

		public override IServiceCollection ConfigureDatabase(IServiceCollection services) {
			return services
				.AddSingleton<IDbProvider, TriggerDbProvider>()
				.AddDbContext<TriggerDbContext>(ServiceLifetime.Transient)
				;
		}
		/// <summary>
		/// Configures all required services that should not be enumerable in the service collection.
		/// </summary>
		/// <param name="services">The services to configure.</param>
		/// <returns>Returns <paramref name="services"/>.</returns>
		public override IServiceCollection ConfigureHiddenServices(IServiceCollection services) {
			return services
				.AddEntityFrameworkNpgsql()
				;
		}

		public override IConfigurationRoot LoadConfig() {
			var builder = new ConfigurationBuilder()            // Create a new instance of the config builder
				.SetBasePath(AppContext.BaseDirectory)          // Specify the default location for the config file
				.AddJsonFile("Config.Public.json")              // Add this (json encoded) file to the configuration
				.AddJsonFile("Config.Private.json")             // Add this (json encoded) file to the configuration
				//.AddJsonFile(GetType(), "Config.Private.json");	// Add this embedded private (json encoded) file to the configuration
#if !PUBLISH
				.AddJsonFile("Config.Public.Debug.json")
				.AddJsonFile("Config.Private.Debug.json")
#endif
				;
			return Config = builder.Build();					// Build the configuration
		}

		public override DiscordSocketConfig GetDiscordSocketConfig() {
			return ConfigUtils.Parse<DiscordSocketConfig>(Config.GetSection("config"));
		}
		public override CommandServiceConfig GetCommandServiceConfig() {
			return ConfigUtils.Parse<CommandServiceConfig>(Config.GetSection("config"));
		}

		public override ReliabilityConfig GetReliabilityConfig() {
			return ConfigUtils.Parse<ReliabilityConfig>(Config.GetSection("config"));
		}

		public override async Task LoadCommandModulesAsync() {
			await base.LoadCommandModulesAsync().ConfigureAwait(false);
			//await Commands.AddModuleAsync<SpoilerModule>(Services).ConfigureAwait(false);
		}

		public override DiscordBotServiceContainer CreateServiceContainer(IServiceCollection services) {
			return new TriggerServiceContainer(services, ConfigureHiddenServices);
		}
	}
}
