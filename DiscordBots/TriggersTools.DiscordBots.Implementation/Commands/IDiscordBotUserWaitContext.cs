using System;
using Discord.WebSocket;
using TriggersTools.DiscordBots.Services;

namespace TriggersTools.DiscordBots.Commands {
	/// <summary>
	/// A context for waiting for additional command input afte the fact.
	/// </summary>
	public interface IDiscordBotUserWaitContext : IUserWaitContext, IDiscordBotServiceContainer, IDisposable {
		/// <summary>
		/// Gets the connected Discord socket client.
		/// </summary>
		new DiscordSocketClient Client { get; }
	}
}
