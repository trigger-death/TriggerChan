using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	internal struct SectionedRgb {
		public ColorRgb Left;
		public ColorRgb Right;
		public ColorRgb Top;
		public ColorRgb Bottom;
		public ColorRgb Center;
		public ColorRgb All;

		public SectionedRgb(ColorRgb uniform) {
			Left = uniform;
			Right = uniform;
			Top = uniform;
			Bottom = uniform;
			Center = uniform;
			All = uniform;
		}

		public SectionedRgb(ColorRgb l, ColorRgb r, ColorRgb t, ColorRgb b, ColorRgb c, ColorRgb a) {
			Left = l;
			Right = r;
			Top = t;
			Bottom = b;
			Center = c;
			All = a;
		}

		public static SectionedRgb operator +(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedRgb operator +(SectionedRgb a, SectionedDouble b) =>
			new SectionedRgb(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedRgb operator +(SectionedRgb a, ColorRgb b) =>
			new SectionedRgb(
				a.Left + b, a.Right + b,
				a.Top + b, a.Bottom + b,
				a.Center + b, a.All + b);
		public static SectionedRgb operator +(SectionedRgb a, double b) =>
			new SectionedRgb(
				a.Left + b, a.Right + b,
				a.Top + b, a.Bottom + b,
				a.Center + b, a.All + b);
		public static SectionedRgb operator +(SectionedDouble a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left + b.Left, a.Right + b.Right,
				a.Top + b.Top, a.Bottom + b.Bottom,
				a.Center + b.Center, a.All + b.All);
		public static SectionedRgb operator +(ColorRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a + b.Left, a + b.Right,
				a + b.Top, a + b.Bottom,
				a + b.Center, a + b.All);
		public static SectionedRgb operator +(double a, SectionedRgb b) =>
			new SectionedRgb(
				a + b.Left, a + b.Right,
				a + b.Top, a + b.Bottom,
				a + b.Center, a + b.All);

		public static SectionedRgb operator -(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedRgb operator -(SectionedRgb a, SectionedDouble b) =>
			new SectionedRgb(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedRgb operator -(SectionedRgb a, ColorRgb b) =>
			new SectionedRgb(
				a.Left - b, a.Right - b,
				a.Top - b, a.Bottom - b,
				a.Center - b, a.All - b);
		public static SectionedRgb operator -(SectionedRgb a, double b) =>
			new SectionedRgb(
				a.Left - b, a.Right - b,
				a.Top - b, a.Bottom - b,
				a.Center - b, a.All - b);
		public static SectionedRgb operator -(SectionedDouble a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left - b.Left, a.Right - b.Right,
				a.Top - b.Top, a.Bottom - b.Bottom,
				a.Center - b.Center, a.All - b.All);
		public static SectionedRgb operator -(ColorRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a - b.Left, a - b.Right,
				a - b.Top, a - b.Bottom,
				a - b.Center, a - b.All);
		public static SectionedRgb operator -(double a, SectionedRgb b) =>
			new SectionedRgb(
				a - b.Left, a - b.Right,
				a - b.Top, a - b.Bottom,
				a - b.Center, a - b.All);

		public static SectionedRgb operator *(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedRgb operator *(SectionedRgb a, SectionedDouble b) =>
			new SectionedRgb(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedRgb operator *(SectionedRgb a, ColorRgb b) =>
			new SectionedRgb(
				a.Left * b, a.Right * b,
				a.Top * b, a.Bottom * b,
				a.Center * b, a.All * b);
		public static SectionedRgb operator *(SectionedRgb a, double b) =>
			new SectionedRgb(
				a.Left * b, a.Right * b,
				a.Top * b, a.Bottom * b,
				a.Center * b, a.All * b);
		public static SectionedRgb operator *(SectionedDouble a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left * b.Left, a.Right * b.Right,
				a.Top * b.Top, a.Bottom * b.Bottom,
				a.Center * b.Center, a.All * b.All);
		public static SectionedRgb operator *(ColorRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a * b.Left, a * b.Right,
				a * b.Top, a * b.Bottom,
				a * b.Center, a * b.All);
		public static SectionedRgb operator *(double a, SectionedRgb b) =>
			new SectionedRgb(
				a * b.Left, a * b.Right,
				a * b.Top, a * b.Bottom,
				a * b.Center, a * b.All);

		public static SectionedRgb operator /(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedRgb operator /(SectionedRgb a, SectionedDouble b) =>
			new SectionedRgb(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedRgb operator /(SectionedRgb a, ColorRgb b) =>
			new SectionedRgb(
				a.Left / b, a.Right / b,
				a.Top / b, a.Bottom / b,
				a.Center / b, a.All / b);
		public static SectionedRgb operator /(SectionedRgb a, double b) =>
			new SectionedRgb(
				a.Left / b, a.Right / b,
				a.Top / b, a.Bottom / b,
				a.Center / b, a.All / b);
		public static SectionedRgb operator /(SectionedDouble a, SectionedRgb b) =>
			new SectionedRgb(
				a.Left / b.Left, a.Right / b.Right,
				a.Top / b.Top, a.Bottom / b.Bottom,
				a.Center / b.Center, a.All / b.All);
		public static SectionedRgb operator /(ColorRgb a, SectionedRgb b) =>
			new SectionedRgb(
				a / b.Left, a / b.Right,
				a / b.Top, a / b.Bottom,
				a / b.Center, a / b.All);
		public static SectionedRgb operator /(double a, SectionedRgb b) =>
			new SectionedRgb(
				a / b.Left, a / b.Right,
				a / b.Top, a / b.Bottom,
				a / b.Center, a / b.All);


		public static SectionedRgb Max(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				ColorRgb.Max(a.Left, b.Left),
				ColorRgb.Max(a.Right, b.Right),
				ColorRgb.Max(a.Top, b.Top),
				ColorRgb.Max(a.Bottom, b.Bottom),
				ColorRgb.Max(a.Center, b.Center),
				ColorRgb.Max(a.All, b.All));

		public static SectionedRgb Min(SectionedRgb a, SectionedRgb b) =>
			new SectionedRgb(
				ColorRgb.Min(a.Left, b.Left),
				ColorRgb.Min(a.Right, b.Right),
				ColorRgb.Min(a.Top, b.Top),
				ColorRgb.Min(a.Bottom, b.Bottom),
				ColorRgb.Min(a.Center, b.Center),
				ColorRgb.Min(a.All, b.All));

		public SectionedRgb ZeroNaNs =>
			new SectionedRgb(
				Left.ZeroNaNs,
				Right.ZeroNaNs,
				Top.ZeroNaNs,
				Bottom.ZeroNaNs,
				Center.ZeroNaNs,
				All.ZeroNaNs);
	}
}
