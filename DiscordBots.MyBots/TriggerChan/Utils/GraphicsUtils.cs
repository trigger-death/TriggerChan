using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Utils {
	public static class GraphicsUtils {
		public static PointF FontOffsetF(Font font) {
			using (Bitmap image = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(image)) {
				const string s = ".";
				g.PageUnit = GraphicsUnit.Pixel;
				StringFormat format = new StringFormat();
				RectangleF rect = new RectangleF(0, 0, int.MaxValue, int.MaxValue);
				CharacterRange[] ranges = { new CharacterRange(0, s.Length) };

				format.SetMeasurableCharacterRanges(ranges);
				return g.MeasureCharacterRanges(s, font, rect, format)[0].GetBounds(g).Location;
			}
		}
		public static Point FontOffsetCeiling(Font font) {
			return Ceiling(FontOffsetF(font));
		}
		public static Point FontOffsetFloor(Font font) {
			return Floor(FontOffsetF(font));
		}
		public static Point FontOffsetRound(Font font) {
			return Round(FontOffsetF(font));
		}
		public static SizeF MeasureStringF(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			using (Graphics g = Graphics.FromImage(image)) {
				g.PageUnit = GraphicsUnit.Pixel;
				StringFormat format = new StringFormat();
				RectangleF rect = new RectangleF(0, 0, int.MaxValue, int.MaxValue);
				CharacterRange[] ranges = { new CharacterRange(0, s.Length) };

				format.SetMeasurableCharacterRanges(ranges);
				return g.MeasureCharacterRanges(s, font, rect, format)[0].GetBounds(g).Size;
			}
		}
		public static Size MeasureStringCeiling(string s, Font font) {
			return Ceiling(MeasureStringF(s, font));
		}
		public static Size MeasureStringFloor(string s, Font font) {
			return Floor(MeasureStringF(s, font));
		}
		public static Size MeasureStringRound(string s, Font font) {
			return Round(MeasureStringF(s, font));
		}
		private static Point Ceiling(PointF size) {
			return new Point((int) Math.Ceiling(size.X), (int) Math.Ceiling(size.Y));
		}
		private static Point Floor(PointF size) {
			return new Point((int) Math.Floor(size.X), (int) Math.Floor(size.Y));
		}
		private static Point Round(PointF size) {
			return new Point((int) Math.Round(size.X), (int) Math.Round(size.Y));
		}
		private static Size Ceiling(SizeF size) {
			return new Size((int) Math.Ceiling(size.Width), (int) Math.Ceiling(size.Height));
		}
		private static Size Floor(SizeF size) {
			return new Size((int) Math.Floor(size.Width), (int) Math.Floor(size.Height));
		}
		private static Size Round(SizeF size) {
			return new Size((int) Math.Round(size.Width), (int) Math.Round(size.Height));
		}
	}
}
