using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maya.Music.Youtube {
	public static class MusicDownloader {

		public static SongDownloadInfo GetDownloadInfo(string url, int maxResolution = 720, Action onProxy = null) {
			IEnumerable<YouTubeInfo> videoInfos = YouTubeUrlResolver.GetDownloadUrls(url, false, onProxy);
			YouTubeInfo video = null;
			foreach (YouTubeInfo info in videoInfos) {
				//if (info.AdaptiveType != AdaptiveType.Audio)
				//	continue;
				if (info.Resolution <= maxResolution &&
					(video == null || info.Resolution > video.Resolution ||
					(info.Resolution == video.Resolution && info.VideoType == VideoType.Mp4)))
				{
					video = info;
				}
				 /*elseif (video == null || (info.AudioBitrate > video.AudioBitrate && info.AudioType == AudioType.Aac)) {
					video = info;
				}*/
			}
			return video;
		}

		public static async Task Download(IDownloadInfo download) {
			lock (download) {
				download.FileName = Path.ChangeExtension(download.FileName, download.Extension);
			}
			download.ResolveUrl();
			try {
				using (HttpClientHandler handler = new HttpClientHandler()) {
					handler.Proxy = download.Proxy;
					using (HttpClient http = new HttpClient(handler))
					using (Stream source = await http.GetStreamAsync(download.DownloadUrl).ConfigureAwait(false))
					using (Stream target = File.OpenWrite(download.FileName)) {
						await source.CopyToAsync(target).ConfigureAwait(false);
					}
					lock (download) {
						download.DownloadStatus = DownloadStatus.Finished;
						download.PostDownload();
					}
				}
			}
			catch (Exception ex) {
				lock (download) {
					download.DownloadError = ex;
					download.DownloadStatus = DownloadStatus.Failed;
				}
			}
		}
	}
}
