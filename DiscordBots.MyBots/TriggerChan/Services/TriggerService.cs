using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TriggersTools.DiscordBots.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class TriggerService : DiscordBotService {
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

		public TriggerService(TriggerServiceContainer services) : base(services) { }

		#endregion
	}
}
