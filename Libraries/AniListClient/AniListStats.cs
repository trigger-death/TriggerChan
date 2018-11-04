using System;
using System.Collections.Generic;
using System.Text;

namespace AniListClient {
	public struct AniListStats {
		public double DaysSpent { get; set; }
		public double MeanScore { get; set; }
		public int Completed { get; set; }
		public int Episodes { get; set; }
		public int Volumes { get; set; }
	}
}
