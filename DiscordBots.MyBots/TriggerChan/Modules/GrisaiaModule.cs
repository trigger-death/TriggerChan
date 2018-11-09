using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Reactions;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Grisaia Images")]
	[Summary("Commands to post images of Grisaia characters")]
	[IsLockable(true)]
	public class GrisaiaModule : TriggerModule {
		
		public GrisaiaModule(TriggerServiceContainer services) : base(services) { }

		[Command("grisaia")]
		[Summary("An random Grisaia image")]
		public Task GrisaiaRandom() {
			return ReplyFileAsync(Random.Choose(TriggerResources.Grisaia));
		}

		[Group("yuuji"), Alias("juicy yuuji")]
		[Summary("Post an image of Yuuji")]
		[Usage("[thumbsup]")]
		public class YuujiGroup : TriggerModule {

			public YuujiGroup(TriggerServiceContainer services) : base(services) { }

			[Name("yuuji")]
			[Command("")]
			[Example("Post a random image of Yuuji")]
			public Task YuujiRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Yuuji));
			}

			[Name("yuuji thumbsup")]
			[Command("thumbsup")]
			[Example("Post an image of the best character doing the best pose in all of Grisaia")]
			public Task YumikoCutter() {
				return ReplyFileAsync(TriggerResources.JuicyYuuji);
			}
		}
		
		[Group("amane"), Alias("atcher")]
		[Summary("Post an image of Amane")]
		[Usage("[seduced|wink]")]
		public class AmaneGroup : TriggerModule {

			public AmaneGroup(TriggerServiceContainer services) : base(services) { }

			[Name("amane")]
			[Command("")]
			[Example("Post a random image of Amane")]
			public Task AmaneRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Amane));
			}
			[Name("amane seduced")]
			[Command("seduced")]
			[Example("Post an image of Amane while seduced")]
			public Task AmaneSeduced() {
				return ReplyFileAsync(TriggerResources.AmaneSeduced);
			}
			[Name("amane wink")]
			[Command("wink")]
			[Example("Post an image of Amane doing the seducing")]
			public Task AmaneWink() {
				return ReplyFileAsync(TriggerResources.AmaneWink);
			}
		}

		[Group("makina"), Alias("matcher")]
		[Summary("Post an image of Makina")]
		[Usage("[roger|thehell]")]
		public class MakinaGroup : TriggerModule {

			public MakinaGroup(TriggerServiceContainer services) : base(services) { }

			[Name("makina")]
			[Command("")]
			[Example("Post a random image of Makina")]
			public Task MakinaRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Makina));
			}

			[Name("makina roger")]
			[Command("roger")]
			[Example("Post an image of Makina while screaming ROGER")]
			public Task MakinaRoger() {
				return ReplyFileAsync(TriggerResources.MakinaRoger);
			}

			[Name("makina thehell")]
			[Command("thehell")]
			[Example("Post an image of Makina flipping out")]
			public Task MakinaThehell() {
				return ReplyFileAsync(TriggerResources.MakinaThehell);
			}
		}
		
		[Group("michiru"), Alias("chiruchiru", "mitcher")]
		[Summary("Post an image of Michiru")]
		[Usage("[brag|smug]")]
		public class MichiruGroup : TriggerModule {

			public MichiruGroup(TriggerServiceContainer services) : base(services) { }

			[Name("michiru")]
			[Command("")]
			[Example("Post a random image of Makina")]
			public Task MichiruRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Michiru));
			}

			[Name("michiru brag")]
			[Command("brag")]
			[Example("Post an image of Michiru being a knowitall while knowing absolutely nothing")]
			public Task MichiruBrag() {
				return ReplyFileAsync(TriggerResources.MichiruBrag);
			}

			[Name("michiru smug")]
			[Command("smug")]
			[Example("Post an image of Michiru thinking she's pretty clever")]
			public Task MichiruSmug() {
				return ReplyFileAsync(TriggerResources.MichiruSmug);
			}
		}

		[Group("sachi"), Alias("satcher")]
		[Summary("Post an image of Sachi")]
		[Usage("[glare|<scales|eyes>]")]
		public class SachiGroup : TriggerModule {

			public SachiGroup(TriggerServiceContainer services) : base(services) { }

			[Name("sachi")]
			[Command("")]
			[Example("Post a random image of Sachi")]
			public Task SachiRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Sachi));
			}
			
			[Name("sachi glare")]
			[Command("glare")]
			[Example("Post an image of Sachi glaring at you")]
			public Task SachiGlare() {
				return ReplyFileAsync(TriggerResources.SachiGlare);
			}

			[Name("sachi scales")]
			[Command("scales"), Alias("eyes")]
			[Example("Post an image where \"The scales have fallen from [Sachi's] eyes\"")]
			public Task SachiScales() {
				return ReplyFileAsync(TriggerResources.SachiScales);
			}
		}

		[Group("yumiko"), Alias("yumichin")]
		[Summary("Post an image of Sachi")]
		[Usage("[<boxcutter|cutter>]")]
		public class YumikoGroup : TriggerModule {

			public YumikoGroup(TriggerServiceContainer services) : base(services) { }

			[Name("yumiko")]
			[Command("")]
			[Example("Post a random image of Yumiko")]
			public Task YumikoRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Yumiko));
			}

			[Name("yumiko boxcutter")]
			[Command("boxcutter"), Alias("cutter")]
			[Example("Post an image of Yumiko threatening you with a box cutter")]
			public Task YumikoCutter() {
				return ReplyFileAsync(TriggerResources.YumikoCutter);
			}
		}
	}
}
