using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// An interface for a user for a website.
	/// </summary>
	public interface IDatabaseUserProfile {
		/// <summary>
		/// Gets the database this profile relates to.
		/// </summary>
		DatabaseType DatabaseType { get; }
		/// <summary>
		/// Gets the username of the user.
		/// </summary>
		string Username { get; }
		/// <summary>
		/// Gets the Id of the user.
		/// </summary>
		long? Id { get; }
		/// <summary>
		/// Gets the Url of the user's profile page.
		/// </summary>
		string Url { get; }
		/// <summary>
		/// Gets the Url of the user's avatar image.
		/// </summary>
		string AvatarUrl { get; }
		/// <summary>
		/// Gets the user's lists.
		/// </summary>
		IEnumerable<IDatabaseUserList> Lists { get; }

		/// <summary>
		/// Gets the display title of the profile embed.
		/// </summary>
		string Title { get; }
		/// <summary>
		/// Gets the display description for the profile embed.
		/// </summary>
		string Description { get; }
		/// <summary>
		/// Gets the list of displayable fields in the profile embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		IEnumerable<string> GetFields(InfoLevel infoLevel);
	}
}
