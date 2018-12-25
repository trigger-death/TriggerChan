using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public class TriggerModule : DiscordBotModule {
		#region Fields

		/// <summary>
		/// Gets the bot service provider.
		/// </summary>
		public new TriggerServiceContainer Services => (TriggerServiceContainer) base.Services;
		/// <summary>
		/// Gets the home guild Ids section.
		/// </summary>
		public IConfigurationSection Home => Services.Home;

		#endregion

		#region Constructors

		public TriggerModule(TriggerServiceContainer services) : base(services) { }

		#endregion
	}
}
