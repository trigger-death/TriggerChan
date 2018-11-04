using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Services;

namespace TriggersTools.DiscordBots.SpoilerBot.Database {
	/// <summary>
	/// The design-time factory for <see cref="SpoilerDbContext"/>.
	/// </summary>
	public class SpoilerDbContextFactory : DbContextExFactory<SpoilerDbProvider, SpoilerDbContext> {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="SpoilerDbContextFactory"/>.
		/// </summary>
		public SpoilerDbContextFactory() : base(new LensOfTruthBot().LoadConfig()) { }

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
