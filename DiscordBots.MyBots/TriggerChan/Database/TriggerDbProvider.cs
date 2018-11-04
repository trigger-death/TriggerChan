using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Configurations;

namespace TriggersTools.DiscordBots.TriggerChan.Database {
	public class TriggerDbProvider : DbProvider {

		#region Constructors
		
		public TriggerDbProvider(IConfigurationRoot config) : base(config) { }

		#endregion

		#region Override Methods

		protected override void AddConfigurations() {
			AddConfiguration(new SqliteConfiguration());
			AddConfiguration(new NpgsqlConfiguration());
		}

		#endregion
	}
}
