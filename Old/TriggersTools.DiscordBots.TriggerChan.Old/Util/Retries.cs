using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class Retries {

		public static readonly RequestOptions RateLimit = new RequestOptions {
			RetryMode = RetryMode.RetryRatelimit,
		};
	}
}
