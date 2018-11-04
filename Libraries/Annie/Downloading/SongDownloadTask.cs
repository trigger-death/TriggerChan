using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annie {
	/// <summary>
	/// The actual download task for a song.
	/// </summary>
	public class SongDownloadTask : ISongDownloadTask {

		#region Info

		/// <summary>
		/// Gets the source url/path that directed the downloader to this download task.
		/// </summary>
		public string SourceUri { get; set; }
		/// <summary>
		/// Gets the namename of the download.
		/// </summary>
		public string FileName { get; set; }
		/// <summary>
		/// Gets the extension of the download.
		/// </summary>
		public string Extension { get; set; }
		/// <summary>
		/// Gets the resolved url for the download.
		/// </summary>
		public string Uri { get; set; }
		/// <summary>
		/// Gets the file size of the download.
		/// </summary>
		public long Size { get; set; }
		/// <summary>
		/// Gets the required proxy for the download task.
		/// </summary>
		public WebProxyInfo Proxy { get; set; }
		/// <summary>
		/// Gets if the file is temporary and should be removed after use.
		/// </summary>
		public bool IsTemporary { get; set; }

		#endregion

		#region Song Info

		/// <summary>
		/// Gets the title of the song.
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// Gets the duration of the song.
		/// </summary>
		public TimeSpan Duration { get; set; }
		/// <summary>
		/// Gets the Url of the song thumbnail.
		/// </summary>
		public string ThumbnailUrl { get; set; }

		#endregion

		#region State

		/// <summary>
		/// Gets or sets the download progress status.
		/// </summary>
		public DownloadStatus Status { get; set; }
		/// <summary>
		/// Gets or sets the actual task for downloading the item.
		/// </summary>
		public Task Task { get; set; }
		/// <summary>
		/// Gets the error that occurred while downloading.
		/// </summary>
		public Exception Error { get; set; }

		#endregion

		#region Events

		/// <summary>
		/// Called before the actual downloading begins.
		/// </summary>
		public virtual void PreDownload() { }
		/// <summary>
		/// Called after the actual downloading has finished.
		/// </summary>
		public virtual void PostDownload() { }

		#endregion
	}
	/// <summary>
	/// The actual download task for a local song.
	/// </summary>
	public class LocalSongDownloadInfo : SongDownloadTask {

		public LocalSongDownloadInfo() {
			// We better not be deleting my files
			IsTemporary = false;
		}
	}
	/// <summary>
	/// The actual download task for a Discord attachment song.
	/// </summary>
	public class DiscordSongDownloadTask : SongDownloadTask {

	}
	/// <summary>
	/// The actual download task for a YouTube song.
	/// </summary>
	public class YouTubeDownloadTask : SongDownloadTask, IYouTubeDownloadTask {

	}
}
