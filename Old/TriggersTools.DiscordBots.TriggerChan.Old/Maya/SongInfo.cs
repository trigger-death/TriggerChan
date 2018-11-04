using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Maya.Music.Youtube {
	public class SongInfo {

		public string Title { get; set; }

		public TimeSpan Duration { get; set; }

		public string FileName { get; set; }

		public bool IsTemporary { get; set; }

		public IUser Owner { get; set; }

		public ITextChannel Channel { get; set; }

		public void GetLocalDetails() {
			ProcessStartInfo start = new ProcessStartInfo() {
				FileName = "ffprobe",
				Arguments = $"-v error -print_format json=compact=1 -show_format \"{FileName}",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				StandardOutputEncoding = Encoding.UTF8,
			};
			Process ffprobe = Process.Start(start);
			ffprobe.WaitForExit();
			JObject json = JObject.Parse(ffprobe.StandardOutput.ReadToEnd());
			try {
				Title = json["format"]["tags"]["title"]?.ToString() ?? Title;
			}
			catch { }
			try {
				Duration = TimeSpan.FromSeconds(double.Parse(json["format"]["duration"]?.ToString() ?? "0"));
			}
			catch { }
		}
	}
}
