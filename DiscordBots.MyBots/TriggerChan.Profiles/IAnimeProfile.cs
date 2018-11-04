using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The interface for an anime database user profile.
	/// </summary>
	public interface IAnimeProfile : IDatabaseUserProfile {
		/// <summary>
		/// Gets if the user has an anime list.
		/// </summary>
		bool HasAnime { get; }
		/// <summary>
		/// Gets if the user has a manga list.
		/// </summary>
		bool HasManga { get; }

		/// <summary>
		/// Gets the user's anime list.
		/// </summary>
		IAnimeList Anime { get; }
		/// <summary>
		/// Gets the user's manga list.
		/// </summary>
		IMangaList Manga { get; }

		/// <summary>
		/// Gets the user's anime and manga lists.
		/// </summary>
		new IEnumerable<IMediaList> Lists { get; }
	}
}
