using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class EmojiService : BotServiceBase {


		protected override void OnInitialized(ServiceProvider services) {
			Client.GuildUpdated += OnGuildUpdated;
		}

		private async Task OnGuildUpdated(SocketGuild before, SocketGuild after) {
			List<GuildEmote> removedEmotes = new List<GuildEmote>(before.Emotes);
			List<GuildEmote> addedEmotes = new List<GuildEmote>();
			List<RenamedGuildEmote> renamedEmotes = new List<RenamedGuildEmote>();
			foreach (GuildEmote afterEmote in after.Emotes) {
				int beforeIndex = removedEmotes.FindIndex(e => e.Id == afterEmote.Id);
				if (beforeIndex != -1) {
					GuildEmote beforeEmote = removedEmotes[beforeIndex];
					removedEmotes.RemoveAt(beforeIndex);
					if (beforeEmote.Name != afterEmote.Name)
						renamedEmotes.Add(new RenamedGuildEmote(beforeEmote, afterEmote));
				}
				else {
					addedEmotes.Add(afterEmote);
				}
			}

			// Any events left in before, must have been deleted.
		}

		[Flags]
		public enum GuildEmoteChanges {
			None = 0,
			Removed = (1 << 0),
			Added = (1 << 1),
			Renamed = (1 << 2),
		}
		
		public enum GuildEmoteChange {
			Removed,
			Added,
			Renamed,
		}

		public class GuildEmoteChangedEventArgs {
			public SocketGuild Guild { get; }
			public GuildEmote Emote { get; }
			public GuildEmoteChange Change { get; }
			public ulong GuildId => Guild.Id;
			public string Name => Emote.Name;
			public ulong Id => Emote.Id;

			public GuildEmoteChangedEventArgs(SocketGuild guild, GuildEmote emote, GuildEmoteChange change) {
				Guild = guild;
				Emote = emote;
				Change = change;
			}
		}

		public class GuildEmoteRenamedEventArgs : GuildEmoteChangedEventArgs {
			public string OldName { get; }

			public GuildEmoteRenamedEventArgs(SocketGuild guild, GuildEmote oldEmote, GuildEmote newEmote)
				: base(guild, newEmote, GuildEmoteChange.Renamed)
			{
				OldName = oldEmote.Name;
			}
		}

		public class GuildEmoteChangeEventArgs {
			public List<GuildEmote> Removed { get; }
			public List<GuildEmote> Added { get; }
			public List<RenamedGuildEmote> Renamed { get; }

			public GuildEmoteChanges Changes {
				get {
					GuildEmoteChanges changes = GuildEmoteChanges.None;
					if (Renamed.Any()) changes |= GuildEmoteChanges.Removed;
					if (Added.Any())   changes |= GuildEmoteChanges.Added;
					if (Renamed.Any()) changes |= GuildEmoteChanges.Renamed;
					return changes;
				}
			}

			public bool AnyChanges => Changes != GuildEmoteChanges.None;

			public GuildEmoteChangeEventArgs(SocketGuild beforeGuild, SocketGuild afterGuild) {
				Removed = new List<GuildEmote>(beforeGuild.Emotes);
				Added = new List<GuildEmote>();
				Renamed = new List<RenamedGuildEmote>();
				foreach (GuildEmote afterEmote in afterGuild.Emotes) {
					int beforeIndex = Removed.FindIndex(e => e.Id == afterEmote.Id);
					if (beforeIndex != -1) {
						GuildEmote beforeEmote = Removed[beforeIndex];
						Removed.RemoveAt(beforeIndex);
						if (beforeEmote.Name != afterEmote.Name)
							Renamed.Add(new RenamedGuildEmote(beforeEmote, afterEmote));
					}
					else {
						Added.Add(afterEmote);
					}
				}
			}

			public GuildEmoteChangeEventArgs(List<GuildEmote> removed, List<GuildEmote> added, List<RenamedGuildEmote> renamed) {
				Removed = removed ?? new List<GuildEmote>();
				Added = added ?? new List<GuildEmote>();
				Renamed = Renamed ?? new List<RenamedGuildEmote>();
			}
		}

		public class RenamedGuildEmote : IEmote, ISnowflakeEntity, IEntity<ulong> {
			public GuildEmote OldEmote { get; }
			public GuildEmote Emote { get; }

			public string OldName => OldEmote.Name;
			public string Name => Emote.Name;
			public ulong Id => Emote.Id;
			public bool Animated => Emote.Animated;
			public DateTimeOffset CreatedAt => Emote.CreatedAt;
			public string Url => Emote.Url;
			public bool IsManaged => Emote.IsManaged;
			public bool RequireColons => Emote.RequireColons;
			public IReadOnlyList<ulong> RoleIds => Emote.RoleIds;

			public override string ToString() => Emote.ToString();
			public override bool Equals(object other) => Emote.Equals(other);
			public override int GetHashCode() => Emote.GetHashCode();

			public RenamedGuildEmote(GuildEmote oldEmote, GuildEmote newEmote) {
				OldEmote = oldEmote;
				Emote = newEmote;
			}

			public static implicit operator GuildEmote(RenamedGuildEmote emote) {
				return emote.Emote;
			}
		}
	}
}
