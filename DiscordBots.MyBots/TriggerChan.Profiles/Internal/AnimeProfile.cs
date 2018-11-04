using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IAnimeProfile"/>.
	/// </summary>
	internal class AnimeProfile : DatabaseUserProfile, IAnimeProfile {
		/// <summary>
		/// Gets if the user has an anime list.
		/// </summary>
		public bool HasAnime => Anime != null;
		/// <summary>
		/// Gets if the user has a manga list.
		/// </summary>
		public bool HasManga => Manga != null;
		
		/// <summary>
		/// Gets the user's anime list.
		/// </summary>
		public IAnimeList Anime { get; set; }
		/// <summary>
		/// Gets the user's manga list.
		/// </summary>
		public IMangaList Manga { get; set; }
		/// <summary>
		/// Gets the user's anime and manga lists.
		/// </summary>
		public new IEnumerable<IMediaList> Lists {
			get {
				if (HasAnime) yield return Anime;
				if (HasManga) yield return Manga;
			}
		}
		/// <summary>
		/// Gets the user's media lists.
		/// </summary>
		protected override IEnumerable<IDatabaseUserList> GetLists() => Lists;
	}
}
