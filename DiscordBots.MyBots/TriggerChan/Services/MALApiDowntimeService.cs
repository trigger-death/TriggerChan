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
using TriggersTools.DiscordBots.TriggerChan.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class MALApiDowntimeService {
		#region Constants

		// Strings
		private const string Title = "MyAnimeList API Downtime";
		private const string Days = "DAYS";
		private const string Hours = "HOURS";
		private const string Minutes = "MINUTES";
		private const string Seconds = "SECONDS";

		// Font Family
		private const string FontFamily = "Arial";

		// Font Sizes
		private const float CounterSize = 124; // 800px
		private const float TitleSize = 50; // 330px
		private const float UnitSize = 28; // 182px

		// Measurements
		private const int PaddingX = 40;
		private const int ColonSpaceX = 19;//-36; + 55
		private const int TitleY = 8;
		private const int CounterY = 122;
		private const int UnitY = 352;
		private const int Height = 420;


		#endregion

		#region Properties

		/// <summary>
		/// Gets the UTC start of the MAL API downtime.
		/// </summary>
		public DateTime StartTimeUtc => new DateTime(2018, 5, 24, 12, 0, 0, DateTimeKind.Utc);
		/// <summary>
		/// Gets the local start of the MAL API downtime.
		/// </summary>
		public DateTime StartTime => new DateTime(2018, 5, 24, 12, 0, 0, DateTimeKind.Utc);
		/// <summary>
		/// Gets the total downtime for the MAL API.
		/// </summary>
		public TimeSpan Downtime => DateTime.UtcNow - StartTimeUtc;

		#endregion

		public Bitmap Draw() {
			TimeSpan downTime = DateTime.UtcNow - StartTimeUtc;
			string days = ((int) Math.Floor(downTime.TotalDays)).ToString();
			string hours = downTime.Hours.ToString().PadLeft(2, '0');
			string minutes = downTime.Minutes.ToString().PadLeft(2, '0');
			string seconds = downTime.Seconds.ToString().PadLeft(2, '0');
			
			using (Brush redBrush = new SolidBrush(Color.FromArgb(161, 46, 49)))
			using (Brush blueBrush = new SolidBrush(Color.FromArgb(46, 81, 162)))
			using (Font titleFont   = new Font(FontFamily, TitleSize, FontStyle.Bold, GraphicsUnit.Point))
			using (Font counterFont = new Font(FontFamily, CounterSize, GraphicsUnit.Point))
			using (Font unitFont    = new Font(FontFamily, UnitSize, GraphicsUnit.Point)) {

				int counterOffset = GraphicsUtils.FontOffsetCeiling(counterFont).X;
				int unitOffset = GraphicsUtils.FontOffsetCeiling(unitFont).X;
				int titleOffset = GraphicsUtils.FontOffsetCeiling(titleFont).X;

				Size titleSize = GraphicsUtils.MeasureStringCeiling(Title, titleFont);

				int daysSize    = GraphicsUtils.MeasureStringCeiling(days,    counterFont).Width;
				int hoursSize   = GraphicsUtils.MeasureStringCeiling(hours,   counterFont).Width;
				int minutesSize = GraphicsUtils.MeasureStringCeiling(minutes, counterFont).Width;
				int secondsSize = GraphicsUtils.MeasureStringCeiling(seconds, counterFont).Width;

				int unitDaysSize    = GraphicsUtils.MeasureStringCeiling(Days,    unitFont).Width;
				int unitHoursSize   = GraphicsUtils.MeasureStringCeiling(Hours,   unitFont).Width;
				int unitMinutesSize = GraphicsUtils.MeasureStringCeiling(Minutes, unitFont).Width;
				int unitSecondsSize = GraphicsUtils.MeasureStringCeiling(Seconds, unitFont).Width;

				int colonSize = GraphicsUtils.MeasureStringCeiling(":", counterFont).Width;

				// Temporarily take out the font offset, add it back at the end.
				int width = PaddingX - counterOffset;
				Point daysPoint         = new Point(width, CounterY); width += daysSize    + ColonSpaceX;
				Point daysColonPoint    = new Point(width, CounterY); width += colonSize   + ColonSpaceX;
				Point hoursPoint        = new Point(width, CounterY); width += hoursSize   + ColonSpaceX;
				Point hoursColonPoint   = new Point(width, CounterY); width += colonSize   + ColonSpaceX;
				Point minutesPoint      = new Point(width, CounterY); width += minutesSize + ColonSpaceX;
				Point minutesColonPoint = new Point(width, CounterY); width += colonSize   + ColonSpaceX;
				Point secondsPoint      = new Point(width, CounterY); width += secondsSize + PaddingX + counterOffset;

				/*Point titlePoint = new Point((width - titleSize.Width - titleOffset) / 2, TitleY);

				Point unitDaysPoint    = new Point(daysPoint.X    + ((daysSize    - unitDaysSize    - unitOffset) / 2), UnitY);
				Point unitHoursPoint   = new Point(hoursPoint.X   + ((hoursSize   - unitHoursSize   - unitOffset) / 2), UnitY);
				Point unitMinutesPoint = new Point(minutesPoint.X + ((minutesSize - unitMinutesSize - unitOffset) / 2), UnitY);
				Point unitSecondsPoint = new Point(secondsPoint.X + ((secondsSize - unitSecondsSize - unitOffset) / 2), UnitY);*/

				Point titlePoint = new Point(width / 2, TitleY);

				Point unitDaysPoint    = new Point(daysPoint.X    + (daysSize    / 2) + counterOffset, UnitY);
				Point unitHoursPoint   = new Point(hoursPoint.X   + (hoursSize   / 2) + counterOffset, UnitY);
				Point unitMinutesPoint = new Point(minutesPoint.X + (minutesSize / 2) + counterOffset, UnitY);
				Point unitSecondsPoint = new Point(secondsPoint.X + (secondsSize / 2) + counterOffset, UnitY);

				Size offset = Size.Empty;

				int widthSecondsUnitDiff = (unitSecondsPoint.X + (secondsSize / 2) + PaddingX) - width;
				if (widthSecondsUnitDiff > 0) {
					offset.Width += widthSecondsUnitDiff;
					width += widthSecondsUnitDiff * 2;
				}

				Bitmap image = new Bitmap(width, Height, PixelFormat.Format24bppRgb);
				using (Graphics g = Graphics.FromImage(image)) {
					g.PageUnit = GraphicsUnit.Pixel;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					g.SmoothingMode     = SmoothingMode.HighQuality;

					g.Clear(Color.White);

					StringFormat center = new StringFormat {
						Alignment = StringAlignment.Center,
					};
					
					g.DrawString(Title, titleFont, blueBrush, Point.Add(titlePoint, offset), center);

					g.DrawString(days,    counterFont, blueBrush, Point.Add(daysPoint,    offset));
					g.DrawString(hours,   counterFont, blueBrush, Point.Add(hoursPoint,   offset));
					g.DrawString(minutes, counterFont, blueBrush, Point.Add(minutesPoint, offset));
					g.DrawString(seconds, counterFont, redBrush,  Point.Add(secondsPoint, offset));

					g.DrawString(":", counterFont, blueBrush, Point.Add(daysColonPoint,    offset));
					g.DrawString(":", counterFont, blueBrush, Point.Add(hoursColonPoint,   offset));
					g.DrawString(":", counterFont, blueBrush, Point.Add(minutesColonPoint, offset));

					g.DrawString(Days,    unitFont, blueBrush, Point.Add(unitDaysPoint,    offset), center);
					g.DrawString(Hours,   unitFont, blueBrush, Point.Add(unitHoursPoint,   offset), center);
					g.DrawString(Minutes, unitFont, blueBrush, Point.Add(unitMinutesPoint, offset), center);
					g.DrawString(Seconds, unitFont, blueBrush, Point.Add(unitSecondsPoint, offset), center);

				}
				return image;
			}
		}

		/// <summary>This only works in Windows. :(</summary>
		/*private static void OpenInMSPaint(this Bitmap bitmap) {
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
		}*/
	}
}
