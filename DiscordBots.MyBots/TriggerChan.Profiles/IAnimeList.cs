using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for an anime database user anime list.
	/// </summary>
	public interface IAnimeList : IMediaList {
		/// <summary>
		/// Gets the total number of episodes watched.
		/// </summary>
		int? Episodes { get; }
	}
	/// <summary>
	/// The interface for an anime database user manga list.
	/// </summary>
	public interface IMangaList : IMediaList {
		/// <summary>
		/// Gets the total number of chapters read.
		/// </summary>
		int? Chapters { get; }
		/// <summary>
		/// Gets the total number of volumes read.
		/// </summary>
		int? Volumes { get; }
	}
}
