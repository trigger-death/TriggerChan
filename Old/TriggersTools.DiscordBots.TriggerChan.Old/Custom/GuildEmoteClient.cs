using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TriggersTools.DiscordBots.TriggerChan.Custom {
	public class GuildEmoteClient {

		public GuildEmoteClient(DiscordSocketClient client) {
			client.GuildUpdated += OnGuildUpdated;
		}

		private Task OnGuildUpdated(SocketGuild before, SocketGuild after) {
			List<GuildEmote> removed = new List<GuildEmote>(before.Emotes);
			foreach (GuildEmote afterEmote in after.Emotes) {
				int beforeIndex = removed.FindIndex(e => e.Id == afterEmote.Id);
				if (beforeIndex != -1) {
					GuildEmote beforeEmote = removed[beforeIndex];

					// Remove from list so that it is not registered as "removed"
					removed.RemoveAt(beforeIndex);

					// Guild emote was renamed
					if (beforeEmote.Name != afterEmote.Name) {
						Renamed?.Invoke(new EmoteRenamedEventArgs(after, beforeEmote, afterEmote));
					}
				}
				else {
					// Guild emote was added
					Added?.Invoke(new EmoteChangedEventArgs(after, afterEmote, EmoteChange.Added));
				}
			}

			// Guild emotes were removed
			foreach (GuildEmote beforeEmote in removed) {
				Added?.Invoke(new EmoteChangedEventArgs(after, beforeEmote, EmoteChange.Removed));
			}
			return Task.FromResult<object>(null);
		}

		#region Events

		#region Changed

		/// <summary>
		/// Raised when a build emote has been added.
		/// </summary>
		public event EmoteChangedEventHandler Added;
		/// <summary>
		/// Raised when a build emote has been removed.
		/// </summary>
		public event EmoteChangedEventHandler Removed;
		/// <summary>
		/// Raised when a build emote has been renamed.
		/// </summary>
		public event EmoteRenamedEventHandler Renamed;

		#endregion

		#endregion
	}
}
