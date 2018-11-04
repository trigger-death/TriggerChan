using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.SteinsGate;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class DivergenceService {

		public DivergenceService() {
			Divergence.EnableLimits = true;
			Divergence.MaxLines = 3;
			Divergence.MaxLength = 24;
		}


		public Bitmap Draw(string text, bool small) {
			var args = new DivergenceArgs {
				Scale = (!small ? DivergenceScale.Medium : DivergenceScale.Small),
				Escape = DivergenceEscape.NewLines,
				Authenticity = DivergenceAuthenticity.Lax,
			};
			return Divergence.Draw(text, args);
		}

	}
}
