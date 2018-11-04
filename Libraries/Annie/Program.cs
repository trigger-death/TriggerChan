using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Annie {
	class Program {
		static int Main(string[] args) {
			//var data = YouTube.youtubeDownload(@"https://www.youtube.com/watch?v=XDk5Ynr9L4A");
			var proxy = new WebProxy(@"46.101.17.199:3128") {
				UseDefaultCredentials = true
			};
			Stopwatch watch = Stopwatch.StartNew();
			//var data = YouTube.youtubeDownload(@"https://www.youtube.com/watch?v=StcXp_hP0Ss", proxy);
			var data = YouTube.youtubeDownload(@"https://www.youtube.com/watch?v=UFFtAISWlCc", proxy);
			//Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();
			//data = YouTube.youtubeDownload(@"https://www.youtube.com/watch?v=y6120QOlsfU");
			Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();
			var url = data.Streams["140"].URLs[0];//.First(s => s.Value.Quality.StartsWith("audio")).Value.URLs[0];
			//url = data.Streams.First(s => s.Value.Quality.StartsWith("audio")).Value.URLs[0];
			//Console.WriteLine("Downloading....");
			Downloader.Save(url, "Native Patties.m4a", proxy);
			Console.WriteLine(watch.ElapsedMilliseconds); watch.Restart();
			Console.ReadLine();
			/*using (var client = new HttpClient())
			using (var source = client.GetStreamAsync(url.URL).GetAwaiter().GetResult())
			using (var target = File.Create("Native Patties.m4a")) {
				const int BufferSize = 4096;
				byte[] buffer = new byte[4096];
				long size = url.Size;
				int read;
				do {
					read = source.Read(buffer, 0, (int) Math.Min(size, BufferSize));
					if (read > 0) {
						target.Write(buffer, 0, read);
						size -= read;
					}
				} while (read > 0 && size > 0);
			}*/
			return 0;
		}

	}
}
