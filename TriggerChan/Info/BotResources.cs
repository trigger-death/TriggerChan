﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan.Info {
	public static class BotResources {

		public static readonly string ImagesDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Images");
		public static readonly string AsciifyDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Asciify");
		public static readonly string AudioDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "Audio");
		public static readonly string YouTubeDir = Path.Combine(
			AppContext.BaseDirectory, "Resources", "YouTube");

		static BotResources() {
			Directory.CreateDirectory(ImagesDir);
			Directory.CreateDirectory(AsciifyDir);
			Directory.CreateDirectory(AudioDir);
			Directory.CreateDirectory(YouTubeDir);
		}

		public static readonly string JavaScript = GetImage("JavaScript.png");
		public static readonly string Merge_Conflict = GetImage("Merge_Conflict.png");
		public static readonly string Well_I_Dont_Love_You = GetImage("Well_I_Dont_Love_You.png");
		public static readonly string Man_of_Culture = GetImage("Man_of_Culture.png");


		public static string GetAsciifyIn(ulong guildId, string ext) {
			return Path.Combine(AsciifyDir, $"Asciify_{guildId}_In{ext}");
		}
		public static string GetAsciifyOut(ulong guildId) {
			return Path.Combine(AsciifyDir, $"Asciify_{guildId}_Out.png");
		}
		public static string GetImage(string filename) {
			return Path.Combine(ImagesDir, filename);
		}

		public static string GetYouTube(ulong guildId, string ext) {
			string file = Path.Combine(YouTubeDir, $"{guildId}");
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