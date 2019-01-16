using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Danbooru {

	[Flags]
	public enum DanbooruRating {
		None = 0,
		[JsonProperty("s")]
		Safe = (1 << 0),
		[JsonProperty("q")]
		Questionable = (1 << 1),
		[JsonProperty("e")]
		Explicit = (1 << 2),

		NotSafe = Questionable | Explicit,
		NotQuestionable = Safe | Explicit,
		NotExplicit = Safe | Questionable,
		Any = Safe | Questionable | Explicit,
	}

	public class InvalidTagsException : Exception {
		public InvalidTagsException(string tag)
			: base($"Tag \"{Format.Sanitize(tag)}\" has invalid characters.\n" +
				  "Tags must only use plain english characters!") { }
				  //"Only letters, digits, parentheses, dashes, tildes, and underscores are allowed.\n" +
				  //"Tildes may not start the tag and underscores may not start or end the tag.") { }
	}

	public class IllegalNsfwTagsException : Exception {
		public IllegalNsfwTagsException(string tag)
			: base($"Tag \"{Format.Sanitize(tag)}\" is not allowed in NSFW searches.") { }
	}

	public class DanbooruPost {

		// GENERAL:

		[JsonProperty("id")]
		public ulong Id { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("rating")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DanbooruRating Rating { get; set; }

		[JsonIgnore]
		public string PostUrl => $@"https://danbooru.donmai.us/posts/{Id}";

		// FILE:

		[JsonProperty("image_width")]
		public int ImageWidth { get; set; }

		[JsonProperty("image_height")]
		public int ImageHeight { get; set; }

		[JsonProperty("file_size")]
		public int FileSize { get; set; }

		[JsonProperty("file_ext")]
		[JsonConverter(typeof(JsonDotExtensionConverter))]
		public string Extension { get; set; }

		// TAGS:

		[JsonProperty("tag_string")]
		[JsonConverter(typeof(JsonStringSpacedArrayConverter))]
		public string[] Tags { get; set; }

		/*[JsonProperty("tag_string_general")]
		[JsonConverter(typeof(JsonStringSpacedArrayConverter))]
		public string[] GeneralTags { get; set; }

		[JsonProperty("tag_string_character")]
		[JsonConverter(typeof(JsonStringSpacedArrayConverter))]
		public string[] CharacterTags { get; set; }

		[JsonProperty("tag_string_copyright")]
		[JsonConverter(typeof(JsonStringSpacedArrayConverter))]
		public string[] CopyrightTags { get; set; }

		[JsonProperty("tag_string_cartist")]
		[JsonConverter(typeof(JsonStringSpacedArrayConverter))]
		public string[] ArtistTags { get; set; }*/
		
		[JsonProperty("is_banned")]
		public bool IsBanned { get; set; }
		[JsonProperty("has_children")]
		public bool HasChildren { get; set; }

		// URL:

		/// <summary>
		/// Gets the full-size image url.
		/// </summary>
		[JsonProperty("file_url")]
		public string FileUrl { get; set; }
		/// <summary>
		/// Gets the large-size image url, but <see cref="FileUrl"/> is the full size.
		/// </summary>
		[JsonProperty("large_file_url")]
		public string LargeFileUrl { get; set; }
		/// <summary>
		/// Gets the preview-size image url.
		/// </summary>
		[JsonProperty("preview_file_url")]
		public string PreviewFileUrl { get; set; }
	}

	public class DanbooruCount {
		public class DanbooruCounts {
			[JsonProperty("posts")]
			public int Posts { get; set; }
		}

		[JsonProperty("counts")]
		public DanbooruCounts Counts { get; set; }

		public static implicit operator int(DanbooruCount count) {
			return count.Counts.Posts;
		}
	}
}
