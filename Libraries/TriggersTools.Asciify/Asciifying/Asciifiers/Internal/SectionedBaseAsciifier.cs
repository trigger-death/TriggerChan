using TriggersTools.Asciify.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using TriggersTools.Asciify.ColorMine.Converters;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	internal abstract class SectionedBaseAsciifier<TFntData, TImgData> : AsciifierBase<TFntData, TImgData>, ISectionedAsciifier {
		protected int left;
		protected int right;
		protected int top;
		protected int bottom;

		protected SectionedDouble fontCounts;

		private double allFactor = 0.0;
		public double AllFactor {
			get => allFactor;
			set {
				if (value < 0)
					throw new ArgumentException("AllFactor must be greater than or equal to zero!");
				allFactor = value;
			}
		}

		protected override void PreInitialize() {
			left = Font.Width / 4;
			top = Font.Height / 4;
			right = Font.Width - left;
			bottom = Font.Height - top;
			int hsideCount = Font.Height * left;
			int vsideCount = Font.Width * top;
			int centerCount = (right - left) * (bottom - top);
			int allCount = Font.Width * Font.Height;
			fontCounts = new SectionedDouble(
				hsideCount, hsideCount,
				vsideCount, vsideCount,
				centerCount, allCount);
		}
	}
}
