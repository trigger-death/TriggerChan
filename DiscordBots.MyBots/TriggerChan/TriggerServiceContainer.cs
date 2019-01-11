using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan {
	/// <summary>
	/// List services here that should be accessible from other services.
	/// </summary>
	/// <remarks>
	/// This class does make a mess of dependency injection, but it's helpful in reducing the typing load
	/// which is extremely important in my situation.
	/// </remarks>
	public class TriggerServiceContainer : DiscordBotServiceContainer {
		#region Properties

		/// <summary>
		/// Gets the home guild Ids section.
		/// </summary>
		public IConfigurationSection Home => Config.GetSection("ids:discord:home");
		/// <summary>
		/// Gets the service used for managing banishments from the bots.
		/// </summary>
		public BotBanService BotBans => GetService<BotBanService>();

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="TriggerServiceContainer"/> with the specified services.
		/// </summary>
		/// <param name="services">The service collection to construct the provider from.</param>
		public TriggerServiceContainer(IServiceCollection services,
			Func<IServiceCollection, IServiceCollection> configureHiddenServices)
			: base(services)
		{
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Override this to assign global services that should be prepared before all other services are
		/// constructed.
		/// </summary>
		public override void AssignServices() {
			
		}

		#endregion

	}
}
