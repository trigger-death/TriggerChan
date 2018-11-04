using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Victoria.Objects;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	public class LavaTrackQueue : List<LavaTrackOwner> {

		#region Properties

		/// <summary>
		/// Gets the first track in the queue.
		/// </summary>
		public LavaTrackOwner First => this.First();
		/// <summary>
		/// Gets the last track in the queue.
		/// </summary>
		public LavaTrackOwner Last => this.Last();

		#endregion

		#region Enqueue/Dequeue

		/// <summary>
		/// Enqueues and creates a new track owner.
		/// </summary>
		/// <returns>The enqueued track.</returns>
		public LavaTrackOwner Enqueue(ICommandContext context, LavaTrack track) {
			LavaTrackOwner trackOwner = new LavaTrackOwner(context, track);
			Add(trackOwner);
			return trackOwner;
		}
		/// <summary>
		/// Enqueues a new track owner.
		/// </summary>
		/// <param name="trackOwner">The track to enqueue.</param>
		/// <returns>The enqueued track.</returns>
		public LavaTrackOwner Enqueue(LavaTrackOwner trackOwner) {
			Add(trackOwner);
			return trackOwner;
		}
		/// <summary>
		/// Dequeues and returns the first track owner.
		/// </summary>
		/// <returns>The dequeued track.</returns>
		public LavaTrackOwner Dequeue() {
			LavaTrackOwner trackOwner = First;
			RemoveAt(0);
			return trackOwner;
		}

		#endregion

		#region Move

		/// <summary>
		/// Moves the track at the specified index forward.
		/// </summary>
		/// <param name="index">The index of the track.</param>
		/// <param name="distance">The relative distance to move the track.</param>
		public void MoveForward(int index, int distance) {
			LavaTrackOwner trackOwner = this[index];
			RemoveAt(index);
			Insert(index - distance, trackOwner);
		}
		/// <summary>
		/// Moves the track at the specified index backward.
		/// </summary>
		/// <param name="index">The index of the track.</param>
		/// <param name="distance">The relative distance to move the track.</param>
		public void MoveBack(int index, int distance) {
			LavaTrackOwner trackOwner = this[index];
			RemoveAt(index);
			Insert(index + distance, trackOwner);
		}
		
		#endregion
	}
}
