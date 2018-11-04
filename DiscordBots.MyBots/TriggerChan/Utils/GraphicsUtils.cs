using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Utils {
	public static class GraphicsUtils {
		public static SizeF MeasureStringF(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return g.MeasureString(s, font);
		}
		public static Size MeasureStringCeiling(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return Ceiling(g.MeasureString(s, font));
		}
		public static Size MeasureStringFloor(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return Floor(g.MeasureString(s, font));
		}
		public static Size MeasureStringRound(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return Round(g.MeasureString(s, font));
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
