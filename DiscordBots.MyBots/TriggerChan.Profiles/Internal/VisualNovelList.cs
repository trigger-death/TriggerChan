using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IVisualNovelList"/>.
	/// </summary>
	internal class VisualNovelList : MediaList, IVisualNovelList {
		/// <summary>
		/// Constructs the visual novel list to set the <see cref="ListType"/>.
		/// </summary>
		public VisualNovelList() {
			ListType = ListType.VisualNovels;
		}
		
	}
}
