using TriggersTools.Asciify.ColorMine.Converters;
using TriggersTools.Asciify.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using TriggersTools.Asciify.ColorMine.Comparisons;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	internal class DotColorAsciifier : AsciifierBase<ColorLab, ColorLab>, IDotColorAsciifier {

		protected override ColorLab CalcFontData(Color color) {
			return LabConverter.ToLab(color);
		}

		protected override ColorLab CalcFontData(IEnumerable<PixelPoint> pixels) {
			double count = 0;
			double inc;
			ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				inc = p.Color.A / 255d;
				charValue += ((ColorRgb) p.Color) * inc;
				count += inc;
			}
			return LabConverter.ToLab((charValue / count).ZeroNaNs);
			/*ColorLab charValue = new ColorLab();
			foreach (PixelPoint p in pixels) {
				inc = p.Color.A / 255d;
				if (inc != 1)
					Console.Write("a");
				charValue += LabConverter.ToLab(p.Color) * inc;
				count += inc;
			}
			return (charValue / count).ZeroNaNs;*/
			/*int count = 0;
			ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				ColorRgb color = ((ColorRgb) p.Color);
				charValue += color * color;
				count++;
			}
			charValue /= count;
			return LabConverter.ToLab(new ColorRgb(
				Math.Sqrt(charValue.R),
				Math.Sqrt(charValue.G),
				Math.Sqrt(charValue.B)
				));*/
			/*ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				inc = p.Color.A / 255d;
				if (inc != 1 && inc != 0)
					Console.Write(inc + ",");
				charValue *= ((ColorRgb) p.Color) * inc;
				count += inc;
			}
			return LabConverter.ToLab(new ColorRgb(
				Math.Pow(charValue.R, (1d / count).ZeroNaN()),
				Math.Pow(charValue.G, (1d / count).ZeroNaN()),
				Math.Pow(charValue.B, (1d / count).ZeroNaN())
				));*/
		}

		protected override ColorLab CalcImageData(IEnumerable<PixelPoint> pixels, Point start) {
			int count = 0;
			ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				charValue += ((ColorRgb) p.Color);
				count++;
			}
			return LabConverter.ToLab((charValue / count));
			/*ColorLab charValue = new ColorLab();
			foreach (PixelPoint p in pixels) {
				charValue += LabConverter.ToLab(p.Color);
				count++;
			}
			return charValue / count;*/
			/*ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				ColorRgb color = ((ColorRgb) p.Color);
				charValue += color * color;
				count++;
			}
			charValue = (charValue / count);
			return LabConverter.ToLab(new ColorRgb(
				Math.Sqrt(charValue.R),
				Math.Sqrt(charValue.G),
				Math.Sqrt(charValue.B)
				));*/
			/*ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				ColorRgb color = ((ColorRgb) p.Color);
				charValue += color * color;
				count++;
			}
			return LabConverter.ToLab(new ColorRgb(
				Math.Sqrt(charValue.R / count),
				Math.Sqrt(charValue.G / count),
				Math.Sqrt(charValue.B / count)
				));*/
			/*ColorRgb charValue = new ColorRgb();
			foreach (PixelPoint p in pixels) {
				charValue *= ((ColorRgb) p.Color);
				count++;
			}
			return LabConverter.ToLab(new ColorRgb(
				Math.Pow(charValue.R, (1d / count).ZeroNaN()),
				Math.Pow(charValue.G, (1d / count).ZeroNaN()),
				Math.Pow(charValue.B, (1d / count).ZeroNaN())
				));*/
		}

		protected override double CalcScore(ColorLab a, ColorLab b) {
			//return Comparer.Compare(a, b, true);
			return Cie76Comparison.CompareS(a, b, true);
		}
	}
}
