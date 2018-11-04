using Discord;
using Discord.WebSocket;

namespace TriggersTools.DiscordBots.TriggerChan.Custom {
	/// <summary>
	/// The type of change that occurred to a guild emote.
	/// </summary>
	public enum EmoteChange {
		/// <summary>
		/// No change occurred.
		/// </summary>
		None = 0,
		/// <summary>
		/// The emote was removed from the list.
		/// </summary>
		Removed,
		/// <summary>
		/// The emote was added from the list.
		/// </summary>
		Added,
		/// <summary>
		/// The guild emote was renamed.
		/// </summary>
		Renamed,
	}

	/// <summary>
	/// Event arguments for when a guild emote has changed in some way.
	/// </summary>
	public class EmoteChangedEventArgs {

		#region Fields

		/// <summary>
		/// The guild containing this emote.
		/// </summary>
		public SocketGuild Guild { get; }
		/// <summary>
		/// The changed emote.
		/// </summary>
		public GuildEmote Emote { get; }
		/// <summary>
		/// The type of change that occurred.
		/// </summary>
		public EmoteChange Change { get; }

		#endregion

		#region Properties

		/// <summary>
		/// The guild Id.
		/// </summary>
		public ulong GuildId => Guild.Id;
		/// <summary>
		/// The emote name.
		/// </summary>
		public string Name => Emote.Name;
		/// <summary>
		/// The emote Id.
		/// </summary>
		public ulong Id => Emote.Id;

		#endregion

		#region Constructors

		internal EmoteChangedEventArgs(SocketGuild guild, GuildEmote emote, EmoteChange change) {
			Guild = guild;
			Emote = emote;
			Change = change;
		}

		#endregion
	}

	/// <summary>
	/// Event arguments for when a guild emote has been renamed.
	/// </summary>
	public class EmoteRenamedEventArgs : EmoteChangedEventArgs {

		#region Fields
		
		/// <summary>
		/// The old name of the emote before the rename.
		/// </summary>
		public string OldName { get; }

		#endregion

		#region Constructors

		internal EmoteRenamedEventArgs(SocketGuild guild, GuildEmote oldEmote, GuildEmote newEmote)
			: base(guild, newEmote, EmoteChange.Renamed)
		{
			OldName = oldEmote.Name;
		}

		#endregion
	}

	/// <summary>
	/// A delegate for when a guild emote has been changed in some way.
	/// </summary>
	/// <param name="e">The event args.</param>
	public delegate void EmoteChangedEventHandler(EmoteChangedEventArgs e);

	/// <summary>
	/// A delegate for when a guild emote has been renamed.
	/// </summary>
	/// <param name="e">The event args.</param>
	public delegate void EmoteRenamedEventHandler(EmoteRenamedEventArgs e);
}
