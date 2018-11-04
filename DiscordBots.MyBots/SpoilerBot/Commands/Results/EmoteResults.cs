using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.SpoilerBot.Reactions;

namespace TriggersTools.DiscordBots.SpoilerBot.Commands {
	internal static class EmoteResults {
		public static RuntimeResult FromSuccess() {
			return ReactionResult.FromSuccess(SpoilerReactions.Success);
		}
		/*public static RuntimeResult FromInvalidArgument() {
			return ReactionResult.FromError(CommandError.ParseFailed, null, SpoilerReactions.InvalidArgument);
		}
		public static RuntimeResult FromException() {
			return ReactionResult.FromError(CommandError.Exception, null, SpoilerReactions.Exception);
		}
		public static RuntimeResult FromUnmetPrecondition() {
			return ReactionResult.FromError(CommandError.UnmetPrecondition, null, SpoilerReactions.UnmetPrecondition);
		}*/
	}
}
