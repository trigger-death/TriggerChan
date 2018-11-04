using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Database {
	/// <summary>
	/// The design-time factory for <see cref="TriggerDbContext"/>.
	/// </summary>
	public class TriggerDbContextFactory : DbContextExFactory<TriggerDbProvider, TriggerDbContext> {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="TriggerDbContextFactory"/>.
		/// </summary>
		public TriggerDbContextFactory() : base(new TriggerChanBot().LoadConfig()) { }

		#endregion

		#region Virtual Methods
		
		/// <summary>
		/// Configures additional required services.
		/// </summary>
		/// <param name="services">The service collection to add to.</param>
		/// <returns>The same server collection that was passed.</returns>
		protected override IServiceCollection ConfigureServices(IServiceCollection services) {
			return base.ConfigureServices(services)
				.AddEntityFrameworkNpgsql();
		}

		#endregion
	}
}
