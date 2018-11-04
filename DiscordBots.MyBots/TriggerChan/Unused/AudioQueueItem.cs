using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annie;
using Discord;
using Discord.Commands;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	public class AudioQueueItem {
		/// <summary>
		/// Gets or sets the owner of who queued this item. 
		/// </summary>
		public IUser Owner { get; set; }
		/// <summary>
		/// Gets or sets the channel this item was queued in. 
		/// </summary>
		public ITextChannel Channel { get; set; }
		/// <summary>
		/// Gets the actual download task for the song.
		/// </summary>
		public ISongDownloadTask Download { get; }

		/// <summary>
		/// Gets the title of the song.
		/// </summary>
		public string Title => Download.Title ?? Download.FileName;
		/// <summary>
		/// Gets the duration of the song.
		/// </summary>
		public TimeSpan Duration => Download.Duration;
		/// <summary>
		/// Gets the namename of the download.
		/// </summary>
		public string FileName => Download.FileName;
		/// <summary>
		/// Gets if the file is temporary and should be removed after use.
		/// </summary>
		public bool IsTemporary => Download.IsTemporary;

		public AudioQueueItem(ICommandContext context, ISongDownloadTask download) {
			Owner = context.User;
			Channel = (ITextChannel) context.Channel;
			Download = download;
		}
	}
}
