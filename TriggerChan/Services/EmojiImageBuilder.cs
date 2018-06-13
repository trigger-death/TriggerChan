using System;
using System.Collections.Generic;
using System.Text;
using IGuild = Discord.IGuild;
using GuildEmote = Discord.GuildEmote;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Text;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.Primitives;

namespace TriggersTools.DiscordBots.TriggerChan.Services {

	public class DownloadedGuildEmote {
		public Image<Rgba32> Image { get; set; }
		public GuildEmote Emote { get; set; }
		public string Name {
			get { return Emote.Name; }
		}
		public string Label {
			get { return $":{Emote.Name}:"; }
		}

		public DownloadedGuildEmote(GuildEmote emote, Image<Rgba32> image) {
			Emote = emote;
			Image = image;
		}
	}

	public static class EmojiImageBuilder {


		private const int FontSize = 16;
		private const string FontName = "Arial";

		private const int Border = 2;
		private const int EmoteSize = 32;
		private const int Spacing = 6;
		private const int NameSpacing = 6;
		private const int BelowEmoteHeight = 16;
		private const int RowHeight = Spacing * 2 + EmoteSize + NameSpacing + BelowEmoteHeight;
		private const int MinColumnWidth = EmoteSize + Spacing * 2 + 4;

		public static byte[] CreateImage(List<GuildEmote> emotes, int? columnsRaw = null, bool compact = false) {

			int columns;
			if (columnsRaw.HasValue)
				columns = Math.Max(columnsRaw.Value, emotes.Count);
			else
				columns = Math.Min(5, (int) Math.Floor(Math.Sqrt(emotes.Count)));
			int rows = (emotes.Count + columns - 1) / columns;

			var font = SystemFonts.CreateFont(FontName, FontSize, FontStyle.Regular);
			
			int totalColumnWidth = 0;
			int column = 0;
			int row = 0;
			int[,] labelWidths = new int[columns, rows];
			int[] columnStarts = new int[columns];
			int[] columnWidths = new int[columns];
			for (int i = 0; i < columnWidths.Length; i++)
				columnWidths[i] = MinColumnWidth;
			foreach (GuildEmote emote in emotes) {
				labelWidths[column, row] = (int) TextMeasurer.Measure(
					$":{emote.Name}:", new RendererOptions(font)
					).Width + Spacing * 2;
				columnWidths[column] = Math.Max(columnWidths[column], labelWidths[column, row]);
				column++;
				if (column == columns) {
					column = 0;
					row++;
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
			
			using (Image<Rgba32> image = new Image<Rgba32>(totalColumnWidth + Border * 2, RowHeight * rows + Border * 2)) {
				image.Mutate(context => {
					context.Fill(new Rgba32(47, 49, 54, 255));
					Rgba32 borderColor = new Rgba32(40, 43, 48);
					context.Fill(borderColor, new Rectangle(0, 0, Border, image.Height));
					context.Fill(borderColor, new Rectangle(image.Width - Border, 0, Border, image.Height));
					context.Fill(borderColor, new Rectangle(0, 0, image.Width, Border));
					context.Fill(borderColor, new Rectangle(0, image.Height - Border, image.Width, Border));
					foreach (GuildEmote emote in emotes) {
						Point point = new Point(columnStarts[column] + Border, row * RowHeight + Border);
						DrawEmote(context, font, point, columnWidths[column], emote).Wait();
						column++;
						if (column == columns) {
							column = 0;
							row++;
						}
					}
				});
				
				using (MemoryStream stream = new MemoryStream()) {
					image.SaveAsPng(stream);
					return stream.ToArray();
				}
			}
		}

		public static async Task DrawEmote(IImageProcessingContext<Rgba32> context, Font font, Point point, int columnWidth, GuildEmote emote) {
			using (Image<Rgba32> emoteImg = await GetEmote(emote)) {
				point.Y += Spacing;

				context.DrawImage(GraphicsOptions.Default, emoteImg, new Point(point.X + (columnWidth - EmoteSize) / 2, point.Y));

				point.Y += EmoteSize + NameSpacing;
				context.DrawText(new TextGraphicsOptions() {
					HorizontalAlignment = HorizontalAlignment.Center,
					Antialias = true,
					ApplyKerning = true,
				}, $":{emote.Name}:", font, Rgba32.White,
				new Point(point.X + columnWidth / 2, point.Y));
			}
		}


		public static async Task<Image<Rgba32>> GetEmote(GuildEmote emote) {
			using (HttpClient client = new HttpClient())
			using (Stream stream = await client.GetStreamAsync(emote.Url)) {
				var image = Image.Load<Rgba32>(stream);
				image.Mutate(x => {
					if (image.Width > image.Height)
						x.Resize(EmoteSize, image.Height * EmoteSize / image.Width);
					else if (image.Height > image.Width)
						x.Resize(image.Width * EmoteSize / image.Height, EmoteSize);
					else
						x.Resize(EmoteSize, EmoteSize);
				});
				return image;
			}
		}
	}
}
