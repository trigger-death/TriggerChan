using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	internal struct SectionedLabCounts {

		public SectionedLab Lab;
		public SectionedDouble Counts;

		public SectionedLabCounts(SectionedLab area, SectionedDouble counts) {
			Lab = (area / counts).ZeroNaNs;
			Counts = counts;
		}
	}
	internal struct SectionedLab {
		public ColorLab Left;
		public ColorLab Right;
		public ColorLab Top;
		public ColorLab Bottom;
		public ColorLab Center;
		public ColorLab All;

		public SectionedLab(ColorLab uniform) {
			Left = uniform;
			Right = uniform;
			Top = uniform;
			Bottom = uniform;
			Center = uniform;
			All = uniform;
		}

		public SectionedLab(ColorLab l, ColorLab r, ColorLab t, ColorLab b, ColorLab c, ColorLab a) {
			Left = l;
			Right = r;
			Top = t;
			Bottom = b;
			Center = c;
			All = a;
		}

		public static SectionedLab operator +(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedLab operator +(SectionedLab a, SectionedDouble b) =>
			new SectionedLab(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedLab operator +(SectionedLab a, ColorLab b) =>
			new SectionedLab(
				a.Left + b, a.Right + b,
				a.Top + b, a.Bottom + b,
				a.Center + b, a.All + b);
		public static SectionedLab operator +(SectionedLab a, double b) =>
			new SectionedLab(
				a.Left + b, a.Right + b,
				a.Top + b, a.Bottom + b,
				a.Center + b, a.All + b);
		public static SectionedLab operator +(SectionedDouble a, SectionedLab b) =>
			new SectionedLab(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedLab operator +(ColorLab a, SectionedLab b) =>
			new SectionedLab(
				a + b.Left, a + b.Right,
				a + b.Top, a + b.Bottom,
				a + b.Center, a + b.All);
		public static SectionedLab operator +(double a, SectionedLab b) =>
			new SectionedLab(
				a + b.Left, a + b.Right,
				a + b.Top, a + b.Bottom,
				a + b.Center, a + b.All);

		public static SectionedLab operator -(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedLab operator -(SectionedLab a, SectionedDouble b) =>
			new SectionedLab(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedLab operator -(SectionedLab a, ColorLab b) =>
			new SectionedLab(
				a.Left - b, a.Right - b,
				a.Top - b, a.Bottom - b,
				a.Center - b, a.All - b);
		public static SectionedLab operator -(SectionedLab a, double b) =>
			new SectionedLab(
				a.Left - b, a.Right - b,
				a.Top - b, a.Bottom - b,
				a.Center - b, a.All - b);
		public static SectionedLab operator -(SectionedDouble a, SectionedLab b) =>
			new SectionedLab(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedLab operator -(ColorLab a, SectionedLab b) =>
			new SectionedLab(
				a - b.Left, a - b.Right,
				a - b.Top, a - b.Bottom,
				a - b.Center, a - b.All);
		public static SectionedLab operator -(double a, SectionedLab b) =>
			new SectionedLab(
				a - b.Left, a - b.Right,
				a - b.Top, a - b.Bottom,
				a - b.Center, a - b.All);

		public static SectionedLab operator *(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedLab operator *(SectionedLab a, SectionedDouble b) =>
			new SectionedLab(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedLab operator *(SectionedLab a, ColorLab b) =>
			new SectionedLab(
				a.Left * b, a.Right * b,
				a.Top * b, a.Bottom * b,
				a.Center * b, a.All * b);
		public static SectionedLab operator *(SectionedLab a, double b) =>
			new SectionedLab(
				a.Left * b, a.Right * b,
				a.Top * b, a.Bottom * b,
				a.Center * b, a.All * b);
		public static SectionedLab operator *(SectionedDouble a, SectionedLab b) =>
			new SectionedLab(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedLab operator *(ColorLab a, SectionedLab b) =>
			new SectionedLab(
				a * b.Left, a * b.Right,
				a * b.Top, a * b.Bottom,
				a * b.Center, a * b.All);
		public static SectionedLab operator *(double a, SectionedLab b) =>
			new SectionedLab(
				a * b.Left, a * b.Right,
				a * b.Top, a * b.Bottom,
				a * b.Center, a * b.All);

		public static SectionedLab operator /(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedLab operator /(SectionedLab a, SectionedDouble b) =>
			new SectionedLab(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedLab operator /(SectionedLab a, ColorLab b) =>
			new SectionedLab(
				a.Left / b, a.Right / b,
				a.Top / b, a.Bottom / b,
				a.Center / b, a.All / b);
		public static SectionedLab operator /(SectionedLab a, double b) =>
			new SectionedLab(
				a.Left / b, a.Right / b,
				a.Top / b, a.Bottom / b,
				a.Center / b, a.All / b);
		public static SectionedLab operator /(SectionedDouble a, SectionedLab b) =>
			new SectionedLab(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedLab operator /(ColorLab a, SectionedLab b) =>
			new SectionedLab(
				a / b.Left, a / b.Right,
				a / b.Top, a / b.Bottom,
				a / b.Center, a / b.All);
		public static SectionedLab operator /(double a, SectionedLab b) =>
			new SectionedLab(
				a / b.Left, a / b.Right,
				a / b.Top, a / b.Bottom,
				a / b.Center, a / b.All);


		public static SectionedLab Max(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				ColorLab.Max(a.Left, b.Left),
				ColorLab.Max(a.Right, b.Right),
				ColorLab.Max(a.Top, b.Top),
				ColorLab.Max(a.Bottom, b.Bottom),
				ColorLab.Max(a.Center, b.Center),
				ColorLab.Max(a.All, b.All));

		public static SectionedLab Min(SectionedLab a, SectionedLab b) =>
			new SectionedLab(
				ColorLab.Min(a.Left, b.Left),
				ColorLab.Min(a.Right, b.Right),
				ColorLab.Min(a.Top, b.Top),
				ColorLab.Min(a.Bottom, b.Bottom),
				ColorLab.Min(a.Center, b.Center),
				ColorLab.Min(a.All, b.All));

		public SectionedLab ZeroNaNs =>
			new SectionedLab(
				Left.ZeroNaNs,
				Right.ZeroNaNs,
				Top.ZeroNaNs,
				Bottom.ZeroNaNs,
				Center.ZeroNaNs,
				All.ZeroNaNs);
	}
}
