using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TriggersTools.DiscordBots.Database {
	/// <summary>
	/// How databases should be instantiated when opened for the first time.
	/// </summary>
	public enum DbCreationMode {
		/// <summary>The database must already exist.</summary>
		OpenExisting,
		/// <summary>Call <see cref="DatabaseFacade.EnsureCreated"/>.</summary>
		EnsureCreated,
		/// <summary>Call <see cref="RelationalDatabaseFacadeExtensions.Migrate"/>.</summary>
		Migrate,
	}
}
