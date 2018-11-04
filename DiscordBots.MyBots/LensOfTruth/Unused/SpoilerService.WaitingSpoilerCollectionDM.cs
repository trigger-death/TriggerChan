using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService {
		private class WaitingSpoilerCollectionDM {

			private readonly ConcurrentDictionary<ulong, ParsedSpoiler> users;

			public WaitingSpoilerCollectionDM() {
				users = new ConcurrentDictionary<ulong, ParsedSpoiler>();
			}

			public bool Add(ParsedSpoiler spoiler) {
				/*if (!users.TryGetValue(spoiler.User.Id, out var spoiler)) {
					users = new ConcurrentDictionary<ulong, ParsedSpoiler>();
				}*/
				if (users.TryRemove(spoiler.User.Id, out var oldSpoiler))
					oldSpoiler.ExpireTimer?.Dispose();
				if (users.TryAdd(spoiler.User.Id, spoiler)) {
					spoiler.ExpireTimer = new Timer(OnExpire, spoiler, TimeSpan.FromMinutes(3), TimeSpan.FromMilliseconds(-1));
					return true;
				}
				return false;
			}

			private void OnExpire(object state) {
				ParsedSpoiler spoiler = (ParsedSpoiler) state;
				Remove(spoiler.User.Id);
				Expired?.Invoke(spoiler);
			}

			public ParsedSpoiler Get(ulong userId) {
				users.TryGetValue(userId, out var spoiler);
				return spoiler;
			}

			public ParsedSpoiler Remove(ulong userId) {
				if (users.TryRemove(userId, out var spoiler))
					spoiler.ExpireTimer?.Dispose();
				return spoiler;
			}

			public event Action<ParsedSpoiler> Expired;
		}
	}
}
