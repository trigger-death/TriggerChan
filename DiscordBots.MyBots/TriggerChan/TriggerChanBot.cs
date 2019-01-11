using System;
using System.Collections.Generic;
using System.IO;
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
		#region Constants

		/// <summary>
		/// The config directory to set to in the constructor for the Discord bot.<para/>
		/// It's a quirk that it needs to be setup this way, but Databases rely on this information, and also
		/// on the bot class.
		/// </summary>
		private static readonly string CtorConfigDirectory =
			Path.Combine(AppContext.BaseDirectory, ".config");
		/// <summary>
		/// The state directory to set to in the constructor for the Discord bot.<para/>
		/// It's a quirk that it needs to be setup this way, but Databases rely on this information, and also
		/// on the bot class.
		/// </summary>
		private static readonly string CtorStateDirectory =
			Path.Combine(CtorConfigDirectory, "state");

		#endregion

		#region Constructors

		public TriggerChanBot() : base(CtorConfigDirectory, CtorStateDirectory) { }

		#endregion

		#region Configuration

		/// <summary>
		/// Loads the configuration files used for the bot.
		/// </summary>
		public override IConfigurationRoot LoadConfig() {
			var builder = new ConfigurationBuilder()            // Create a new instance of the config builder
				.SetBasePath(ConfigDirectory)   // Specify the default location for the config file
				.AddJsonFile("Config.Public.json")              // Add this (json encoded) file to the configuration
				.AddJsonFile("Config.Private.json")             // Add this (json encoded) file to the configuration
#if !PUBLISH
				.AddJsonFile("Config.Public.Beta.json")
				.AddJsonFile("Config.Private.Beta.json")
#endif
				;
			return Config = builder.Build();                    // Build the configuration
		}
		/// <summary>
		/// Loads the <see cref="DiscordSocketConfig"/> used to automatically setup the
		/// <see cref="DiscordSocketClient"/>.
		/// </summary>
		public override DiscordSocketConfig GetDiscordSocketConfig() {
			return ConfigUtils.Parse<DiscordSocketConfig>(Config.GetSection("config"));
		}
		/// <summary>
		/// Loads the <see cref="CommandServiceConfig"/> used to automatically setup the
		/// <see cref="CommandService"/>.
		/// </summary>
		public override CommandServiceConfig GetCommandServiceConfig() {
			return ConfigUtils.Parse<CommandServiceConfig>(Config.GetSection("config"));
		}
		/// <summary>
		/// Loads the <see cref="ReliabilityConfig"/> used to automatically setup the
		/// <see cref="ReliabilityService"/>.
		/// </summary>
		public override ReliabilityConfig GetReliabilityConfig() {
			return ConfigUtils.Parse<ReliabilityConfig>(Config.GetSection("config"));
		}

		/// <summary>
		/// Configures all required services for the Discord Bot.
		/// </summary>
		/// <param name="services">The services to configure.</param>
		/// <returns>Returns <paramref name="services"/>.</returns>
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
				.AddSingleton<DanbooruService>()
				.AddSingleton<TalkBackService>()
				.AddSingleton<TimeZoneService>()
				.AddSingleton<AsciifyService>()
				.AddSingleton<DevelopmentService>()
				.AddSingleton<DatabaseProfileService>() // Database user profiles (MAL, AniList, VNDb, etc)

				// Audio
				.AddSingleton<Lavalink>()
				.AddSingleton<LavaAudioService>()
				//.AddSingleton<AudioService>()

				.AddSingleton<Random>()
				.AddSingleton<BotBanService>()
				;
		}
		/// <summary>
		/// Configures all required database services for the Discord Bot.
		/// </summary>
		/// <param name="services">The services to configure.</param>
		/// <returns>Returns <paramref name="services"/>.</returns>
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

		/// <summary>
		/// Creates the <see cref="DiscordBotServiceContainer"/> or an extended class.
		/// </summary>
		/// <param name="services">The service collection to initialize the service container with.</param>
		/// <returns>The newly created service container.</returns>
		public override DiscordBotServiceContainer CreateServiceContainer(IServiceCollection services) {
			return new TriggerServiceContainer(services, ConfigureHiddenServices);
		}

		#endregion

		#region Management

		/// <summary>
		/// Loads the commands and modules into the command service.
		/// </summary>
		public override async Task LoadCommandModulesAsync() {
			await base.LoadCommandModulesAsync().ConfigureAwait(false);
			//await Commands.AddModuleAsync<SpoilerModule>(Services).ConfigureAwait(false);
		}

		#endregion
	}
}
