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

		/*[Command("grisaia")]
		[Summary("An random Grisaia image")]
		public Task GrisaiaRandom() {
			return ReplyFileAsync(Random.Choose(TriggerResources.Grisaia));
		}*/
		[Group("grisaia")]
		[Summary("Post an image from Grisaia")]
		[Usage("[snoop]")]
		public class GrisaiaGroup : TriggerModule {

			public GrisaiaGroup(TriggerServiceContainer services) : base(services) { }

			[Name("grisaia")]
			[Command("")]
			[Example("Post a random image of Yumiko")]
			public Task GrisaiaRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Grisaia));
			}
			[Name("grisaia snoop")]
			[Command("snoop"), Alias("peep", "follow")]
			[Example("Post an image of the Grisaia Heroines snooping around")]
			public Task GrisaiaPeek() {
				return ReplyFileAsync(TriggerResources.GrisaiaPeek);
			}
			[Name("grisaia deletethis")]
			[Command("deletethis"), Alias("deletthis")]
			[Example("Post a random image of a Grisaia character threatening someone to delete this RIGHT NOW")]
			public Task GrisaiaDeleteThis() {
				return ReplyFileAsync(Random.Choose(TriggerResources.DeleteThis));
			}
		}

		[Group("yuuji"), Alias("juicy yuuji", "best girl")]
		[Summary("Post an image of Best Girl")]
		[Usage("[thumbsup|bully [amane|chizuru|machiru]|defense|deletethis|scarred|wobble]")]
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
			public Task YuujiThumbsUp() {
				return ReplyFileAsync(TriggerResources.YuujiThumbsUp);
			}
			[Name("yuuji scarred")]
			[Command("scarred")]
			[Example("Post an image of Yuuji being scarred for life")]
			public Task YuujiScarred() {
				return ReplyFileAsync(TriggerResources.YuujiScarred);
			}
			[Name("yuuji deletethis")]
			[Command("deletethis"), Alias("deletthis")]
			[Example("Post an image of Yuuji threatening someone to delete this RIGHT NOW")]
			public Task YuujiDeleteThis() {
				return ReplyFileAsync(TriggerResources.YuujiDeleteThis);
			}
			[Name("yuuji defense")]
			[Command("defense"), Alias("national", "national defense")]
			[Example("Post a gif of Yuuji... *dancing*")]
			public Task YuujiDefense() {
				return ReplyFileAsync(TriggerResources.YuujiDefense);
			}
			[Name("yuuji wobble")]
			[Command("wobble")]
			[Example("Post a gif of Yuuji... *dancing*")]
			public Task YuujiWobble() {
				return ReplyFileAsync(TriggerResources.YuujiWobble);
			}
			[Name("yuuji bully")]
			[Command("bully")]
			[Example("Post a random picture of Yuuji bullying someone")]
			public Task YuujiBullyRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.YuujiBully));
			}
			[Name("yuuji bully amane")]
			[Command("bully amane")]
			[Example("Post an image of Yuuji bullying Amane")]
			public Task YuujiBullyAmane() {
				return ReplyFileAsync(TriggerResources.YuujiBullyAmane);
			}
			[Name("yuuji bully chizuru")]
			[Command("bully chizuru")]
			[Example("Post an image of Yuuji bullying Chizuru")]
			public Task YuujiBullyChizuru() {
				return ReplyFileAsync(TriggerResources.YuujiBullyChizuru);
			}
			[Name("yuuji bully michiru")]
			[Command("bully michiru"), Alias("bully chiruchiru")]
			[Example("Post an image of Yuuji bullying Michiru")]
			public Task YuujiBullyMichiru() {
				return ReplyFileAsync(TriggerResources.YuujiBullyMichiru);
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
		[Usage("[roger|thehell|victory]")]
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
			[Name("makina victory")]
			[Command("victory")]
			[Example("Post an image of Makina triumphing over all odds")]
			public Task MakinaVictory() {
				return ReplyFileAsync(TriggerResources.MakinaVictory);
			}
		}
		
		[Group("michiru"), Alias("chiruchiru", "mitcher")]
		[Summary("Post an image of Michiru")]
		[Usage("[brag|smug|happy|fainted]")]
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
			[Name("michiru happy")]
			[Command("happy")]
			[Example("Post an image of happy Michiru :)")]
			public Task MichiruHappy() {
				return ReplyFileAsync(TriggerResources.MichiruHappy);
			}
			[Name("michiru fainted")]
			[Command("fainted")]
			[Example("Post an image of Michiru failing to stay alive :(")]
			public Task MichiruFainted() {
				return ReplyFileAsync(TriggerResources.MichiruFainted);
			}
		}

		[Group("sachi"), Alias("satcher")]
		[Summary("Post an image of Sachi")]
		[Usage("[glare|scales|pat|murder]")]
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
			[Name("sachi pat")]
			[Command("pat"), Alias("pats", "headpat", "headpats")]
			[Example("Post an image of Sachi receiving premium headpats")]
			public Task SachiPat() {
				return ReplyFileAsync(TriggerResources.SachiPat);
			}
			[Name("sachi murder")]
			[Command("murder")]
			[Example("Post an image of Sachi plotting a friendlt neighborhood assasination")]
			public Task SachiMurder() {
				return ReplyFileAsync(TriggerResources.SachiMurder);
			}
		}

		[Group("yumiko"), Alias("yumichin")]
		[Summary("Post an image of Sachi")]
		[Usage("[boxcutter]")]
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
				return ReplyFileAsync(TriggerResources.YumikoBoxcutter);
			}
		}

		[Group("kazuki")]
		[Summary("Post an image of Kazuki, Best Girl's sister")]
		[Usage("[angry|ohwell|deletethis]")]
		public class KazukiGroup : TriggerModule {

			public KazukiGroup(TriggerServiceContainer services) : base(services) { }

			[Name("kazuki")]
			[Command("")]
			[Example("Post a random image of Kazuki")]
			public Task KazukiRandom() {
				return ReplyFileAsync(Random.Choose(TriggerResources.Kazuki));
			}
			[Name("kazuki angry")]
			[Command("angry")]
			[Example("Post an image of Kazuki looking down on all of us")]
			public Task KazukiAngry() {
				return ReplyFileAsync(TriggerResources.KazukiAngry);
			}
			[Name("kazuki angry")]
			[Command("angry")]
			[Example("Post an image of Kazuki sighing and giving up with your shear stupidity")]
			public Task KazukiOhWell() {
				return ReplyFileAsync(TriggerResources.KazukiOhWell);
			}
			[Name("kazuki deletethis")]
			[Command("deletethis"), Alias("deletthis")]
			[Example("Post an image of Kazuki threatening someone to delete this RIGHT NOW")]
			public Task KazukiDeleteThis() {
				return ReplyFileAsync(TriggerResources.KazukiDeleteThis);
			}
		}
	}
}
