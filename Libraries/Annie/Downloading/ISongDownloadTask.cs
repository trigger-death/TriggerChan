using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annie {
	/// <summary>
	/// The interface for a downloadable song item.
	/// </summary>
	public interface ISongDownloadTask : IDownloadTask {

		#region Song Info

		/// <summary>
		/// Gets the title of the song.
		/// </summary>
		string Title { get; }
		/// <summary>
		/// Gets the duration of the song.
		/// </summary>
		TimeSpan Duration { get; }
		/// <summary>
		/// Gets the Url of the song thumbnail.
		/// </summary>
		string ThumbnailUrl { get; }

		#endregion
	}
	/// <summary>
	/// The interface for a downloadable local song item.
	/// </summary>
	public interface ILocalDownloadTask : ISongDownloadTask {

	}
	/// <summary>
	/// The interface for a downloadable Discord attachment song item.
	/// </summary>
	public interface IDiscordDownloadTask : ISongDownloadTask {

	}
	/// <summary>
	/// The interface for a downloadable YouTube song item.
	/// </summary>
	public interface IYouTubeDownloadTask : ISongDownloadTask {

	}
}
