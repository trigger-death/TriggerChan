using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService {
		private class WaitingSpoilerCollection {

			private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ParsedSpoiler>> channels;

			public WaitingSpoilerCollection() {
				channels = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ParsedSpoiler>>();
			}

			public bool Add(ParsedSpoiler spoiler) {
				if (!channels.TryGetValue(spoiler.Channel.Id, out var users)) {
					users = new ConcurrentDictionary<ulong, ParsedSpoiler>();
					if (!channels.TryAdd(spoiler.Channel.Id, users))
						users = channels[spoiler.Channel.Id];
				}
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
				Remove(spoiler.Channel.Id, spoiler.User.Id);
				Expired?.Invoke(spoiler);
			}

			public ParsedSpoiler Get(ulong channelId, ulong userId) {
				if (!channels.TryGetValue(channelId, out var users))
					return null;
				users.TryGetValue(userId, out var spoiler);
				return spoiler;
			}

			public ParsedSpoiler Remove(ulong channelId, ulong userId) {
				if (!channels.TryGetValue(channelId, out var users))
					return null;
				users.TryGetValue(userId, out var spoiler);
				if (users.TryRemove(userId, out spoiler))
					spoiler.ExpireTimer?.Dispose();
				if (users.Count == 0)
					channels.TryRemove(userId, out _);
				return spoiler;
			}

			public event Action<ParsedSpoiler> Expired;
		}
	}
}
