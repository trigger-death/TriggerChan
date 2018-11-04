using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IMerchProfile"/>.
	/// </summary>
	internal class MerchProfile : DatabaseUserProfile, IMerchProfile {
		/// <summary>
		/// Gets the status of the My Figure Collection user.
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// Gets if the user has a figure list.
		/// </summary>
		public bool HasFigures => Figures != null;
		/// <summary>
		/// Gets if the user has a goods list.
		/// </summary>
		public bool HasGoods => Goods != null;
		/// <summary>
		/// Gets if the user has a media list.
		/// </summary>
		public bool HasMedia => Media != null;

		/// <summary>
		/// Gets the user's figure list.
		/// </summary>
		public IFigureList Figures { get; set; }
		/// <summary>
		/// Gets the user's goods list.
		/// </summary>
		public IGoodsList Goods { get; set; }
		/// <summary>
		/// Gets the user's media list.
		/// </summary>
		public IAllMediaList Media { get; set; }
		/// <summary>
		/// Gets the user's merch lists.
		/// </summary>
		public new IEnumerable<IMerchList> Lists {
			get {
				if (HasFigures) yield return Figures;
				if (HasGoods)   yield return Goods;
				if (HasMedia)   yield return Media;
			}
		}
		/// <summary>
		/// Gets the user's merch lists.
		/// </summary>
		protected override IEnumerable<IDatabaseUserList> GetLists() => Lists;

		/// <summary>
		/// Gets the display description for the profile embed.
		/// </summary>
		public override string Description => Status;
	}
}
