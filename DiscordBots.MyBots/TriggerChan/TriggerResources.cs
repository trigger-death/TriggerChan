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
		public static readonly string TempImagesDir = Path.Combine(AppContext.BaseDirectory, ".config", "tmp", "images");

		static TriggerResources() {
			Directory.CreateDirectory(ImagesDir);
			Directory.CreateDirectory(AsciifyImagesDir);
			Directory.CreateDirectory(TempImagesDir);
		}

		public static readonly string Tasts = GetImage("tasts.png");
		public static readonly string JavaScript = GetImage("javaScript.png");
		public static readonly string MergeConflict = GetImage("merge_conflict.png");
		public static readonly string ManOfCulture = GetImage("man_of_culture.png");
		public static readonly string ModAbuse = GetImage("mod_abuse.jpg");
		public static readonly string NoNoNo = GetImage("nonono.gif");
		public static readonly string Shock = GetImage("shock.gif");
		public static readonly string Swat = GetImage("swat.gif");

		public static readonly string Disgusting = GetTalkback("disgusting.png");
		public static readonly string DontTouchMePervert = GetTalkback("dont_touch_me_pervert.png");
		public static readonly string DontTouchMePervertRare = GetTalkback("dont_touch_me_pervert_rare.png");
		public static readonly string[] TalkbackWellIDontLoveYou = new string[] {
			Disgusting,
			DontTouchMePervert,
		};
		public static readonly string[] TalkbackWellIDontLoveYouRare = new string[] {
			DontTouchMePervertRare,
		};

		public static readonly string YuujiThumbsUp = GetGrisaia("yuuji_thumbsup.png");
		public static readonly string YuujiDeleteThis = GetGrisaia("yuuji_deletethis.png");
		public static readonly string YuujiBullyAmane = GetGrisaia("yuuji_bully_amane.png");
		public static readonly string YuujiBullyMichiru = GetGrisaia("yuuji_bully_michiru.png");
		public static readonly string YuujiBullyChizuru = GetGrisaia("yuuji_bully_chizuru.png");
		public static readonly string YuujiScarred = GetGrisaia("yuuji_scarred.png");
		public static readonly string YuujiWobble = GetGrisaia("yuuji_wobble.gif");
		public static readonly string YuujiDefense = GetGrisaia("yuuji_national_defense.png");
		public static readonly string[] Yuuji = {
			YuujiThumbsUp,
			YuujiDeleteThis,
			YuujiBullyAmane,
			YuujiBullyMichiru,
			YuujiBullyChizuru,
			YuujiScarred,
			YuujiWobble,
			YuujiDefense,
		};
		public static readonly string[] YuujiBully = {
			YuujiBullyAmane,
			YuujiBullyMichiru,
			YuujiBullyChizuru,
		};
		public static readonly string KazukiOhWell = GetGrisaia("kazuki_ohwell.png");
		public static readonly string KazukiAngry = GetGrisaia("kazuki_angry.png");
		public static readonly string KazukiDeleteThis = GetGrisaia("kazuki_deletethis.png");
		public static readonly string[] Kazuki = {
			KazukiOhWell,
			KazukiAngry,
			KazukiDeleteThis,
		};
		public static readonly string[] DeleteThis = {
			YuujiDeleteThis,
			KazukiDeleteThis,
		};
		public static readonly string AmaneSeduced = GetGrisaia("amane_seduced.png");
		public static readonly string AmaneWink = GetGrisaia("amane_wink.png");
		public static readonly string[] Amane = {
			AmaneSeduced,
			AmaneWink,
		};
		public static readonly string MakinaRoger = GetGrisaia("makina_roger.png");
		public static readonly string MakinaThehell = GetGrisaia("makina_thehell.png");
		public static readonly string MakinaVictory = GetGrisaia("makina_victory.png");
		public static readonly string[] Makina = {
			MakinaRoger,
			MakinaThehell,
			MakinaVictory,
		};
		public static readonly string MichiruBrag = GetGrisaia("michiru_brag.png");
		public static readonly string MichiruSmug = GetGrisaia("michiru_smug.png");
		public static readonly string MichiruHappy = GetGrisaia("michiru_happy.png");
		public static readonly string MichiruFainted = GetGrisaia("michiru_fainted.png");
		public static readonly string[] Michiru = {
			MichiruBrag,
			MichiruSmug,
			MichiruHappy,
			MichiruFainted,
		};
		public static readonly string SachiGlare = GetGrisaia("sachi_glare.png");
		public static readonly string SachiScales = GetGrisaia("sachi_scales.png");
		public static readonly string SachiPat = GetGrisaia("sachi_pat.png");
		public static readonly string SachiMurder = GetGrisaia("sachi_murder.png");
		public static readonly string[] Sachi = {
			SachiGlare,
			SachiScales,
			SachiPat,
			SachiMurder,
		};
		public static readonly string YumikoBoxcutter = GetGrisaia("yumiko_boxcutter.png");
		public static readonly string[] Yumiko = {
			YumikoBoxcutter,
		};
		public static readonly string GrisaiaPeek = GetGrisaia("grisaia_peek.png");
		public static readonly string[] Grisaia = {
			GrisaiaPeek,
			KazukiOhWell,
			KazukiAngry,
			KazukiDeleteThis,
			YuujiThumbsUp,
			YuujiDeleteThis,
			YuujiBullyAmane,
			YuujiBullyMichiru,
			YuujiBullyChizuru,
			YuujiScarred,
			YuujiWobble,
			YuujiDefense,
			AmaneSeduced,
			AmaneWink,
			MakinaRoger,
			MakinaThehell,
			MakinaVictory,
			MichiruBrag,
			MichiruSmug,
			MichiruHappy,
			MichiruFainted,
			SachiGlare,
			SachiScales,
			SachiPat,
			SachiMurder,
			YumikoBoxcutter,
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
