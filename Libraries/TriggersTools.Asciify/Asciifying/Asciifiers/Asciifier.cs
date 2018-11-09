using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.Asciify.Asciifying.Asciifiers {
	public static class Asciifier {


		public static IDotColorAsciifier DotColor => new DotColorAsciifier();
		//public static IAsciifier DotColor => new AsciifierBase2();

		public static IDotIntensityAsciifier DotIntensity => new DotIntensityAsciifier();

		public static ISectionedColorAsciifier SectionedColor => new SectionedColorAsciifier();
		//public static IAsciifier SectionedColor => new AsciifierBase3();

		public static ISectionedIntensityAsciifier SectionedIntensity => new SectionedIntensityAsciifier();

	}
}
