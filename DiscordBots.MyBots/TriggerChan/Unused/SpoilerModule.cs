using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.SpoilerBot.Modules;
using TriggersTools.DiscordBots.TriggerChan.Database;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public class SpoilerModule : SpoilerModule<TriggerDbContext> {

		public SpoilerModule(DiscordBotServiceContainer services) : base(services) { }
	}
}
