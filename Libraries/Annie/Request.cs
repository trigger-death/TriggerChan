using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Annie {
	public static class Request {
		
		/*public static WebHeaderCollection Headers(string url, WebProxy proxy) {
			WebRequest request = WebRequest.Create(url);
			request.Proxy = proxy;
			request.Method = "GET";
			request.Timeout = 5000;
			using (WebResponse response = request.GetResponse())
				return response.Headers;
		}*/

		public static long Size(string url, WebProxy proxy) {
			var request = (HttpWebRequest) WebRequest.Create(url);
			request.Proxy = proxy;
			request.Method = "GET";
			request.Timeout = 5000;
			request.Referer = "https://www.youtube.com";
			using (WebResponse response = request.GetResponse()) {
				if (long.TryParse(response.Headers.Get("Content-Length"), out long size))
					return size;
			}
			return 0L;
		}

		public static string Get(string url, WebProxy proxy) {
			var request = (HttpWebRequest) WebRequest.Create(url);
			request.Proxy = proxy;
			request.Method = "GET";
			request.Timeout = 10000;
			request.Referer = "https://www.youtube.com";

			using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
			using (var reader = new StreamReader(responseStream))
				return reader.ReadToEnd();
		}
	}
}
