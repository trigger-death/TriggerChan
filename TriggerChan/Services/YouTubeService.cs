using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class YouTubeService : BotServiceBase {

		private readonly Regex YouTubeRegex;
		private readonly Regex YouTubeCodeRegex;

		public YouTubeService() {
			const string pattern = @"^https?:\/\/\w*.?(youtube\.com|youtu\.be)";
			RegexOptions options = RegexOptions.IgnoreCase;
			YouTubeRegex = new Regex(pattern, options);
			RegexOptions optionsCode = RegexOptions.IgnoreCase;// | RegexOptions.ExplicitCapture;
			const string patternCode = @"(?:https?:\/\/\w*.?(?:youtube\.com\/watch\?.*v=|youtu\.be\/))((\-|\w)+?)(?:$|&)";
			YouTubeCodeRegex = new Regex(patternCode, optionsCode);
		}


		public bool IsYouTubeUrl(string url, out string videoCode) {
			Match match = YouTubeCodeRegex.Match(url);
			bool success = match.Success && match.Groups.Count >= 2;
			if (success)
				videoCode = match.Groups[1].Value;
			else
				videoCode = null;
			return success;
		}

		public async Task<string> Download(SocketCommandContext context, string url, string ext) {
			//TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

			LocalGuild guild = Settings.GetLocalGuild(context.Guild.Id);

			string file = BotResources.GetYouTube(context.Guild.Id, ".mp4");// ext);
			Console.WriteLine(file);
			
			Process youtubedl;
			lock (guild) {
				if (!(guild.YouTubeProcess?.HasExited ?? true)) {
					guild.YouTubeProcess.Kill();
					guild.YouTubeProcess = null;
				}

				ProcessStartInfo youtubedlDownload = new ProcessStartInfo() {
					FileName = "youtube-dl",
					//Arguments = $"-x --abort-on-error --audio-format mp3 -o \"{file.Replace(".mp3", ".%(ext)s")}\" {url}",
					Arguments = $"-v --format best -o \"{file}\" {url}",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					/*UseShellExecute = false*/     //Linux?
				};
				youtubedl = Process.Start(youtubedlDownload);
				guild.YouTubeProcess = youtubedl;
			}

			//new Thread(() => {

				//Wait until download is finished
			youtubedl.WaitForExit();
			Thread.Sleep(500);
			if (File.Exists(file)) {
				/*string lines = youtubedl.StandardOutput.ReadToEnd();

				const string Header = "[download] Destination: ";

				string headerWGuildId = $"{Header}{guild.Id}";

				int start = lines.IndexOf(headerWGuildId);
				if (start != -1) {
					int end = lines.IndexOf('\n', start + headerWGuildId.Length);
					if (end != -1) {
						start += Header.Length;
						file = lines.Substring(start, end - start);
						Console.WriteLine(file);
						return file;
					}
				}*/

				//Return MP3 Path & Video Title
				return file;
			}
			else {
				//Error downloading
				//tcs.SetResult(null);
				await context.Channel.SendMessageAsync("Failed to download video");

				return null;
				//MusicBot.Print($"Could not download Song, youtube-dl responded with:\n\r{youtubedl.StandardOutput.ReadToEnd()}", ConsoleColor.Red);
			}
			/*//}).Start();

			string result = await tcs.Task;
			//if (result == null)
			//	throw new Exception("youtube-dl.exe failed to download!");

			//Remove \n at end of Line
			result = result.Replace("\n", "").Replace(Environment.NewLine, "");

			return result;*/
		}

		public Tuple<string, string, string> GetInfo(SocketCommandContext context, string url) {
			string title;
			string duration;
			string extension;

			LocalGuild guild = Settings.GetLocalGuild(context.Guild.Id);
			
			Process youtubedl;
			lock (guild) {
				if (!(guild.YouTubeProcess?.HasExited ?? true)) {
					guild.YouTubeProcess.Kill();
					guild.YouTubeProcess = null;
				}

				ProcessStartInfo youtubedlGetTitle = new ProcessStartInfo() {
					FileName = "youtube-dl",
					Arguments = $"-s -e --get-filename --get-duration -o \"output.%(ext)s\" {url}",
					RedirectStandardOutput = true,
					//UseShellExecute = false,
					/*UseShellExecute = false*/     //Linux?
				};
				youtubedl = Process.Start(youtubedlGetTitle);
				guild.YouTubeProcess = youtubedl;
			}

			//Get Video Title
			youtubedl.WaitForExit();
			//Read Title
			string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');

			if (lines.Length >= 2) {
				title = lines[0];
				extension = Path.GetExtension(lines[1]);
				duration = lines[2];
			}
			else {
				title = "No Title found";
				extension = ".mp4";
				duration = "0:00:00";
			}

			return new Tuple<string, string, string>(title, duration, extension);
		}
	}
}
