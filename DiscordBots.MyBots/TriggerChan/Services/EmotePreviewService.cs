using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IGuild = Discord.IGuild;
using GuildEmote = Discord.GuildEmote;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TriggersTools.DiscordBots.TriggerChan.Utils;
using System.Linq;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class EmotePreviewService {

		private class DownloadedGuildEmote : IDisposable {
			public Bitmap Image { get; set; }
			public GuildEmote Emote { get; set; }
			public string Name {
				get { return Emote.Name; }
			}
			public string Label {
				get { return $":{Emote.Name}:"; }
			}

			public DownloadedGuildEmote(GuildEmote emote, Bitmap image) {
				Emote = emote;
				Image = image;
			}

			public void Dispose() {
				Image.Dispose();
			}
		}

		private class DownloadedGuildEmoteList : List<DownloadedGuildEmote>, IDisposable {


			public void Dispose() {
				foreach (DownloadedGuildEmote emote in this)
					emote?.Dispose();
			}
		}

		private const int FontSize = 11; // 11pt, 72px
		private const string FontName = "Arial";

		private const int Border = 2;
		private const int EmoteSize = 32;
		private const int Spacing = 6;
		private const int NameSpacing = 6;
		private const int BelowEmoteHeight = 16;
		private const int RowHeight = Spacing * 2 + EmoteSize + NameSpacing + BelowEmoteHeight;
		private const int MinColumnWidth = EmoteSize + Spacing * 2 + 4;

		private int SortEmotes(GuildEmote a, GuildEmote b) {
			if (a.Animated != b.Animated)
				return (a.Animated ? -1 : 1);
			return string.Compare(a.Name, b.Name, true);
		}

		public Bitmap Draw(IEnumerable<GuildEmote> emotesCollection, int? columnsRaw = null, bool compact = false) {
			List<GuildEmote> emotes = emotesCollection.ToList();
			emotes.Sort(SortEmotes);
			int columns;
			if (columnsRaw.HasValue)
				columns = Math.Min(columnsRaw.Value, emotes.Count);
			else
				columns = Math.Min(5, (int) Math.Floor(Math.Sqrt(emotes.Count)));
			int rows = (emotes.Count + columns - 1) / columns;

			int totalColumnWidth = 0;
			int column = 0;
			int row = 0;
			int[,] labelWidths = new int[columns, rows];
			int[] columnStarts = new int[columns];
			int[] columnWidths = new int[columns];
			using (Font font = new Font(FontName, FontSize, FontStyle.Regular)) {

				for (int i = 0; i < columnWidths.Length; i++)
					columnWidths[i] = MinColumnWidth;
				foreach (GuildEmote emote in emotes) {
					labelWidths[column, row] = GraphicsUtils.MeasureStringCeiling($":{emote.Name}:", font).Width + Spacing * 2;
					columnWidths[column] = Math.Max(columnWidths[column], labelWidths[column, row]);
					column++;
					if (column == columns) {
						column = 0;
						row++;
					}
				}
			}
			column = 0;
			row = 0;
			for (int x = 1; x < columns; x++) {
				if (!compact) {
					columnStarts[x] = columnStarts[x - 1] + columnWidths[x - 1];
				}
				else {
					for (int y = 0; y < rows; y++) {
						int prevLeftover = (columnWidths[x - 1] - labelWidths[x - 1, y]) / 2;
						int leftover = (columnWidths[x] - labelWidths[x, y]) / 2;
						int newStart = columnStarts[x - 1] + labelWidths[x - 1, y] + prevLeftover - leftover;
						int dif = newStart - columnStarts[x];
						for (int x2 = x; x2 < columns && dif > 0; x2++)
							columnStarts[x2] += dif;
					}
				}
			}
			totalColumnWidth = columnStarts[columns - 1] + columnWidths[columns - 1];

			Size imageSize = new Size(
				totalColumnWidth + Border * 2,
				RowHeight * rows + Border * 2);

			using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(47, 49, 54)))
			using (Brush borderBrush = new SolidBrush(Color.FromArgb(40, 43, 48)))
			using (Font font = new Font(FontName, FontSize, FontStyle.Regular)) {
				Bitmap bitmap = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format32bppPArgb);
				try {
					using (Graphics g = Graphics.FromImage(bitmap)) {
						g.PageUnit = GraphicsUnit.Pixel;
						g.Clear(Color.FromArgb(47, 49, 54));
						g.FillRectangle(borderBrush, new Rectangle(0, 0, Border, bitmap.Height));
						g.FillRectangle(borderBrush, new Rectangle(bitmap.Width - Border, 0, Border, bitmap.Height));
						g.FillRectangle(borderBrush, new Rectangle(0, 0, bitmap.Width, Border));
						g.FillRectangle(borderBrush, new Rectangle(0, bitmap.Height - Border, bitmap.Width, Border));
						foreach (GuildEmote emote in emotes) {
							Point point = new Point(columnStarts[column] + Border, row * RowHeight + Border);
							DrawEmote(g, font, point, columnWidths[column], emote).Wait();
							if (++column == columns) {
								column = 0;
								row++;
							}
						}
					}

					return bitmap;
				}
				catch {
					bitmap.Dispose();
					throw;
				}
			}
		}

		private async Task DrawEmote(Graphics g, Font font, Point point, int columnWidth, GuildEmote emote) {
			using (Bitmap emoteImg = await GetEmote(emote).ConfigureAwait(false)) {
				point.Y += Spacing;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				Rectangle emoteRect = new Rectangle(
					point.X + (columnWidth - EmoteSize) / 2, point.Y,
					EmoteSize, EmoteSize);

				if (emoteImg.Width > emoteImg.Height)
					emoteRect.Height = emoteImg.Height * EmoteSize / emoteImg.Width;
				else
					emoteRect.Width = emoteImg.Width * EmoteSize / emoteImg.Height;
				
				g.DrawImage(emoteImg, emoteRect);

				point.X += columnWidth / 2;
				point.Y += EmoteSize + NameSpacing;
				
				g.DrawString($":{emote.Name}:", font, Brushes.White, point, new StringFormat {
					Alignment = StringAlignment.Center,
					FormatFlags = StringFormatFlags.FitBlackBox,
				});
			}
		}


		private async Task<Bitmap> GetEmote(GuildEmote emote) {
			using (HttpClient client = new HttpClient())
			using (Stream stream = await client.GetStreamAsync(emote.Url).ConfigureAwait(false)) {
				return (Bitmap) Image.FromStream(stream);
			}
		}
	}
}
