/*
 * Code from Youtube Extractor
 * Credits to: Flagbug
 * Link: https://github.com/flagbug/YoutubeExtractor
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Maya.Music.Youtube {
	public static class YouTubeUrlResolver {
		private const string RateBypassFlag = "ratebypass";
		private const string SignatureQuery = "signature";

		public static void DecryptDownloadUrl(YouTubeInfo videoInfo) {
			IDictionary<string, string> queries = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);

			if (queries.ContainsKey(SignatureQuery)) {
				string encryptedSignature = queries[SignatureQuery];

				string decrypted;

				try {
					decrypted = GetDecipheredSignature(videoInfo.HtmlPlayerVersion, encryptedSignature, videoInfo.Proxy);
				} catch (Exception ex) {
					throw new Exception("Could not decipher signature", ex);
				}

				videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decrypted);
				videoInfo.RequiresDecryption = false;
			}
		}

		public static IEnumerable<YouTubeInfo> GetDownloadUrls(string videoUrl, bool decryptSignature = true, Action onProxy = null) {
			if (videoUrl == null)
				throw new ArgumentNullException("videoUrl");

			bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);

			if (!isYoutubeUrl) {
				throw new ArgumentException("URL is not a valid youtube URL!");
			}

			try {
				WebProxy proxy;
				var json = LoadJson(videoUrl, out proxy, onProxy);

				string videoTitle = GetVideoTitle(json);
				TimeSpan videoDuration = GetVideoDuration(json);

				IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);

				IEnumerable<YouTubeInfo> infos = GetVideoInfos(downloadUrls, videoTitle, videoDuration, proxy).ToList();

				string htmlPlayerVersion = "vflHFWD7-/en_US/base";
				try {
					htmlPlayerVersion = GetHtml5PlayerVersion(json);
				}
				catch { }

				foreach (YouTubeInfo info in infos) {
					info.HtmlPlayerVersion = htmlPlayerVersion;

					if (decryptSignature && info.RequiresDecryption) {
						DecryptDownloadUrl(info);
					}
				}

				return infos;
			}

			catch (Exception ex) {
				if (ex is WebException) {
					throw;
				}

				throw new Exception("Could not parse the Youtube page for URL " + videoUrl, ex);
			}
		}

		public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl) {
			url = url.Trim();

			url = url.Replace("youtu.be/", "youtube.com/watch?v=");
			url = url.Replace("www.youtube", "youtube");
			url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

			if (url.Contains("/v/")) {
				url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
			}

			url = url.Replace("/watch#", "/watch?");

			IDictionary<string, string> query = HttpHelper.ParseQueryString(url);

			string v;

			if (!query.TryGetValue("v", out v)) {
				normalizedUrl = null;
				return false;
			}

			normalizedUrl = "http://youtube.com/watch?v=" + v;

			return true;
		}

		private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(JObject json) {
			string[] splitByUrls = GetStreamMap(json).Split(',');
			string[] adaptiveFmtSplitByUrls = GetAdaptiveStreamMap(json).Split(',');
			splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

			foreach (string s in splitByUrls) {
				IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
				string url;

				bool requiresDecryption = false;

				if (queries.ContainsKey("s") || queries.ContainsKey("sig")) {
					requiresDecryption = queries.ContainsKey("s");
					string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];

					url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);

					string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;

					url += fallbackHost;
				}

				else {
					url = queries["url"];
				}

				url = HttpHelper.UrlDecode(url);
				url = HttpHelper.UrlDecode(url);

				IDictionary<string, string> parameters = HttpHelper.ParseQueryString(url);
				if (!parameters.ContainsKey(RateBypassFlag))
					url += string.Format("&{0}={1}", RateBypassFlag, "yes");

				yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
			}
		}

		private static string GetAdaptiveStreamMap(JObject json) {
			JToken streamMap = json["args"]["adaptive_fmts"];

			// bugfix: adaptive_fmts is missing in some videos, use url_encoded_fmt_stream_map instead
			if (streamMap == null) {
				streamMap = json["args"]["url_encoded_fmt_stream_map"];
			}

			return streamMap.ToString();
		}

		private static string GetDecipheredSignature(string htmlPlayerVersion, string signature, WebProxy proxy) {
			return Decipherer.DecipherWithVersion(signature, htmlPlayerVersion, proxy);
		}

		private static string GetHtml5PlayerVersion(JObject json) {
			var regex = new Regex(@"player-(.+?).js|player_(.+?).js");

			string js = json["assets"]["js"].ToString();

			Debug.WriteLine(js);
			Debug.WriteLine(regex.Match(js).Result("$1"));
			return regex.Match(js).Result("$1");
		}

		private static string GetStreamMap(JObject json) {
			JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

			string streamMapString = streamMap == null ? null : streamMap.ToString();

			if (streamMapString == null || streamMapString.Contains("been+removed")) {
				throw new Exception("Video is removed or has an age restriction.");
			}

			return streamMapString;
		}

		private static IEnumerable<YouTubeInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle, TimeSpan videoDuration, WebProxy proxy) {
			var downLoadInfos = new List<YouTubeInfo>();

			foreach (ExtractionInfo extractionInfo in extractionInfos) {
				string itag = HttpHelper.ParseQueryString(extractionInfo.Uri.Query)["itag"];

				int formatCode = int.Parse(itag);

				YouTubeInfo info = YouTubeInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

				if (info != null) {
					info = new YouTubeInfo(info) {
						DownloadUrl = extractionInfo.Uri.ToString(),
						Title = videoTitle,
						Duration = videoDuration,
						RequiresDecryption = extractionInfo.RequiresDecryption,
						Proxy = proxy,
					};
				}

				else {
					info = new YouTubeInfo(formatCode) {
						DownloadUrl = extractionInfo.Uri.ToString()
					};
				}

				downLoadInfos.Add(info);
			}

			return downLoadInfos;
		}

		private static string GetVideoTitle(JObject json) {
			JToken title = json["args"]["title"];

			return title == null ? String.Empty : title.ToString();
		}

		private static TimeSpan GetVideoDuration(JObject json) {
			JToken seconds = json["args"]["length_seconds"];

			return seconds == null ? TimeSpan.Zero : TimeSpan.FromSeconds(double.Parse(seconds.ToString()));
		}

		private static bool IsVideoUnavailable(string pageSource) {
			const string unavailableContainer = "<div id=\"watch-player-unavailable\">";

			return pageSource.Contains(unavailableContainer);
		}

		private static JObject LoadJsonInternal(string url, WebProxy proxy) {
			string pageSource = HttpHelper.DownloadString(url, proxy);

			if (IsVideoUnavailable(pageSource)) {
				throw new Exception();
			}

			var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);
			Match m = dataRegex.Match(pageSource);
			if (!m.Success)
				throw new Exception("Could not LoadJson");
			string extractedJson = m.Result("$1");

			return JObject.Parse(extractedJson);
		}

		private static JObject LoadJson(string url, out WebProxy proxy, Action onProxy = null) {
			proxy = null;
			try {
				return LoadJsonInternal(url, null);
			}
			catch (Exception) {
				onProxy?.Invoke();
				for (int i = 0; i < HttpHelper.Proxies.Count; i++) {
					try {
						proxy = HttpHelper.Proxies[i];
						return LoadJsonInternal(url, proxy);
					}
					catch (Exception ex) {
						if (i + 1 == HttpHelper.Proxies.Count)
							throw ex;
					}
				}
			}
			return null;
		}

		private class ExtractionInfo {
			public bool RequiresDecryption { get; set; }

			public Uri Uri { get; set; }
		}
	}
}
