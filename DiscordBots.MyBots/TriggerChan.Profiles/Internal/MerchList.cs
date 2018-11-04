using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IMerchList"/>.
	/// </summary>
	internal class MerchList : DatabaseUserList, IMerchList {
		/// <summary>
		/// Gets the number of owned items in the list.
		/// </summary>
		public int? Owned { get; set; }
		/// <summary>
		/// Gets the number of ordered items in the list.
		/// </summary>
		public int? Ordered { get; set; }
		/// <summary>
		/// Gets the number of wished-for items in the list.
		/// </summary>
		public int? Wished { get; set; }
		/// <summary>
		/// Gets the number of favorited items in the list.
		/// </summary>
		public int? Favorites { get; set; }

		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public override IEnumerable<string> GetFields(InfoLevel infoLevel) {
			var info = ListType.ToInfo();
			if (Owned.HasValue)
				yield return $"{info.StatusNames[ListStatus.Owned]}: {Owned:N0}";
			if (Ordered.HasValue)
				yield return $"{info.StatusNames[ListStatus.Ordered]}: {Ordered:N0}";
			if (Wished.HasValue)
				yield return $"{info.StatusNames[ListStatus.Wished]}: {Wished:N0}";
			if (Favorites.HasValue)
				yield return $"{info.StatusNames[ListStatus.Favorites]}: {Favorites:N0}";
		}
	}
	/// <summary>
	/// The internal implementation of <see cref="IFigureList"/>.
	/// </summary>
	internal class FigureList : MerchList, IFigureList {
		public FigureList() {
			ListType = ListType.Figures;
		}
	}
	/// <summary>
	/// The internal implementation of <see cref="IGoodsList"/>.
	/// </summary>
	internal class GoodsList : MerchList, IGoodsList {
		public GoodsList() {
			ListType = ListType.Goods;
		}
	}
	/// <summary>
	/// The internal implementation of <see cref="IAllMediaList"/>.
	/// </summary>
	internal class AllMediaList : MerchList, IAllMediaList {
		public AllMediaList() {
			ListType = ListType.Media;
		}
	}
}
