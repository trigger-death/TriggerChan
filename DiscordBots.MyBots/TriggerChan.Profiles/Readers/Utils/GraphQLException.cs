using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Utils {
	internal class GraphQLException : Exception {
		public IReadOnlyList<GraphQLError> Errors { get; }
		public bool IsAny404 { get; }

		public GraphQLException(IEnumerable<GraphQLError> errors) {
			Errors = errors.ToImmutableArray();
			IsAny404 = errors.Any(e => e.Status == 404);
		}
	}
}
