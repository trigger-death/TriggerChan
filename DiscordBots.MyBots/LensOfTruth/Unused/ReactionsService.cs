using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {

	public class ReactionInfo {
		public string Name { get; }
		public IEmote Emote { get; }
		public string Description { get; }
		public string Category { get; }

		private ReactionInfo(MemberInfo member, IEmote value) {
			Name = member.Name;
			Emote = value;

			Description = member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description available";

			Category = member.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Misc";
		}
		public ReactionInfo(object instance, PropertyInfo prop) : this(prop, (IEmote) prop.GetValue(instance)) {

		}
		public ReactionInfo(object instance, FieldInfo field) : this(field, (IEmote) field.GetValue(instance)) {

		}
	}

	public class ReactionCategory {
		internal readonly List<ReactionInfo> reactions = new List<ReactionInfo>();

		public string Category { get; }
		public IEnumerable<ReactionInfo> Reactions => reactions.AsReadOnly();

		public ReactionCategory(string category) {
			Category = category;
		}
	}

	public class ReactionsService {

		private const string Responses = "Responses";
		private const string Actions = "Actions";

		/*[Description("The command was successful")]
		[Category(Responses)]
		public Emoji Success { get; } = new Emoji("✅");*/
		[Description("The command was not found")]
		[Category(Responses)]
		public Emoji UnknownCommand { get; } = new Emoji("❓");
		[Description("Not allowed to use the command")]
		[Category(Responses)]
		public Emoji UnmetPrecondition { get; } = new Emoji("⛔");
		/*[Description("The command is locked")]
		[Category(Responses)]
		public Emoji CommandLocked { get; } = new Emoji("🔒");*/
		[Description("The command was incorrectly used")]
		[Category(Responses)]
		public Emoji InvalidArgument { get; } = new Emoji("❌");
		[Description("An error occurred within the bot")]
		[Category(Responses)]
		public Emoji Exception { get; } = new Emoji("⚠");
		/*[Description("User not in voice channel")]
		[Category(Responses)]
		public Emoji NotInVoiceChannel { get; }  = new Emoji("🔈");
		[Description("Waiting for an attachment")]
		[Category(Responses)]
		public Emoji WaitingForUpload { get; } = new Emoji("⏳");
		[Description("The reply was sent to you as a direct message")]
		[Category(Responses)]
		public Emoji DMSent { get; } = new Emoji("📨");*/


		[Description("Click to hear spoiler")]
		[Category(Actions)]
		public Emoji ViewSpoiler { get; } = new Emoji("🔍");
		/*[Description("Pins a message with enough reacts")]
		[Category(Actions)]
		public Emoji PinMessage { get; } = new Emoji("📌");
		[Description("Marks a result as okay with Discord's policies")]
		[Category(Actions)]
		public Emoji Agreeable { get; } = new Emoji(char.ConvertFromUtf32(0x1F197)); // Squared OK
		[Description("Marks a result as against Discord's policies")]
		[Category(Actions)]
		public Emoji Dangerous { get; } = new Emoji(char.ConvertFromUtf32(0x1F694)); // Oncoming Police Car*/

		//public static readonly Emoji UnmetPrecondition2 = new Emoji("🚫");
		//public static readonly Emoji Unauthorized3 = new Emoji("👮");

		//public static ReadOnlyDictionary<string, ReactionCategory> Categories { get; }
		//public static ReadOnlyDictionary<string, ReactionInfo> Reactions { get; }
		
		public void Build() {

			Type emoteType = typeof(IEmote);
			foreach (PropertyInfo prop in typeof(ReactionsService).GetProperties()) {
				if (emoteType.IsAssignableFrom(prop.PropertyType)) {
					ReactionInfo reaction = new ReactionInfo(this, prop);
					AddReaction(reaction);
				}
			}
		}

		private readonly Dictionary<string, ReactionCategory> categories = new Dictionary<string, ReactionCategory>();
		private readonly List<ReactionInfo> reactions = new List<ReactionInfo>();
		public IEnumerable<ReactionCategory> Categories => categories.Values;
		public IEnumerable<ReactionInfo> Reactions => reactions.AsReadOnly();

		public void AddReaction(ReactionInfo reaction) {
			reactions.Add(reaction);
			if (!categories.TryGetValue(reaction.Category, out var category)) {
				category = new ReactionCategory(reaction.Category);
			}
			category.reactions.Add(reaction);
		}
	}
}
