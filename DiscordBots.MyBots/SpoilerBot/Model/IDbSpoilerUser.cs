using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {
	public interface IDbSpoilerUser {
		List<Spoiler> Spoilers { get; set; }
		List<SpoiledUser> SpoiledUsers { get; set; }
	}
}
