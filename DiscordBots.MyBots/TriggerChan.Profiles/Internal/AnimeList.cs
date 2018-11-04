using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// The internal implementation of <see cref="IAnimeList"/>.
	/// </summary>
	internal class AnimeList : MediaList, IAnimeList {
		/// <summary>
		/// Constructs the anime list to set the <see cref="ListType"/>.
		/// </summary>
		public AnimeList() {
			ListType = ListType.Anime;
		}
		
		/// <summary>
		/// Gets the total number of episodes watched.
		/// </summary>
		public int? Episodes { get; set; }

		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public override IEnumerable<string> GetFields(InfoLevel infoLevel) {
			foreach (string field in base.GetFields(infoLevel))
				yield return field;
			if (Episodes.HasValue)
				yield return $"Episodes: {Episodes:N0}";
		}
	}
	/// <summary>
	/// The internal implementation of <see cref="IMangaList"/>.
	/// </summary>
	internal class MangaList : MediaList, IMangaList {
		/// <summary>
		/// Constructs the manga list to set the <see cref="ListType"/>.
		/// </summary>
		public MangaList() {
			ListType = ListType.Manga;
		}

		/// <summary>
		/// Gets the total number of chapters read.
		/// </summary>
		public int? Chapters { get; set; }
		/// <summary>
		/// Gets the total number of volumes read.
		/// </summary>
		public int? Volumes { get; set; }

		/// <summary>
		/// Gets the list of displayable fields in the list embed.
		/// </summary>
		/// <param name="infoLevel">The level of information to display.</param>
		/// <returns>The list of fields to display.</returns>
		public override IEnumerable<string> GetFields(InfoLevel infoLevel) {
			foreach (string field in base.GetFields(infoLevel))
				yield return field;
			if (Chapters.HasValue)
				yield return $"Chapters: {Chapters:N0}";
			if (Volumes.HasValue)
				yield return $"Volumes: {Volumes:N0}";
		}
	}
}
