using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IDatabaseUserProfile"/>.
	/// </summary>
	internal class DatabaseUserProfile : IDatabaseUserProfile {
		/// <summary>
		/// Gets the database this profile relates to.
		/// </summary>
		public DatabaseType DatabaseType { get; set; }
		/// <summary>
		/// Gets the username of the user.
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// Gets the Id of the user.
		/// </summary>
		public long? Id { get; set; }
		/// <summary>
		/// Gets the Url of the user's profile page.
		/// </summary>
		public string Url { get; set; }
		/// <summary>
		/// Gets the Url of the user's avatar image.
		/// </summary>
		public string AvatarUrl { get; set; }
		/// <summary>
		/// Gets the user's lists.
		/// </summary>
		public IEnumerable<IDatabaseUserList> Lists => GetLists();
		/// <summary>
		/// Gets the user's lists. Used to allow use of the new operator over the <see cref="Lists"/> property.
		/// </summary>
		protected virtual IEnumerable<IDatabaseUserList> GetLists() => Enumerable.Empty<IDatabaseUserList>();

		/// <summary>
		/// Gets the display title of the profile embed.
		/// </summary>
		public virtual string Title => $"{Username}'s {DatabaseType.ToInfo().DefaultName} Profile";
		/// <summary>
		/// Gets the display description for the profile embed.
		/// </summary>
		public virtual string Description => null;
		/// <summary>
		/// Gets the list of displayable fields in the profile embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public virtual IEnumerable<string> GetFields(InfoLevel infoLevel) => Enumerable.Empty<string>();
	}
}
