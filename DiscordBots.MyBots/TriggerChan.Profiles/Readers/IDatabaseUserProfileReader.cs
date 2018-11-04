using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers {
	/// <summary>
	/// The interface for reading database user profiles through API or scraping.
	/// </summary>
	public interface IDatabaseUserProfileReader {
		/// <summary>
		/// Parses the user name from the input. Accepts urls, usernames, and user Ids.
		/// </summary>
		/// <param name="input">The input string to parse.</param>
		/// <returns>The parsed username result.</returns>
		//Task<ParsedUsername> ParseUsernameAsync(string input);

		/// <summary>
		/// Parses the user name from the input. Accepts urls, usernames, and user Ids.
		/// </summary>
		/// <param name="input">The input string to parse.</param>
		/// <returns>The parsed username, or null if it could not be found.</returns>
		string ParseUsername(string input);

		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		Task<IDatabaseUserProfile> QueryProfileAsync(string usernameOrId);
	}
	/// <summary>
	/// The interface for reading anime database user profiles through API or scraping.
	/// </summary>
	public interface IAnimeProfileReader : IDatabaseUserProfileReader {
		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		new Task<IAnimeProfile> QueryProfileAsync(string usernameOrId);
	}
	/// <summary>
	/// The interface for reading visual novel database user profiles through API or scraping.
	/// </summary>
	public interface IVisualNovelProfileReader : IDatabaseUserProfileReader {
		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		new Task<IVisualNovelProfile> QueryProfileAsync(string usernameOrId);
	}
	/// <summary>
	/// The interface for reading merchandice database user profiles through API or scraping.
	/// </summary>
	public interface IMerchProfileReader : IDatabaseUserProfileReader {
		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		new Task<IMerchProfile> QueryProfileAsync(string usernameOrId);
	}
}
