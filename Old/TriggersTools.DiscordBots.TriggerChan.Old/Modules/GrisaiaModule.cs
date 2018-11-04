using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Grisaia Images")]
	[IsLockable]
	public class GrisaiaModule : BotModuleBase {

		[Command("grisaia")]
		[Summary("An random Grisaia image macro")]
		public async Task GrisaiaRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Grisaia));
		}

		[Alias("yuuji"), Command("juicy yuuji")]
		[Summary("An image macro of the best character doing the best pose in all of Grisaia")]
		public async Task JuicyYuuji() {
			await Context.Channel.SendFileAsync(BotResources.Juicy_Yuuji);
		}

		[Command("amane"), Alias("atcher")]
		[Summary("A random image macro of Amane")]
		public async Task AmaneRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Amane));
		}

		[Command("amane seduced"), Alias("atcher seduced")]
		[Summary("An image macro of seduced while Amane")]
		public async Task AmaneSeduced() {
			await Context.Channel.SendFileAsync(BotResources.Amane_Seduced);
		}

		[Command("amane wink"), Alias("atcher wink")]
		[Summary("An image macro of Amane doing the seducing")]
		public async Task AmaneWink() {
			await Context.Channel.SendFileAsync(BotResources.Amane_Wink);
		}

		[Command("makina"), Alias("matcher")]
		[Summary("A random image macro of Makina")]
		public async Task MakinaRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Makina));
		}

		[Command("makina roger"), Alias("matcher roger")]
		[Summary("An image macro of Makina while screaming ROGER")]
		public async Task MakinaRoger() {
			await Context.Channel.SendFileAsync(BotResources.Makina_Roger);
		}

		[Command("makina thehell"), Alias("matcher thehell")]
		[Summary("An image macro of Makina flipping out")]
		public async Task MakinaThehell() {
			await Context.Channel.SendFileAsync(BotResources.Makina_Thehell);
		}

		[Command("michiru"), Alias("chiruchiru")]
		[Summary("A random image macro of Michiru")]
		public async Task MichiruRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Michiru));
		}

		[Command("michiru brag"), Alias("chiruchiru brag", "mitcher brag")]
		[Summary("An image macro of Michiru being a knowitall while knowing absolutely nothing")]
		public async Task MichiruBrag() {
			await Context.Channel.SendFileAsync(BotResources.Michiru_Brag);
		}

		[Command("michiru smug"), Alias("chiruchiru smug", "mitcher smug")]
		[Summary("An image macro of Michiru thinking she's pretty clever")]
		public async Task MichiruSmug() {
			await Context.Channel.SendFileAsync(BotResources.Michiru_Smug);
		}

		[Command("sachi"), Alias("satcher")]
		[Summary("A random image macro of Sachi")]
		public async Task SachiRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Sachi));
		}

		[Command("sachi glare"), Alias("satcher glare")]
		[Summary("An image macro of Sachi glaring at you")]
		public async Task SachiGlare() {
			await Context.Channel.SendFileAsync(BotResources.Sachi_Glare);
		}

		[Command("sachi scales"), Alias("sachi eyes", "satcher scales", "satcher eyes")]
		[Summary("An image macro where \"The scales have fallen from [Sachi's] eyes\"")]
		public async Task SachiScales() {
			await Context.Channel.SendFileAsync(BotResources.Sachi_Scales);
		}

		[Command("yumiko"), Alias("yumichin")]
		[Summary("A random image macro of Yumiko")]
		public async Task YumikoRandom() {
			await Context.Channel.SendFileAsync(Random.Choose(BotResources.Yumiko));
		}

		[Command("yumiko cutter"), Alias("yumiko boxcutter", "yumichin cutter", "yumichin boxcutter")]
		[Summary("An image macro of Sachi glaring at you")]
		public async Task YumikoCutter() {
			await Context.Channel.SendFileAsync(BotResources.Yumiko_Cutter);
		}
	}
}
