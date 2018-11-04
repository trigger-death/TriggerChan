using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using TriggersTools.DiscordBots.Services;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	public class ColorService : DiscordService {

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="ColorService"/>.
		/// </summary>
		public ColorService(DiscordServiceProvider services) : base(services) { }

		#endregion

		public byte[] MakeColorSquare(Discord.Color color, int size) {
			return MakeColorSquare(color, size, size);
		}

		public byte[] MakeColorSquare(Discord.Color color, int width, int height) {
			using (Bitmap bitmap = new Bitmap(width, height)) {
				using (Graphics g = Graphics.FromImage(bitmap))
					g.Clear(Color.FromArgb(color.R, color.G, color.B));
				using (MemoryStream stream = new MemoryStream()) {
					bitmap.Save(stream, ImageFormat.Png);
					return stream.ToArray();
				}
			}
		}
	}
}
