using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annie {
	public class YouTubeArgs {
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		[JsonProperty(PropertyName = "adaptive_fmts")]
		public string Stream { get; set; }
		[JsonProperty(PropertyName = "url_encoded_fmt_stream_map")]
		public string Stream2 { get; set; }
		[JsonProperty(PropertyName = "length_seconds")]
		public double DurationSeconds { get; set; }
		[JsonIgnore]
		public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
	}

	public class YouTubeAssets {
		[JsonProperty(PropertyName = "js")]
		public string JS { get; set; }
	}

	public class YouTubeData {
		[JsonProperty(PropertyName = "args")]
		public YouTubeArgs Args { get; set; }
		[JsonProperty(PropertyName = "assets")]
		public YouTubeAssets Assets { get; set; }
	}
	public static class YouTube {

		public static bool IsVideoUnavailable(string html) {
			const string unavailableContainer = @"id=""player-unavailable""";
			return html.Contains(unavailableContainer);
		}

		public static DownloaderData youtubeDownload(string uri, WebProxy proxy) {
			string[] vid = Utils.MatchOneOf(
				uri,
				@"watch\?v=([^/&]+)",
				@"youtu\.be/([^?/]+)",
				@"embed/([^/?]+)`",
				@"v/([^/?]+)`");
			if (vid == null)
				throw new Exception("can't find vid");
			string videoURL = $@"https://www.youtube.com/watch?v={vid[1]}&gl=US&hl=en&has_verified=1&bpctr=9999999999";
			
			string html = Request.Get(videoURL, proxy);
			if (IsVideoUnavailable(html))
				return null;
			string ytplayer = Utils.MatchOneOf(html, @";ytplayer\.config\s*=\s*({.+?});")[1];
			
			YouTubeData youtube = JsonConvert.DeserializeObject<YouTubeData>(ytplayer);
			string title = youtube.Args.Title;

			var streams = extractVideoURLS(youtube, proxy);

			return new DownloaderData {
				Site = "YouTube youtube.com",
				Title = title,
				Type = "video",
				Streams = streams,
				URL = uri,
			};
		}

		public static Dictionary<string, DownloaderStream> extractVideoURLS(YouTubeData data, WebProxy proxy) {
			string[] youtubeStreams = data.Args.Stream.Split(',');
			if (string.IsNullOrEmpty(data.Args.Stream)) {
				youtubeStreams = data.Args.Stream2.Split(',');
			}
			string ext;
			DownloaderURL audio = null;
			var streams = new Dictionary<string, DownloaderStream>();

			foreach (string s in youtubeStreams) {
				var stream = ParseQuery(s);
				stream.TryGetValue("itag", out string itag);
				stream.TryGetValue("type", out string streamType);
				bool isAudio = streamType.StartsWith("audio/mp4");

				stream.TryGetValue("quality_label", out string quality);
				if (string.IsNullOrEmpty(quality))
					stream.TryGetValue("quality", out quality); // for url_encoded_fmt_stream_map

				if (!string.IsNullOrEmpty(quality))
					quality = $"{quality} {streamType}";
				else
					quality = streamType;

				if (isAudio)
					ext = "m4a";
				else
					//continue;
					ext = Utils.MatchOneOf(streamType, @"(\w+)/(\w+);")[2];
				string realURL = genSignedURL(stream["url"], stream, data.Assets.JS, proxy);
				long size = Request.Size(realURL, proxy);
				DownloaderURL urlData = new DownloaderURL {
					URL = realURL,
					Size = size,
					Ext = ext,
				};
				if (isAudio) {
					// Audio data for merging with video
					audio = urlData;
				}
				streams[itag] = new DownloaderStream {
					URLs = new List<DownloaderURL> { urlData },
					Size = size,
					Quality = quality,
				};
			}

			// `url_encoded_fmt_stream_map`
			if (string.IsNullOrEmpty(data.Args.Stream)) {
				return streams;
			}

			// Unlike `url_encoded_fmt_stream_map`, all videos in `adaptive_fmts` have no sound,
			// we need download video and audio both and then merge them.
			// Another problem is that even if we add `ratebypass=yes`, the download speed still slow sometimes. https://github.com/iawia002/annie/issues/191#issuecomment-405449649

			// All videos here have no sound and need to be added separately
			foreach (var f in streams.Values) {
				if (f.Quality.Contains("video/")) {
					f.Size += audio.Size;
					f.URLs.Add(audio);
				}
			}
			return streams;
		}
		public static string genSignedURL(string streamURL, Dictionary<string, string> stream, string js, WebProxy proxy) {
			string realURL;
			if (streamURL.Contains("signature=")) {
				// URL itself already has a signature parameter
				realURL = streamURL;
			}
			else {
				// URL has no signature parameter
				stream.TryGetValue("sig", out string sig);
				if (string.IsNullOrEmpty(sig)) {
					// Signature need decrypt
					sig = getSig(stream["s"], js, proxy);
				}
				realURL = $"{streamURL}&signature={sig}";
			}
			if (!realURL.Contains("ratebypass")) {
				realURL += "&ratebypass=yes";
			}
			return realURL;
		}
		private static readonly ConcurrentDictionary<string, string[]> tokensCache = new ConcurrentDictionary<string, string[]>();
		public static string getSig(string sig, string js, WebProxy proxy) {
			string sigURL = $@"https://www.youtube.com{js}";
			if (!tokensCache.TryGetValue(sigURL, out string[] tokens)) {
				string html = Request.Get(sigURL, proxy);
				tokens = Signatures.getSigTokens(html);
				tokensCache.TryAdd(sigURL, tokens);
			}
			return Signatures.decipherTokens(tokens, sig);
		}

		public static Dictionary<string, string> ParseQuery(string s) {
			// remove anything other than query string from url
			int index = s.IndexOf("?");
			if (index != -1)
				s = s.Substring(index + 1);

			var dictionary = new Dictionary<string, string>();

			foreach (string vp in s.Split('&')) {
				string[] strings = vp.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				dictionary.Add(strings[0], strings.Length == 2 ? WebUtility.UrlDecode(strings[1]) : string.Empty);
			}

			return dictionary;
		}
	}
}
