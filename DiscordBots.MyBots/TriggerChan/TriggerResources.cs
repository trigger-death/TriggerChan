using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan {
	public static class TriggerResources {

		/*public static readonly string ImagesDir = Path.Combine(
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
			AppContext.BaseDirectory, "Resources", "Music");*/

		public static readonly string ResourcesDir = Path.Combine(AppContext.BaseDirectory, "Resources");
		public static readonly string ImagesDir = Path.Combine(ResourcesDir, "Images");
		public static readonly string GrisaiaImagesDir = Path.Combine(ImagesDir, "Grisaia");
		public static readonly string OcarinaImagesDir = Path.Combine(ImagesDir, "Ocarina");
		public static readonly string AsciifyImagesDir = Path.Combine(ImagesDir, "Asciify");
		public static readonly string TalkbackImagesDir = Path.Combine(ImagesDir, "Talkback");
		public static readonly string TempImagesDir = Path.Combine(ImagesDir, "Temp");

		static TriggerResources() {
			Directory.CreateDirectory(ImagesDir);
			Directory.CreateDirectory(AsciifyImagesDir);
			Directory.CreateDirectory(TempImagesDir);
		}

		public static readonly string JavaScript = GetImage("JavaScript.png");
		public static readonly string MergeConflict = GetImage("Merge_Conflict.png");
		public static readonly string ManOfCulture = GetImage("Man_of_Culture.png");
		
		public static readonly string Disgusting = GetTalkback("Disgusting.png");
		public static readonly string DontTouchMePervert = GetTalkback("Dont_Touch_Me_Pervert.png");
		public static readonly string[] TalkbackWellIDontLoveYou = new string[] {
			Disgusting,
			DontTouchMePervert,
		};

		public static readonly string JuicyYuuji = GetGrisaia("Juicy_Yuuji.png");
		public static readonly string[] Yuuji = {
			JuicyYuuji,
		};
		public static readonly string AmaneSeduced = GetGrisaia("Amane_Seduced.png");
		public static readonly string AmaneWink = GetGrisaia("Amane_Wink.png");
		public static readonly string[] Amane = {
			AmaneSeduced,
			AmaneWink,
		};
		public static readonly string MakinaRoger = GetGrisaia("Makina_Roger.png");
		public static readonly string MakinaThehell = GetGrisaia("Makina_Thehell.png");
		public static readonly string[] Makina = {
			MakinaRoger,
			MakinaThehell,
		};
		public static readonly string MichiruBrag = GetGrisaia("Michiru_Brag.png");
		public static readonly string MichiruSmug = GetGrisaia("Michiru_Smug.png");
		public static readonly string[] Michiru = {
			MichiruBrag,
			MichiruSmug,
		};
		public static readonly string SachiGlare = GetGrisaia("Sachi_Glare.png");
		public static readonly string SachiScales = GetGrisaia("Sachi_Scales.png");
		public static readonly string[] Sachi = {
			SachiGlare,
			SachiScales,
		};
		public static readonly string YumikoCutter = GetGrisaia("Yumiko_Cutter.png");
		public static readonly string[] Yumiko = {
			YumikoCutter,
		};
		public static readonly string[] Grisaia = {
			JuicyYuuji,
			AmaneSeduced,
			AmaneWink,
			MakinaRoger,
			MakinaThehell,
			MichiruBrag,
			MichiruSmug,
			SachiGlare,
			SachiScales,
			YumikoCutter,
		};


		public static string GetAsciifyIn(ulong guildId, string ext) {
			return Path.Combine(AsciifyImagesDir, $"Asciify_{guildId}_In{ext}");
		}
		public static string GetAsciifyOut(ulong guildId) {
			return Path.Combine(AsciifyImagesDir, $"Asciify_{guildId}_Out.png");
		}
		public static string GetImage(string filename) {
			return Path.Combine(ImagesDir, filename);
		}
		public static string GetGrisaia(string filename) {
			return Path.Combine(GrisaiaImagesDir, filename);
		}
		public static string GetTalkback(string filename) {
			return Path.Combine(TalkbackImagesDir, filename);
		}
	}
}
