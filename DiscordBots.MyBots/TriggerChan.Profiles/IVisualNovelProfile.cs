using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for an visual novel database user profile.
	/// </summary>
	public interface IVisualNovelProfile : IDatabaseUserProfile {
		/// <summary>
		/// Gets if the user has an anime list.
		/// </summary>
		bool HasVisualNovels { get; }

		/// <summary>
		/// Gets the user's visual novel list.
		/// </summary>
		IVisualNovelList VisualNovels { get; }

		/// <summary>
		/// Gets the user's visual novel lists.
		/// </summary>
		new IEnumerable<IVisualNovelList> Lists { get; }
	}
}
