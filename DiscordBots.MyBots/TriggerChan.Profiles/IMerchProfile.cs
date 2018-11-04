using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for an merchandise database user profile.
	/// </summary>
	public interface IMerchProfile : IDatabaseUserProfile {
		/// <summary>
		/// Gets the status of the My Figure Collection user.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Gets if the user has a figure list.
		/// </summary>
		bool HasFigures { get; }
		/// <summary>
		/// Gets if the user has a goods list.
		/// </summary>
		bool HasGoods { get; }
		/// <summary>
		/// Gets if the user has a media list.
		/// </summary>
		bool HasMedia { get; }

		/// <summary>
		/// Gets the user's figure list.
		/// </summary>
		IFigureList Figures { get; }
		/// <summary>
		/// Gets the user's goods list.
		/// </summary>
		IGoodsList Goods { get; }
		/// <summary>
		/// Gets the user's media list.
		/// </summary>
		IAllMediaList Media { get; }

		/// <summary>
		/// Gets the user's merchandice lists.
		/// </summary>
		new IEnumerable<IMerchList> Lists { get; }
	}
}
