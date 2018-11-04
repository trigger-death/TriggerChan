using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Discord;
using TriggersTools.DiscordBots.Reactions;

namespace TriggersTools.DiscordBots.SpoilerBot.Reactions {
	public class SpoilerReactions : ReactionService {

		#region Constants

		// Category Names
		private const string Responses = "Responses";
		private const string Actions = "Actions";

		#endregion

		#region Constructors

		public SpoilerReactions(DiscordBotServiceContainer services) : base(services) {
			
		}

		#endregion

		#region Response Reactions
		
		[Description("The command was successful")]
		[Category(Responses)]
		public static Emoji Success { get; } = new Emoji("✅");
		[Description("The command was not found")]
		[Category(Responses)]
		public static Emoji UnknownCommand { get; } = new Emoji("❓");
		[Description("Not allowed to use the command")]
		[Category(Responses)]
		public static Emoji UnmetPrecondition { get; } = new Emoji("⛔");
		[Description("The command is or was locked")]
		[Category(Responses)]
		public static Emoji Locked { get; } = new Emoji("🔒");
		[Description("The command is or was unlocked")]
		[Category(Responses)]
		public static Emoji Unlocked { get; } = new Emoji("🔓");
		[Description("The command was incorrectly formatted")]
		[Category(Responses)]
		public static Emoji InvalidArgument { get; } = new Emoji("❌");
		[Description("An error occurred within the bot")]
		[Category(Responses)]
		public static Emoji Exception { get; } = new Emoji("⚠");

		#endregion

		#region Action Reactions

		[Description("Click to hear spoiler")]
		[Category(Actions)]
		public static Emoji ViewSpoiler { get; } = new Emoji("🔍");

		#endregion
	}
}
