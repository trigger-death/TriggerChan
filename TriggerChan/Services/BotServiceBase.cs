using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public abstract class BotServiceBase {

		public bool IsInitialized { get; private set; }

		public ServiceProvider Services { get; private set; }
		public DiscordSocketClient Client { get; private set; }
		public CommandService Commands { get; private set; }
		public IConfigurationRoot Config { get; private set; }
		public SettingsService Settings { get; private set; }
		public LoggingService Logging { get; private set; }
		public AudioService Audio { get; private set; }
		public FunService Fun { get; private set; }
		public DanbooruService Danbooru { get; private set; }
		public SpoilerService Spoilers { get; private set; }
		public HelpService Help { get; private set; }
		public Random Random { get; private set; }
		public StartupService Startup { get; private set; }

		public void Initialize(ServiceProvider services) {
			if (!IsInitialized) {
				Services = services;
				Client = services.GetService<DiscordSocketClient>();
				Commands = services.GetService<CommandService>();
				Config = services.GetService<IConfigurationRoot>();
				Settings = services.GetService<SettingsService>();
				Logging = services.GetService<LoggingService>();
				Audio = services.GetService<AudioService>();
				Fun = services.GetService<FunService>();
				Spoilers = services.GetService<SpoilerService>();
				Help = services.GetService<HelpService>();
				Danbooru = services.GetService<DanbooruService>();
				Startup = services.GetService<StartupService>();
				Random = services.GetService<Random>();
				OnInitialized(services);
				IsInitialized = true;
			}
		}

		protected virtual void OnInitialized(ServiceProvider services) { }

	}
}
