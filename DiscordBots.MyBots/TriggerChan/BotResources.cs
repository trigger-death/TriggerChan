using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan {
	public static class BotResources {

		public static readonly string ImagesDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Images");
		public static readonly string GrisaiaDir = Path.Combine(
			ImagesDir, "Grisaia");
		public static readonly string TalkbackDir = Path.Combine(
			ImagesDir, "Talkback");
		public static readonly string AsciifyDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Asciify");
		public static readonly string AudioDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Audio");
		public static readonly string MusicDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Music");

		static BotResources() {
			Directory.CreateDirectory(ImagesDir);
			Directory.CreateDirectory(AsciifyDir);
			Directory.CreateDirectory(AudioDir);
			Directory.CreateDirectory(MusicDir);
		}

		public static readonly string JavaScript = GetImage("JavaScript.png");
		public static readonly string Merge_Conflict = GetImage("Merge_Conflict.png");
		public static readonly string Man_of_Culture = GetImage("Man_of_Culture.png");
		
		public static readonly string Disgusting = GetTalkback("Disgusting.png");
		public static readonly string Dont_Touch_Me_Pervert = GetTalkback("Dont_Touch_Me_Pervert.png");
		public static readonly string[] Well_I_Dont_Love_You = new string[] {
			Disgusting,
			Dont_Touch_Me_Pervert,
		};

		public static readonly string Juicy_Yuuji = GetGrisaia("Juicy_Yuuji.png");
		public static readonly string[] Yuuji = {
			Juicy_Yuuji,
		};
		public static readonly string Amane_Seduced = GetGrisaia("Amane_Seduced.png");
		public static readonly string Amane_Wink = GetGrisaia("Amane_Wink.png");
		public static readonly string[] Amane = {
			Amane_Seduced,
			Amane_Wink,
		};
		public static readonly string Makina_Roger = GetGrisaia("Makina_Roger.png");
		public static readonly string Makina_Thehell = GetGrisaia("Makina_Thehell.png");
		public static readonly string[] Makina = {
			Makina_Roger,
			Makina_Thehell,
		};
		public static readonly string Michiru_Brag = GetGrisaia("Michiru_Brag.png");
		public static readonly string Michiru_Smug = GetGrisaia("Michiru_Smug.png");
		public static readonly string[] Michiru = {
			Michiru_Brag,
			Michiru_Smug,
		};
		public static readonly string Sachi_Glare = GetGrisaia("Sachi_Glare.png");
		public static readonly string Sachi_Scales = GetGrisaia("Sachi_Scales.png");
		public static readonly string[] Sachi = {
			Sachi_Glare,
			Sachi_Scales,
		};
		public static readonly string Yumiko_Cutter = GetGrisaia("Yumiko_Cutter.png");
		public static readonly string[] Yumiko = {
			Yumiko_Cutter,
		};
		public static readonly string[] Grisaia = {
			Juicy_Yuuji,
			Amane_Seduced,
			Amane_Wink,
			Makina_Roger,
			Makina_Thehell,
			Michiru_Brag,
			Michiru_Smug,
			Sachi_Glare,
			Sachi_Scales,
			Yumiko_Cutter,
		};


		public static string GetAsciifyIn(ulong guildId, string ext) {
			return Path.Combine(AsciifyDir, $"Asciify_{guildId}_In{ext}");
		}
		public static string GetAsciifyOut(ulong guildId) {
			return Path.Combine(AsciifyDir, $"Asciify_{guildId}_Out.png");
		}
		public static string GetImage(string filename) {
			return Path.Combine(ImagesDir, filename);
		}
		public static string GetGrisaia(string filename) {
			return Path.Combine(GrisaiaDir, filename);
		}
		public static string GetTalkback(string filename) {
			return Path.Combine(TalkbackDir, filename);
		}

		public static string GetMusic(ulong guildId, string name, string ext) {
			string file = Path.Combine(MusicDir, $"{guildId}");
			Directory.CreateDirectory(file);
			file = Path.Combine(file, name);
			for (char c = 'A'; c < 'J'; c++) {
				string newFile = $"{file}_{c}{ext}";
				if (!File.Exists(newFile))
					return newFile;
				FileStream stream = null;

				try {
					stream = File.OpenWrite(newFile);
				}
				catch (IOException) {
					continue;
				}
				finally {
					if (stream != null)
						stream.Close();
				}
				try {
					File.Delete(newFile);
				}
				catch (IOException) {
					continue;
				}
				return newFile;
			}
			return null;
		}

		/*public static string GetYouTube(ulong guildId, string ext) {
			for (char c = 'A'; c < 'J'; c++) {
				string newFile = $"{file}_{c}{ext}";
				if (!File.Exists(newFile))
					return newFile;
				FileStream stream = null;

				try {
					stream = File.OpenWrite(newFile);
				}
				catch (IOException) {
					continue;
				}
				finally {
					if (stream != null)
						stream.Close();
				}
				try {
					File.Delete(newFile);
				}
				catch (IOException) {
					continue;
				}
				return newFile;
			}
			return null;
		}*/

		public static string GetAudio(string filename) {
			return Path.Combine(AudioDir, filename);
		}


		public static readonly HashSet<string> AudioExtensions = new HashSet<string> {
			".wav", ".mp3", ".m4a"
		};

		public static bool IsValidAudioFile(string path) {
			return Directory.GetFiles(AudioDir, $"{path}", SearchOption.AllDirectories).Any();
		}

		public static List<string> GetAudioFiles() {
			List<string> audioFiles = new List<string>();
			GetDirectoryAudioFiles(audioFiles, AudioDir);
			return audioFiles;
		}

		private static void GetDirectoryAudioFiles(List<string> audioFiles, string directory) {
			string[] files = Directory.GetFiles(directory);
			foreach (string file in files) {
				string ext = Path.GetExtension(file);
				if (AudioExtensions.Contains(ext))
					audioFiles.Add(GetRelativePath(file, AudioDir));
			}
			string[] subdirectories = Directory.GetDirectories(directory);
			foreach (string subdir in subdirectories) {
				GetDirectoryAudioFiles(audioFiles, subdir);
			}
		}

		private static string GetRelativePath(string path, string directory) {
			Uri pathUri = new Uri(path);
			// Folders must end in a slash
			if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				directory += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(directory);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
	}
}
