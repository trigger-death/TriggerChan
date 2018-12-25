﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Discord;
using TriggersTools.DiscordBots.Reactions;
using TriggersTools.DiscordBots.SpoilerBot.Reactions;

namespace TriggersTools.DiscordBots.TriggerChan.Reactions {
	public class TriggerReactions : ReactionService {

		#region Constants

		// Category Names
		private const string BotResponses = "Bot Responses";
		private const string BotActions = "Bot Actions";
		private const string UserActions = "User Actions";

		#endregion

		#region Constructors

		public TriggerReactions(TriggerServiceContainer services) : base(services) { }

		#endregion

		#region Response Reactions
		
		[Description("The command was successful")]
		[Category(BotResponses)]
		public static Emoji Success { get; } = new Emoji("✅");
		[Description("The command was not found")]
		[Category(BotResponses)]
		public static Emoji UnknownCommand { get; } = new Emoji("❓");
		[Description("Not allowed to use the command")]
		[Category(BotResponses)]
		public static Emoji UnmetPrecondition { get; } = new Emoji("⛔");
		[Description("The command is or was locked")]
		[Category(BotResponses)]
		public static Emoji Locked { get; } = new Emoji("🔒");
		[Description("The command is or was unlocked")]
		[Category(BotResponses)]
		public static Emoji Unlocked { get; } = new Emoji("🔓");
		[Description("The command was incorrectly formatted")]
		[Category(BotResponses)]
		public static Emoji InvalidArgument { get; } = new Emoji("❌");
		[Description("An error occurred within the bot")]
		[Category(BotResponses)]
		public static Emoji Exception { get; } = new Emoji("⚠");
		[Description("User not in voice channel")]
		[Category(BotResponses)]
		public static Emoji NotInVoice { get; } = new Emoji("🔈");
		[Description("The reply was sent to you as a direct message")]
		[Category(BotResponses)]
		public static Emoji DMSent { get; } = new Emoji("📨");

		#endregion

		#region Bot Action Reactions

		[Description("Click to hear spoiler")]
		[Category(BotActions)]
		public static Emoji ViewSpoiler { get; } = new Emoji("🔍");
		[Description("Marks a result as okay with Discord's policies")]
		[Category(BotActions)]
		public static Emoji Agreeable { get; } = new Emoji(char.ConvertFromUtf32(0x1F197)); // Squared OK
		[Description("Marks a result as against Discord's policies")]
		[Category(BotActions)]
		public static Emoji Dangerous { get; } = new Emoji(char.ConvertFromUtf32(0x1F694)); // Oncoming Police Car

		#endregion

		#region User Action Reactions

		[Description("Pins a message with enough reacts")]
		[Category(UserActions)]
		public static Emoji PinMessage { get; } = new Emoji("📌");

		#endregion
	}
}