using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Services;
using TriggersTools.DiscordBots.TriggerChan.Database;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class TriggerSpoilerService : SpoilerServiceBase {

		public TriggerSpoilerService(DiscordBotServiceContainer services) : base(services) { }

		protected override SpoilerDbContext GetDb() => GetDb<TriggerDbContext>();
	}
}
