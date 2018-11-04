using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Victoria.Objects;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	public class LavaTrackOwner {
		/// <summary>
		/// Gets the owner of who queued this item. 
		/// </summary>
		public IUser User { get; }
		/// <summary>
		/// Gets the channel this item was queued in. 
		/// </summary>
		public ITextChannel Channel { get; }
		/// <summary>
		/// Gets the contained Lavalink track.
		/// </summary>
		public LavaTrack Track { get; }
		/// <summary>
		/// Gets the position of the track in the player.
		/// </summary>
		public TimeSpan Position => Track.Position;
		/// <summary>
		/// Gets the length of the track.
		/// </summary>
		public TimeSpan Length => Track.Length;

		/// <summary>
		/// Casts the <see cref="LavaTrackOwner"/> to a <see cref="LavaTrack"/>.
		/// </summary>
		public static implicit operator LavaTrack(LavaTrackOwner track) {
			return track.Track;
		}

		public LavaTrackOwner(ICommandContext context, LavaTrack track) {
			User = context.User;
			Channel = (ITextChannel) context.Channel;
			Track = track;
		}
	}
}
