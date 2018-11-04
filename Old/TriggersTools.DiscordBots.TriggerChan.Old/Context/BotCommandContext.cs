using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Context {
	public class BotCommandContext : SocketCommandContext, IBotErrorResult {

		private BotServiceBase serviceBase;

		public BotCommandContext(BotServiceBase serviceBase, SocketUserMessage msg)
			: base(serviceBase.Client, msg)
		{
			this.serviceBase = serviceBase;
			IsSuccess = true;
		}

		public Exception Exception { get; set; }
		public CustomCommandError? CustomError { get; set; }
		public CommandError? Error { get; set; }
		public string ErrorReason { get; set;}
		public bool IsSuccess { get; set; }

		public CommandService Commands => serviceBase.Commands;
		public IConfigurationRoot Config => serviceBase.Config;
		public IServiceProvider Services => serviceBase.Services;
		public SettingsService Settings => serviceBase.Settings;
		public LoggingService Logging => serviceBase.Logging;
		public AudioService Audio => serviceBase.Audio;
		public FunService Fun => serviceBase.Fun;
		public Random Random => serviceBase.Random;
		public SpoilerService Spoilers => serviceBase.Spoilers;
		public DanbooruService Danbooru => serviceBase.Danbooru;
		public HelpService Help => serviceBase.Help;
		public StartupService Startup => serviceBase.Startup;
	}
}
