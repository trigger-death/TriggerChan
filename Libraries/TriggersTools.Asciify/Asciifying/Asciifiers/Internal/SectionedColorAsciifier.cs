using TriggersTools.Asciify.ColorMine.Converters;
using TriggersTools.Asciify.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using TriggersTools.Asciify.ColorMine.Comparisons;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	/*internal struct SectionedColorI {
		public ColorI Left;
		public ColorI Right;
		public ColorI Top;
		public ColorI Bottom;
		public ColorI Center;
		public ColorI All;
	}*/
	
	
	internal class SectionedColorAsciifier : SectionedBaseAsciifier<SectionedLab, SectionedLabCounts>, ISectionedColorAsciifier {

		protected override SectionedLab CalcFontData(Color color) {
			return new SectionedLab(LabConverter.ToLab(color));
		}

		protected override SectionedLab CalcFontData(IEnumerable<PixelPoint> pixels) {
			SectionedDouble counts = new SectionedDouble();
			//SectionedLab area = new SectionedLab();
			SectionedRgb area = new SectionedRgb();
			double inc;
			foreach (PixelPoint p in pixels) {
				inc = p.Color.A / 255d;
				//ColorLab color = LabConverter.ToLab(p.Color) * inc;
				ColorRgb color = ((ColorRgb) p.Color) * inc;
				if (p.X < left) {
					area.Left += color;
					counts.Left += inc;
					//leftCount++;
				}
				else if (p.X >= right) {
					area.Right += color;
					counts.Right += inc;
					//rightCount++;
				}
				if (p.Y < top) {
					area.Top += color;
					counts.Top += inc;
					//topCount++;
				}
				else if (p.Y >= bottom) {
					area.Bottom += color;
					counts.Bottom += inc;
					//bottomCount++;
				}
				if (p.X >= left && p.X < right &&
					p.Y >= top && p.Y < bottom) {
					area.Center += color;
					counts.Center += inc;
					//centerCount++;
				}
				area.All += color;
				counts.All += inc;
				//allCount++;
			}
			return (new SectionedLab {
				Top = LabConverter.ToLab(area.Top),
				Left = LabConverter.ToLab(area.Left),
				Right = LabConverter.ToLab(area.Right),
				Bottom = LabConverter.ToLab(area.Bottom),
				Center = LabConverter.ToLab(area.Center),
				All = LabConverter.ToLab(area.All),
			} / counts).ZeroNaNs;
			//return (area / counts).ZeroNaNs;
		}

		protected override SectionedLabCounts CalcImageData(IEnumerable<PixelPoint> pixels, Point start) {
			/*int leftCount = 0;
			int rightCount = 0;
			int topCount = 0;
			int bottomCount = 0;
			int centerCount = 0;
			int allCount = 0;*/
			SectionedDouble counts = new SectionedDouble();
			//SectionedLab area = new SectionedLab();
			SectionedRgb area = new SectionedRgb();
			foreach (PixelPoint p in pixels) {
				//ColorLab color = LabConverter.ToLab(p.Color);
				ColorRgb color = (ColorRgb) p.Color;
				if (p.X < left) {
					area.Left += color;
					counts.Left++;
					//leftCount++;
				}
				else if (p.X >= right) {
					area.Right += color;
					counts.Right++;
					//rightCount++;
				}
				if (p.Y < top) {
					area.Top += color;
					counts.Top++;
					//topCount++;
				}
				else if (p.Y >= bottom) {
					area.Bottom += color;
					counts.Bottom++;
					//bottomCount++;
				}
				if (p.X >= left && p.X < right &&
					p.Y >= top && p.Y < bottom) {
					area.Center += color;
					counts.Center++;
					//centerCount++;
				}
				area.All += color;
				counts.All++;
				//allCount++;
			}
			return new SectionedLabCounts(new SectionedLab {
				Top = LabConverter.ToLab(area.Top),
				Left = LabConverter.ToLab(area.Left),
				Right = LabConverter.ToLab(area.Right),
				Bottom = LabConverter.ToLab(area.Bottom),
				Center = LabConverter.ToLab(area.Center),
				All = LabConverter.ToLab(area.All),
			}, counts);
			//return new SectionedLabCounts(area, counts);
		}

		protected override double CalcScore(SectionedLabCounts a, SectionedLab b) {
			/*double left = Comparer.Compare(a.Lab.Left, TransformLab(b.Lab.Left)) * a.Counts.Left;
			double right = Comparer.Compare(a.Lab.Right, TransformLab(b.Lab.Right)) * a.Counts.Right;
			double top = Comparer.Compare(a.Lab.Top, TransformLab(b.Lab.Top)) * a.Counts.Top;
			double bottom = Comparer.Compare(a.Lab.Bottom, TransformLab(b.Lab.Bottom)) * a.Counts.Bottom;
			double center = Comparer.Compare(a.Lab.Center, TransformLab(b.Lab.Center)) * a.Counts.Center;
			double all = Comparer.Compare(a.Lab.All, TransformLab(b.Lab.All)) * a.Counts.All;
			return left + right + top + bottom + center + all * AllFactor;*/
			/*double left = Comparer.Compare(a.Lab.Left, b.Lab.Left) * a.Counts.Left;
			double right = Comparer.Compare(a.Lab.Right, b.Lab.Right) * a.Counts.Right;
			double top = Comparer.Compare(a.Lab.Top, b.Lab.Top) * a.Counts.Top;
			double bottom = Comparer.Compare(a.Lab.Bottom, b.Lab.Bottom) * a.Counts.Bottom;
			double center = Comparer.Compare(a.Lab.Center, b.Lab.Center) * a.Counts.Center;
			double all = Comparer.Compare(a.Lab.All, b.Lab.All) * a.Counts.All;
			return left + right + top + bottom + center + all * AllFactor;*/
			double left = Cie76Comparison.CompareS(a.Lab.Left, b.Left) * a.Counts.Left;
			double right = Cie76Comparison.CompareS(a.Lab.Right, b.Right) * a.Counts.Right;
			double top = Cie76Comparison.CompareS(a.Lab.Top, b.Top) * a.Counts.Top;
			double bottom = Cie76Comparison.CompareS(a.Lab.Bottom, b.Bottom) * a.Counts.Bottom;
			double center = Cie76Comparison.CompareS(a.Lab.Center, b.Center) * a.Counts.Center;
			double all = Cie76Comparison.CompareS(a.Lab.All, b.All) * a.Counts.All;
			return left + right + top + bottom + center + all * AllFactor;
		}
	}
}
