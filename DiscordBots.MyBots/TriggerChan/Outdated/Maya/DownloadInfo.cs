using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Maya.Music.Youtube {
	public enum DownloadStatus {
		InProgress,
		Finished,
		Failed,
	}
	public interface IDownloadInfo {
		string FileName { get; set; }
		string DownloadUrl { get; set; }
		DownloadStatus DownloadStatus { get; set; }
		string Extension { get; }
		Task DownloadTask { get; set; }
		WebProxy Proxy { get; set; }
		Exception DownloadError { get; set; }

		void ResolveUrl();
		void PostDownload();
	}
	public abstract class SongDownloadInfo : SongInfo, IDownloadInfo {

		public string DownloadUrl { get; set; }
		public DownloadStatus DownloadStatus { get; set; }
		public abstract string Extension { get; }
		public WebProxy Proxy { get; set; }

		public Task DownloadTask { get; set; }
		public Exception DownloadError { get; set; }

		public virtual void ResolveUrl() { }
		public virtual void PostDownload() { }

		public SongDownloadInfo() {
			IsTemporary = true;
		}

	}

	public class DiscordSongInfo : SongDownloadInfo {
		public override string Extension => Path.GetExtension(FileName);
		public override void PostDownload() {
			GetLocalDetails();
		}
	}
}
