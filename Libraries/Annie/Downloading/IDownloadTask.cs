using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Annie {
	/// <summary>
	/// The status of a download. Whether it's started, finished, or failed.
	/// </summary>
	public enum DownloadStatus {
		/// <summary>The download has not started yet.</summary>
		NotStarted,
		/// <summary>The download is in progress.</summary>
		InProgress,
		/// <summary>The download has finished successfully.</summary>
		Finished,
		/// <summary>The download has finished unsuccessfully.</summary>
		Failed,
	}
	/// <summary>
	/// The interface for a downloadable item.
	/// </summary>
	public interface IDownloadTask {

		#region Info
		
		/// <summary>
		/// Gets the source url/path that directed the downloader to this download task.
		/// </summary>
		string SourceUri { get; }
		/// <summary>
		/// Gets the namename of the download.
		/// </summary>
		string FileName { get; }
		/// <summary>
		/// Gets the extension of the download.
		/// </summary>
		string Extension { get; }
		/// <summary>
		/// Gets the resolved url for the download.
		/// </summary>
		string Uri { get; }
		/// <summary>
		/// Gets the file size of the download.
		/// </summary>
		long Size { get; }
		/// <summary>
		/// Gets the required proxy for the download task.
		/// </summary>
		WebProxyInfo Proxy { get; }
		/// <summary>
		/// Gets if the file is temporary and should be removed after use.
		/// </summary>
		bool IsTemporary { get; }

		#endregion

		#region State

		/// <summary>
		/// Gets or sets the download progress status.
		/// </summary>
		DownloadStatus Status { get; set; }
		/// <summary>
		/// Gets or sets the actual task for downloading the item.
		/// </summary>
		Task Task { get; set; }
		/// <summary>
		/// Gets the error that occurred while downloading.
		/// </summary>
		Exception Error { get; set; }

		#endregion

		#region Events

		/// <summary>
		/// Called before the actual downloading begins.
		/// </summary>
		void PreDownload();
		/// <summary>
		/// Called after the actual downloading has finished.
		/// </summary>
		void PostDownload();

		#endregion
	}
}
