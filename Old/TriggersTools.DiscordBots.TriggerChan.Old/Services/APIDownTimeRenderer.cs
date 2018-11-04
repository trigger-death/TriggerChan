using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	// Uses .NET Framework's System.Drawing or .Net Standard's System.Drawing.Common.

	public static class GraphicsHelper {
		public static SizeF MeasureStringF(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return g.MeasureString(s, font);
		}
		public static Size MeasureStringUp(string s, Font font) {
			using (Bitmap image = new Bitmap(1, 1))
			using (Graphics g = Graphics.FromImage(image))
				return RoundUp(g.MeasureString(s, font));
		}
		public static Size RoundUp(SizeF size) {
			return new Size((int) Math.Ceiling(size.Width), (int) Math.Ceiling(size.Height));
		}
	}
	public static class APIDownTimeRenderer {

		#region Constants

		// Strings
		public const string Title = "MyAnimeList API Downtime";
		public const string Days = "DAYS";
		public const string Hours = "HOURS";
		public const string Minutes = "MINUTES";
		public const string Seconds = "SECONDS";

		// Font Family
		public const string FontFamily = "Arial";
		
		// Font Sizes
		public const float CounterSize = 124;
		public const float TitleSize = 50;
		public const float UnitSize = 28;
		
		// Measurements
		public const int PaddingX = 10;
		public const int ColonSpaceX = -36;
		public const int TitleY = 8;
		public const int CounterY = 122;
		public const int UnitY = 352;
		public const int Height = 420;

		public static readonly DateTime StartTimeUtc = new DateTime(2018, 5, 24, 12, 0, 0, DateTimeKind.Utc);

		#endregion

		public static Bitmap CreateImage() {
			TimeSpan downTime = DateTime.UtcNow - StartTimeUtc;
			string days = ((int) Math.Floor(downTime.TotalDays)).ToString();
			string hours = downTime.Hours.ToString().PadLeft(2, '0');
			string minutes = downTime.Minutes.ToString().PadLeft(2, '0');
			string seconds = downTime.Seconds.ToString().PadLeft(2, '0');
			
			using (Brush redBrush = new SolidBrush(Color.FromArgb(161, 46, 49)))
			using (Brush blueBrush = new SolidBrush(Color.FromArgb(46, 81, 162)))
			using (Font titleFont   = new Font(FontFamily, TitleSize, FontStyle.Bold))
			using (Font counterFont = new Font(FontFamily, CounterSize))
			using (Font unitFont    = new Font(FontFamily, UnitSize)) {
				
				Size titleSize = GraphicsHelper.MeasureStringUp(Title, titleFont);

				Size daysSize    = GraphicsHelper.MeasureStringUp(days,    counterFont);
				Size hoursSize   = GraphicsHelper.MeasureStringUp(hours,   counterFont);
				Size minutesSize = GraphicsHelper.MeasureStringUp(minutes, counterFont);
				Size secondsSize = GraphicsHelper.MeasureStringUp(seconds, counterFont);

				Size unitDaysSize    = GraphicsHelper.MeasureStringUp(Days,    unitFont);
				Size unitHoursSize   = GraphicsHelper.MeasureStringUp(Hours,   unitFont);
				Size unitMinutesSize = GraphicsHelper.MeasureStringUp(Minutes, unitFont);
				Size unitSecondsSize = GraphicsHelper.MeasureStringUp(Seconds, unitFont);

				Size colonSize = GraphicsHelper.MeasureStringUp(":", counterFont);

				int width = PaddingX;
				Point daysPoint         = new Point(width, CounterY); width += daysSize.Width    + ColonSpaceX;
				Point daysColonPoint    = new Point(width, CounterY); width += colonSize.Width   + ColonSpaceX;
				Point hoursPoint        = new Point(width, CounterY); width += hoursSize.Width   + ColonSpaceX;
				Point hoursColonPoint   = new Point(width, CounterY); width += colonSize.Width   + ColonSpaceX;
				Point minutesPoint      = new Point(width, CounterY); width += minutesSize.Width + ColonSpaceX;
				Point minutesColonPoint = new Point(width, CounterY); width += colonSize.Width   + ColonSpaceX;
				Point secondsPoint      = new Point(width, CounterY); width += secondsSize.Width + PaddingX;

				Point titlePoint = new Point((width - titleSize.Width) / 2, TitleY);

				Point unitDaysPoint    = new Point(daysPoint.X    + ((daysSize.Width    - unitDaysSize.Width) / 2), UnitY);
				Point unitHoursPoint   = new Point(hoursPoint.X   + ((hoursSize.Width   - unitHoursSize.Width) / 2), UnitY);
				Point unitMinutesPoint = new Point(minutesPoint.X + ((minutesSize.Width - unitMinutesSize.Width) / 2), UnitY);
				Point unitSecondsPoint = new Point(secondsPoint.X + ((secondsSize.Width - unitSecondsSize.Width) / 2), UnitY);

				Point offset = Point.Empty;

				int widthSecondsUnitDiff = (unitSecondsPoint.X + unitSecondsSize.Width + PaddingX) - width;
				if (widthSecondsUnitDiff > 0) {
					offset.X += widthSecondsUnitDiff;
					width += widthSecondsUnitDiff * 2;
				}

				Bitmap image = new Bitmap(width, Height, PixelFormat.Format24bppRgb);
				using (Graphics g = Graphics.FromImage(image)) {
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					g.SmoothingMode     = SmoothingMode.HighQuality;

					g.Clear(Color.White);
					
					g.DrawString(Title, titleFont, blueBrush, Point.Add(offset, (Size) titlePoint));

					g.DrawString(days,    counterFont, blueBrush, Point.Add(offset, (Size) daysPoint));
					g.DrawString(hours,   counterFont, blueBrush, Point.Add(offset, (Size) hoursPoint));
					g.DrawString(minutes, counterFont, blueBrush, Point.Add(offset, (Size) minutesPoint));
					g.DrawString(seconds, counterFont, redBrush,  Point.Add(offset, (Size) secondsPoint));

					g.DrawString(":", counterFont, blueBrush, Point.Add(offset, (Size) daysColonPoint));
					g.DrawString(":", counterFont, blueBrush, Point.Add(offset, (Size) hoursColonPoint));
					g.DrawString(":", counterFont, blueBrush, Point.Add(offset, (Size) minutesColonPoint));

					g.DrawString(Days,    unitFont, blueBrush, Point.Add(offset, (Size) unitDaysPoint));
					g.DrawString(Hours,   unitFont, blueBrush, Point.Add(offset, (Size) unitHoursPoint));
					g.DrawString(Minutes, unitFont, blueBrush, Point.Add(offset, (Size) unitMinutesPoint));
					g.DrawString(Seconds, unitFont, blueBrush, Point.Add(offset, (Size) unitSecondsPoint));

				}
				return image;
			}
		}

		/// <summary>This only works in Windows. :(</summary>
		private static void OpenInMSPaint(this Bitmap bitmap) {
			bitmap.Save("image.png");
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				ProcessStartInfo start = new ProcessStartInfo {
					UseShellExecute = false,
					FileName = "mspaint",
					Arguments = "image.png",
				};
				Process.Start(start);
				Thread.Sleep(1000);
				try {
					File.Delete("image.png");
				}
				catch { }
			}
		}
	}
}
