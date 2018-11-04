using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IVisualNovelProfile"/>.
	/// </summary>
	internal class VisualNovelProfile : DatabaseUserProfile, IVisualNovelProfile {
		/// <summary>
		/// Gets if the user has an anime list.
		/// </summary>
		public bool HasVisualNovels => VisualNovels != null;

		/// <summary>
		/// Gets the user's visual novel list.
		/// </summary>
		public IVisualNovelList VisualNovels { get; set; }
		/// <summary>
		/// Gets the user's media lists.
		/// </summary>
		public new IEnumerable<IVisualNovelList> Lists {
			get {
				if (HasVisualNovels) yield return VisualNovels;
			}
		}
		/// <summary>
		/// Gets the user's media lists.
		/// </summary>
		protected override IEnumerable<IDatabaseUserList> GetLists() => Lists;
	}
}
