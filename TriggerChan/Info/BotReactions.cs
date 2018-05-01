using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Info {

	public class ReactionInfo {
		public string Name { get; }
		public Emoji Emoji { get; }
		public string Description { get; }
		public string Category { get; }


		public static implicit operator Emoji(ReactionInfo react) {
			return react.Emoji;
		}

		public ReactionInfo(FieldInfo field) {
			Name = field.Name;
			Emoji = (Emoji) field.GetValue(null);

			var desc = field.GetCustomAttribute<DescriptionAttribute>();
			Description = desc?.Description ?? "No description available";
			
			var cat = field.GetCustomAttribute<CategoryAttribute>();
			Category = cat?.Category ?? "Misc";
		}
	}

	public class ReactionCategory {
		public string Category { get; }
		public List<ReactionInfo> Reactions { get; }

		public ReactionCategory(string category) {
			Category = category;
			Reactions = new List<ReactionInfo>();
		}
	}

	public static class BotReactions {

		private const string Responses = "Responses";
		private const string Actions = "Actions";

		[Description("The command was successful")]
		[Category(Responses)]
		public static readonly Emoji Success = new Emoji("✅");
		[Description("The command does not exist")]
		[Category(Responses)]
		public static readonly Emoji UnknownCommand = new Emoji("❓");
		[Description("Requirements not met")]
		[Category(Responses)]
		public static readonly Emoji UnmetPrecondition = new Emoji("⛔");
		[Description("Incorrect argument count")]
		[Category(Responses)]
		public static readonly Emoji BadArgCount = new Emoji("🔢");
		[Description("Failed to parse arguments")]
		[Category(Responses)]
		public static readonly Emoji ParseFailed = new Emoji("🔠");
		[Description("The command is locked")]
		[Category(Responses)]
		public static readonly Emoji CommandLocked = new Emoji("🔒");
		[Description("An argument was invalid")]
		[Category(Responses)]
		public static readonly Emoji InvalidArgument = new Emoji("❌");
		[Description("The bot had an error")]
		[Category(Responses)]
		public static readonly Emoji Exception = new Emoji("⚠️");
		[Description("User not in voice channel")]
		[Category(Responses)]
		public static readonly Emoji NotInVoiceChannel = new Emoji("🔈");
		[Description("Waiting for an attachment")]
		[Category(Responses)]
		public static readonly Emoji WaitingForUpload = new Emoji("⏳");
		[Description("The reply was sent to you as a direct message")]
		[Category(Responses)]
		public static readonly Emoji DMSent = new Emoji("📨");


		[Description("Click to hear spoiler")]
		[Category(Actions)]
		public static readonly Emoji ViewSpoiler = new Emoji("🔍");
		[Description("Pins a message with enough reacts")]
		[Category(Actions)]
		public static readonly Emoji PinMessage = new Emoji("📌");

		//public static readonly Emoji UnmetPrecondition2 = new Emoji("🚫");
		//public static readonly Emoji Unauthorized3 = new Emoji("👮");

		public static IReadOnlyDictionary<string, ReactionCategory> Categories { get; }
		public static IReadOnlyDictionary<string, ReactionInfo> Reactions { get; }

		public static ReactionInfo GetReaction(string name) {
			ReactionInfo reaction;
			Reactions.TryGetValue(name, out reaction);
			return reaction;
		}

		static BotReactions() {
			Dictionary<string, ReactionCategory> categories = new Dictionary<string, ReactionCategory>();
			Dictionary<string, ReactionInfo> reactions = new Dictionary<string, ReactionInfo>();

			Type emojiType = typeof(Emoji);
			foreach (FieldInfo field in typeof(BotReactions).GetFields()) {
				if (field.FieldType == emojiType) {
					ReactionInfo reaction = new ReactionInfo(field);
					ReactionCategory category;
					if (!categories.TryGetValue(reaction.Category, out category)) {
						category = new ReactionCategory(reaction.Category);
						categories.Add(reaction.Category, category);
					}
					category.Reactions.Add(reaction);
					reactions.Add(field.Name, reaction);
				}
			}
			Categories = new ReadOnlyDictionary<string, ReactionCategory>(categories);
			Reactions = new ReadOnlyDictionary<string, ReactionInfo>(reactions);
		}
	}
}
