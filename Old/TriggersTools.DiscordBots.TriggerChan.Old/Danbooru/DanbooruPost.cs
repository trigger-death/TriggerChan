using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Danbooru {

	[Flags]
	public enum DanbooruRating {
		None = 0,
		[JsonProperty(PropertyName = "s")]
		Safe = (1 << 0),
		[JsonProperty(PropertyName = "q")]
		Questionable = (1 << 1),
		[JsonProperty(PropertyName = "e")]
		Explicit = (1 << 2),

		NotSafe = Questionable | Explicit,
		NotQuestionable = Safe | Explicit,
		NotExplicit = Safe | Questionable,
		Any = Safe | Questionable | Explicit,
	}

	public class InvalidTagsException : Exception {
		public InvalidTagsException(string tag)
			: base($"Tag *{Format.Sanitize(tag)}* has invalid characters.\n" +
				  "Only letters, digits, parentheses, dashes, tildes, and underscores are allowed.\n" +
				  "Tildes may not start the tag and underscores may not start or end the tag.") { }
	}

	public class IllegalNsfwTagsException : Exception {
		public IllegalNsfwTagsException(string tag)
			: base($"Tag *{Format.Sanitize(tag)}* is not allowed in searches.") { }
	}

	public class DanbooruPost {

		// GENERAL:

		[JsonProperty(PropertyName = "id")]
		public ulong Id { get; set; }

		[JsonProperty(PropertyName = "score")]
		public int Score { get; set; }

		[JsonProperty(PropertyName = "rating")]
		[JsonConverter(typeof(JsonEnumStringConverter))]
		public DanbooruRating Rating { get; set; }

		[JsonIgnore]
		public string PostUrl {
			get { return @"https://danbooru.donmai.us/posts/" + Id.ToString(); }
		}

		// FILE:

		[JsonProperty(PropertyName = "image_width")]
		public int ImageWidth { get; set; }

		[JsonProperty(PropertyName = "image_height")]
		public int ImageHeight { get; set; }

		[JsonProperty(PropertyName = "file_size")]
		public int FileSize { get; set; }

		[JsonProperty(PropertyName = "file_ext")]
		[JsonConverter(typeof(JsonDotExtensionConverter))]
		public string Extension { get; set; }

		// TAGS:

		[JsonProperty(PropertyName = "tag_string")]
		[JsonConverter(typeof(JsonStringArrayConverter))]
		public string[] Tags { get; set; }

		[JsonProperty(PropertyName = "tag_string_general")]
		[JsonConverter(typeof(JsonStringArrayConverter))]
		public string[] GeneralTags { get; set; }

		[JsonProperty(PropertyName = "tag_string_character")]
		[JsonConverter(typeof(JsonStringArrayConverter))]
		public string[] CharacterTags { get; set; }

		[JsonProperty(PropertyName = "tag_string_copyright")]
		[JsonConverter(typeof(JsonStringArrayConverter))]
		public string[] CopyrightTags { get; set; }

		[JsonProperty(PropertyName = "tag_string_cartist")]
		[JsonConverter(typeof(JsonStringArrayConverter))]
		public string[] ArtistTags { get; set; }
		
		// URL:

		[JsonProperty(PropertyName = "file_url")]
		public string FileUrl { get; set; }

		[JsonProperty(PropertyName = "large_file_url")]
		public string LargeFileUrl { get; set; }

		[JsonProperty(PropertyName = "preview_file_url")]
		public string PreviewFileUrl { get; set; }
	}
}
