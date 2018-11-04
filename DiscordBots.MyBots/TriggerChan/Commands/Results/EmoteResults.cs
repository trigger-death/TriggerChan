using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Reactions;

namespace TriggersTools.DiscordBots.TriggerChan.Commands {
	public static class EmoteResults {
		public static RuntimeResult FromSuccess() {
			return ReactionResult.FromSuccess(TriggerReactions.Success);
		}
		public static RuntimeResult FromInvalidArgument() {
			return ReactionResult.FromError(CommandError.ParseFailed, null, TriggerReactions.InvalidArgument);
		}
		public static RuntimeResult FromInvalidArgument(string reason) {
			return ReactionResult.FromError(CommandError.ParseFailed, reason, TriggerReactions.InvalidArgument);
		}
		public static RuntimeResult FromException() {
			return ReactionResult.FromError(CommandError.Exception, null, TriggerReactions.Exception);
		}
		public static RuntimeResult FromUnmetPrecondition() {
			return ReactionResult.FromError(CommandError.UnmetPrecondition, null, TriggerReactions.UnmetPrecondition);
		}
		public static RuntimeResult FromLocked() {
			return ReactionResult.FromError(CommandError.UnmetPrecondition, null, TriggerReactions.Success, TriggerReactions.Locked);
		}
		public static RuntimeResult FromUnlocked() {
			return ReactionResult.FromError(CommandError.UnmetPrecondition, null, TriggerReactions.Success, TriggerReactions.Unlocked);
		}
		public static RuntimeResult FromDMSent() {
			return ReactionResult.FromSuccess(TriggerReactions.DMSent);
		}
		public static RuntimeResult FromNotInVoice() {
			return ReactionResult.FromSuccess(TriggerReactions.NotInVoice);
		}
	}
}
