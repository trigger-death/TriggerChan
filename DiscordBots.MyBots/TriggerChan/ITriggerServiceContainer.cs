using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TriggersTools.DiscordBots;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Database;

namespace TriggersTools.DiscordBots.TriggerChan {
	/// <summary>
	/// A context for Trigger-Chan services, modules, and command contexts.
	/// </summary>
	public interface ITriggerServiceContainer : IDiscordBotServiceContainer {
		#region Properties
		
		/// <summary>
		/// Gets the Http Client for web requests.
		/// </summary>
		HttpClient HttpClient { get; }

		#endregion
	}
}
