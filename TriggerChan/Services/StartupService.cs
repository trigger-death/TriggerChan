using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Modules;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class StartupService {
		public DiscordSocketClient Client { get; }
		public CommandService Commands { get; }
		public IConfigurationRoot Config { get; }
		public SettingsService Settings { get; }
		public LoggingService Logging { get; }
		public AudioService Audio { get; }
		public FunService Fun { get; }
		public SpoilerService Spoilers { get; }
		public ServiceProvider Services { get; private set; }
		public ServiceCollection ServiceCollection { get; private set; }

		// DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
		public StartupService(
			DiscordSocketClient discord,
			CommandService commands,
			IConfigurationRoot config,
			SettingsService settings,
			LoggingService logging,
			AudioService audio,
			FunService fun,
			SpoilerService spoilers)
		{
			Client = discord;
			Commands = commands;
			Config = config;
			Settings = settings;
			Logging = logging;
			Audio = audio;
			Fun = fun;
			Spoilers = spoilers;
		}

		public async Task StartAsync(ServiceProvider services, ServiceCollection serviceCollection) {
			Services = services;
			ServiceCollection = serviceCollection;
			string discordToken = Config["tokens:discord"];     // Get the discord token from the config file
			if (string.IsNullOrWhiteSpace(discordToken))
				throw new Exception("Please enter your bot's token into the `Config.json` file found in the applications root directory.");

			await Client.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
			await Client.StartAsync();                                // Connect to the websocket
			Client.Connected += OnDiscordConnected;


			await Commands.AddModulesAsync(Assembly.GetEntryAssembly());     // Load commands and modules into the command service
		}

		private async Task OnDiscordConnected() {
			try {
				using (var database = new BotDatabaseContext())
					database.Database.EnsureCreated();
			}
			catch (Exception ex) {
				Console.WriteLine(ex);
			}
			Type serviceType = typeof(BotServiceBase);
			foreach (ServiceDescriptor descriptor in ServiceCollection) {
				if (serviceType.IsAssignableFrom(descriptor.ServiceType)) {
					BotServiceBase service = (BotServiceBase)
						Services.GetService(descriptor.ServiceType);
					service.Initialize(Services);
				}
			}
			await SetDefaultStatus();
			//await Client.SetGameAsync("Kill la Kill", null, ActivityType.Watching);
			/*Dictionary<ulong, IMessageChannel> channels = new Dictionary<ulong, IMessageChannel>();
			List<CachedMessage> messagesToDelete = new List<CachedMessage>();
			foreach (CachedMessage cache in database.CachedMessages) {
				IMessageChannel channel;
				if (!channels.TryGetValue(cache.ChannelId, out channel)) {
					channel = discord.GetChannel(cache.ChannelId) as IMessageChannel;
					if (channel != null)
						channels.Add(cache.ChannelId, channel);
				}
				if (channel == null) {
					Console.WriteLine("Deleting because no channel: {0}", cache.ChannelId);
					messagesToDelete.Add(cache);
					continue;
				}
				IMessage message = await channel.GetMessageAsync(cache.Id, CacheMode.AllowDownload);
				if (message != null)
					Console.WriteLine("Cached message: {0}", cache.Id);
				else {
					messagesToDelete.Add(cache);
					Console.WriteLine("Deleting because no message: {0}", cache.Id);
				}
			}
			//database.CachedMessages.RemoveRange(messagesToDelete);
			await database.SaveChangesAsync();*/
		}

		public async Task SetDefaultStatus() {
			await Client.SetGameAsync("with Scissor Blades", null, ActivityType.Playing);
		}
	}
}
