using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IDatabaseUserList"/>.
	/// </summary>
	internal abstract class DatabaseUserList : IDatabaseUserList {
		/// <summary>
		/// Gets the type of the list.
		/// </summary>
		public ListType ListType { get; set; }
		/// <summary>
		/// Gets the Url of the list.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Gets the title of the list embed.
		/// </summary>
		public string Title => ListType.ToInfo().Name;
		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public abstract IEnumerable<string> GetFields(InfoLevel infoLevel);
	}
}
