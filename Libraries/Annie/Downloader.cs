using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Annie {
	/// <summary>
	/// URL data struct for single URL information
	/// </summary>
	public class DownloaderURL {
		[JsonProperty(PropertyName = "url")]
		public string URL { get; set; }
		[JsonProperty(PropertyName = "size")]
		public long Size { get; set; }
		[JsonProperty(PropertyName = "ext")]
		public string Ext { get; set; }
	}
	/// <summary>
	/// Stream data struct for each stream
	/// </summary>
	public class DownloaderStream {
		/// <summary>
		/// [URL: {URL, Size, Ext}, ...]
		/// Some video files have multiple fragments
		/// and support for downloading multiple image files at once
		/// </summary>
		[JsonProperty(PropertyName = "urls")]
		public List<DownloaderURL> URLs { get; set; }
		[JsonProperty(PropertyName = "quality")]
		public string Quality { get; set; }
		/// <summary>
		/// total size of all urls
		/// </summary>
		[JsonProperty(PropertyName = "size")]
		public long Size { get; set; }
		/// <summary>
		/// name used in sortedStreams
		/// </summary>
		[JsonIgnore]
		public string name { get; set; }
	}
	/// <summary>
	/// Data data struct for video information
	/// </summary>
	public class DownloaderData {
		[JsonProperty(PropertyName = "site")]
		public string Site { get; set; }
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
		/// <summary>
		/// each stream has it's own URLs and Quality
		/// </summary>
		[JsonProperty(PropertyName = "streams")]
		public Dictionary<string, DownloaderStream> Streams { get; set; }
		[JsonIgnore]
		public DownloaderStream[] sortedStreams;

		/// <summary>
		/// Err is used to record whether an error occurred when extracting data.
		/// It is used to record the error information corresponding to each url when extracting the list data.
		/// NOTE(iawia002): err is only used in Data list
		/// </summary>
		[JsonProperty(PropertyName = "-")]
		public Exception Err;
		/// <summary>
		/// URL is used to record the address of this download
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string URL { get; set; }
	}
	public static class Downloader {

		//static readonly HttpClient client = new HttpClient();
		const long chunkSize = 10 * 1024 * 1024;
		static readonly byte[] buffer = new byte[chunkSize];
		private static int WriteFile(string url, Stream file, long start, long end, WebProxy proxy) {
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.Method = "GET";
			request.Referer = "https://www.youtube.com";
			request.Proxy = proxy;
			request.AddRange(start, end);
			using (var response = request.GetResponse())
			using (var stream = response.GetResponseStream())
				stream.CopyTo(file);
			return (int) (end - start + 1);
		}

		public static void Save(DownloaderURL urlData, string fileName, WebProxy proxy) {
			string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
			string tempFilePath = fileName + ".download";
			using (var file = File.Create(tempFilePath)) {
				//if (urlData.URL.Contains("googlevideo")) {
				long start = 0, end;
					
				long remainingSize = urlData.Size;
				long chunks = remainingSize / chunkSize;
				if (remainingSize % chunkSize != 0) {
					chunks++;
				}
				for (long i = 1; i <= chunks; i++) {
					end = start + chunkSize - 1;
					end = Math.Min(urlData.Size - 1, end);
					long temp = start;
					int written = WriteFile(urlData.URL, file, start, end, proxy);
					temp += written;
					start = end + 1;
				}
				//}
			}
			if (File.Exists(filePath))
				File.Delete(filePath);
			File.Move(tempFilePath, filePath);
		}

	}
}
