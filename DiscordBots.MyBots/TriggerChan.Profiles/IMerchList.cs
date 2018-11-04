using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for a merchandice database user list.
	/// </summary>
	public interface IMerchList : IDatabaseUserList {
		/// <summary>
		/// Gets the number of owned items in the list.
		/// </summary>
		int? Owned { get; }
		/// <summary>
		/// Gets the number of ordered items in the list.
		/// </summary>
		int? Ordered { get; }
		/// <summary>
		/// Gets the number of wished-for items in the list.
		/// </summary>
		int? Wished { get; }
		/// <summary>
		/// Gets the number of favorited items in the list.
		/// </summary>
		int? Favorites { get; }
	}
	/// <summary>
	/// The interface for a merchandice database user figure list.
	/// </summary>
	public interface IFigureList : IMerchList {

	}
	/// <summary>
	/// The interface for a merchandice database user goods list.
	/// </summary>
	public interface IGoodsList : IMerchList {

	}
	/// <summary>
	/// The interface for a merchandice database user media list.
	/// </summary>
	public interface IAllMediaList : IMerchList {
		
	}
}
