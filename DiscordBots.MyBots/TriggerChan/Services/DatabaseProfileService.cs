using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Profiles;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class DatabaseProfileService : TriggerService {

		
		private readonly Dictionary<DatabaseType, IDatabaseUserProfileReader> readers = new Dictionary<DatabaseType, IDatabaseUserProfileReader> {
			{ DatabaseType.MyAnimeList, new MyAnimeListProfileReader() },
			{ DatabaseType.AniList, new AniListProfileReader() },
			{ DatabaseType.Kitsu, null/*new KitsuProfileReader()*/ },
			{ DatabaseType.VNDb, null/*new VNdbProfileReader()*/ },
			{ DatabaseType.MyFigureCollection, new MyFigureCollectionProfileReader() },
		};

		public DatabaseProfileService(TriggerServiceContainer services) : base(services) {
			
		}

		/// <summary>
		/// Gets the database user profile reader of the specified database type.
		/// </summary>
		/// <param name="database">The database service to get the reader for.</param>
		/// <returns>The reader for the specified database service.</returns>
		public IDatabaseUserProfileReader GetReader(DatabaseType database) {
			return readers[database];
		}

		/// <summary>
		/// Parses the user name from the input. Accepts urls, usernames, and user Ids.
		/// </summary>
		/// <param name="database">The database service to parse the username for.</param>
		/// <param name="input">The input string to parse.</param>
		/// <returns>The parsed username, or null if it could not be found.</returns>
		public string ParseUsername(DatabaseType database, string input) {
			return readers[database].ParseUsername(input);
		}
		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="database">The database service to query the profile from.</param>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		public Task<IDatabaseUserProfile> QueryProfileAsync(DatabaseType database, string username) {
			return readers[database].QueryProfileAsync(username);
		}
	}
}
