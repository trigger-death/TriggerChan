using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	public interface IDatabaseUserList {
		/// <summary>
		/// Gets the type of the list.
		/// </summary>
		ListType ListType { get; }
		/// <summary>
		/// Gets the Url of the list.
		/// </summary>
		string Url { get; }
		
		/// <summary>
		/// Gets the title of the list embed.
		/// </summary>
		string Title { get; }
		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		IEnumerable<string> GetFields(InfoLevel infoLevel);
	}
}
