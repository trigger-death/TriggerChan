/*
 * Code from Youtube Extractor
 * Credits to: Flagbug
 * Link: https://github.com/flagbug/YoutubeExtractor
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Maya.Music.Youtube {
	internal static class HttpHelper {

		public static List<WebProxy> Proxies = new List<WebProxy>{
			new WebProxy(@"159.89.124.13:8080") {
				UseDefaultCredentials = true },
		};

		private static Task<WebResponse> ProxyTask = null;

		private static object ProxyLock = new object();

		public static string DownloadString(string url, WebProxy proxy) {
			var request = WebRequest.Create(url);
			request.Proxy = proxy;
			request.Method = "GET";
			request.Timeout = 120000;

			Task<WebResponse> task;
			if (proxy == null) {
				task = MakeTask(request);
			}
			else {
				// Prevent overuse of proxy to avoid timeouts
				lock (ProxyLock) {
					if (!ProxyTask?.IsCompleted ?? false)
						ProxyTask.Wait();
					task = MakeTask(request);
					ProxyTask = task;
				}
			}

			return task.ContinueWith(t => ReadStreamFromResponse(t.Result)).Result;
		}

		private static Task<WebResponse> MakeTask(WebRequest request) {
			return Task.Factory.FromAsync(
					request.BeginGetResponse,
					asyncResult => request.EndGetResponse(asyncResult),
					null);
		}

		public static string HtmlDecode(string value) {
			return WebUtility.HtmlDecode(value);
		}

		public static IDictionary<string, string> ParseQueryString(string s) {
			// remove anything other than query string from url
			if (s.Contains("?")) {
				s = s.Substring(s.IndexOf('?') + 1);
			}

			var dictionary = new Dictionary<string, string>();

			foreach (string vp in Regex.Split(s, "&")) {
				string[] strings = Regex.Split(vp, "=");
				dictionary.Add(strings[0], strings.Length == 2 ? UrlDecode(strings[1]) : string.Empty);
			}

			return dictionary;
		}

		public static string ReplaceQueryStringParameter(string currentPageUrl, string paramToReplace, string newValue) {
			var query = ParseQueryString(currentPageUrl);

			query[paramToReplace] = newValue;

			var resultQuery = new StringBuilder();
			bool isFirst = true;

			foreach (KeyValuePair<string, string> pair in query) {
				if (!isFirst) {
					resultQuery.Append("&");
				}

				resultQuery.Append(pair.Key);
				resultQuery.Append("=");
				resultQuery.Append(pair.Value);

				isFirst = false;
			}

			var uriBuilder = new UriBuilder(currentPageUrl) {
				Query = resultQuery.ToString()
			};

			return uriBuilder.ToString();
		}

		public static string UrlDecode(string url) {
			return System.Net.WebUtility.UrlDecode(url);
		}

		private static string ReadStreamFromResponse(WebResponse response) {
			using (Stream responseStream = response.GetResponseStream()) {
				using (var sr = new StreamReader(responseStream)) {
					return sr.ReadToEnd();
				}
			}
		}
	}
}
