using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TriggersTools.DiscordBots.Extensions;

namespace TriggersTools.DiscordBots {
	/// <summary>
	/// A json Discord message that can be deserialized and converted into a <see cref="string"/> and
	/// <see cref="Discord.Embed"/>.
	/// </summary>
	public class JsonMessage {
		#region Fields

		/// <summary>
		/// Gets or sets the message text before the embed.
		/// </summary>
		[JsonProperty(PropertyName = "content")]
		public string Content { get; set; }
		/// <summary>
		/// Gets or sets the message embed.
		/// </summary>
		[JsonProperty(PropertyName = "embed")]
		public JsonEmbed Embed { get; set; }

		#endregion

		#region Deserialize

		/// <summary>
		/// Deserializes the <see cref="JsonMessage"/> from the json string.
		/// </summary>
		/// <param name="json">The json to deserialize the message from.</param>
		/// <returns>The deserialized json message.</returns>
		public static JsonMessage FromJson(string json) {
			var jsonMessage = JsonConvert.DeserializeObject<JsonMessage>(json);
			if (jsonMessage.Content == null && jsonMessage.Embed == null)
				throw new ArgumentException("Content or Embed must be specified!");
			return jsonMessage;
		}
		/// <summary>
		/// Deserializes the <see cref="JsonMessage"/> from the url's json string.
		/// </summary>
		/// <param name="url">The url to get the json from.</param>
		/// <returns>The deserialized json message.</returns>
		public static async Task<JsonMessage> FromUrlAsync(string url) {
			using (var client = new HttpClient()) {
				string json = await client.GetStringAsync(url).ConfigureAwait(false);
				return FromJson(json);
			}
		}

		#endregion
	}
	/// <summary>
	/// A json Discord embed that can be deserialized and converted into an <see cref="Embed"/> or
	/// <see cref="EmbedBuilder"/>.
	/// </summary>
	public class JsonEmbed {
		#region Fields

		/// <summary>
		/// The title of the embed.
		/// </summary>
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }
		/// <summary>
		/// The description of the embed.
		/// </summary>
		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }
		/// <summary>
		/// Url link for the embed title.
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		/// <summary>
		/// The display color for the embed. Pure black results in no color.
		/// </summary>
		[JsonProperty(PropertyName = "color")]
		[JsonConverter(typeof(JsonDiscordColorConverter))]
		public Color? Color { get; set; }
		/// <summary>
		/// The optional timestamp for the embed that is shown in the footer.
		/// </summary>
		[JsonProperty(PropertyName = "timestamp")]
		public DateTimeOffset? Timestamp { get; set; }

		/// <summary>
		/// The author of the embed. This can be parsed as just a string for the name, or an object.
		/// </summary>
		[JsonProperty(PropertyName = "author")]
		[JsonConverter(typeof(JsonEmbedAuthor.JsonEmbedAuthorConverter))]
		public JsonEmbedAuthor Author { get; set; }
		/// <summary>
		/// The footer of the embed. This can be parsed as just a string for the text, or an object.
		/// </summary>
		[JsonProperty(PropertyName = "footer")]
		[JsonConverter(typeof(JsonEmbedFooter.JsonEmbedFooterConverter))]
		public JsonEmbedFooter Footer { get; set; }
		/// <summary>
		/// The thumbnail image of the embed. This can be parsed as just a string for the text, or an object.
		/// </summary>
		[JsonProperty(PropertyName = "thumbnail")]
		[JsonConverter(typeof(JsonEmbedImage.JsonEmbedImageConverter))]
		public JsonEmbedImage Thumbnail { get; set; }
		/// <summary>
		/// The bottom image of the embed. This can be parsed as just a string for the text, or an object.
		/// </summary>
		[JsonProperty(PropertyName = "image")]
		[JsonConverter(typeof(JsonEmbedImage.JsonEmbedImageConverter))]
		public JsonEmbedImage Image { get; set; }
		/// <summary>
		/// The fields for the embed.
		/// </summary>
		[JsonProperty(PropertyName = "fields")]
		public JsonEmbedField[] Fields { get; set; }

		#endregion
		
		#region Build

		/// <summary>
		/// Converts the json embed into an <see cref="EmbedBuilder"/>.
		/// </summary>
		/// <returns>The built embed builder.</returns>
		public EmbedBuilder ToBuilder() {
			EmbedBuilder embed = new EmbedBuilder {
				Title = Title,
				Description = Description,
				Url = Url,
				ThumbnailUrl = Thumbnail?.Url,
				ImageUrl = Image?.Url,
			};
			if (Color.HasValue)
				embed.Color = Color.Value;
			if (Timestamp.HasValue)
				embed.Timestamp = Timestamp.Value;
			embed.Author = Author?.ToBuilder();
			embed.Footer = Footer?.ToBuilder();
			if (Fields != null) {
				foreach (var field in Fields) {
					if (field == null)
						throw new ArgumentNullException("Embed field cannot be null!");
					if (field.Pages != null)
						embed.PaginateField(field.Name, field.Pages, field.Inline);
					else
						embed.AddField(field.ToBuilder());
				}
			}
			return embed;
		}
		/// <summary>
		/// Converts the json embed into an <see cref="Embed"/>.
		/// </summary>
		/// <returns>The built embed.</returns>
		public Embed Build() {
			return ToBuilder().Build();
		}

		#endregion
	}
	/// <summary>
	/// A json Discord embed author that can be deserialized and converted into an <see cref="EmbedAuthor"/>
	/// or <see cref="EmbedAuthorBuilder"/>.
	/// </summary>
	public class JsonEmbedAuthor {
		#region Converters

		internal class JsonEmbedAuthorConverter : JsonConverter {
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				serializer.Serialize(writer, value);
			}
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				if (reader.Value is string name) return new JsonEmbedAuthor { Name = name };
				return serializer.Deserialize(reader, typeof(JsonEmbedAuthor));
			}
			public override bool CanConvert(Type objectType) {
				return objectType == typeof(string) || objectType == typeof(JObject);
			}
		}

		#endregion

		#region Fields

		/// <summary>
		/// Gets the name for the author. This value is required and also does not support markdown.
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		/// <summary>
		/// Gets the optional url for the author's name.
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }
		/// <summary>
		/// Gets the optional url of the icon to display next to the author.
		/// </summary>
		[JsonProperty(PropertyName = "icon_url")]
		public string IconUrl { get; set; }

		#endregion

		#region Build

		/// <summary>
		/// Converts the json embed author into an <see cref="EmbedAuthorBuilder"/>.
		/// </summary>
		/// <returns>The built embed author builder.</returns>
		public EmbedAuthorBuilder ToBuilder() {
			return new EmbedAuthorBuilder {
				Name = Name,
				Url = Url,
				IconUrl = IconUrl,
			};
		}
		/// <summary>
		/// Converts the json embed author into an <see cref="EmbedAuthor"/>.
		/// </summary>
		/// <returns>The built embed author.</returns>
		public EmbedAuthor Build() {
			return ToBuilder().Build();
		}

		#endregion
	}
	/// <summary>
	/// A json Discord embed footer that can be deserialized and converted into an <see cref="EmbedFooter"/>
	/// or <see cref="EmbedFooterBuilder"/>.
	/// </summary>
	public class JsonEmbedFooter {
		#region Converters

		internal class JsonEmbedFooterConverter : JsonConverter {
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				serializer.Serialize(writer, value);
			}
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				if (reader.Value is string text) return new JsonEmbedFooter { Text = text };
				return serializer.Deserialize(reader, typeof(JsonEmbedFooter));
			}
			public override bool CanConvert(Type objectType) {
				return objectType == typeof(string) || objectType == typeof(JObject);
			}
		}

		#endregion

		#region Fields

		/// <summary>
		/// Gets the text for the footer. This value is required and also does not support markdown.
		/// </summary>
		[JsonProperty(PropertyName = "text")]
		public string Text { get; set; }
		/// <summary>
		/// Gets the optional url of the icon to display next to the footer.
		/// </summary>
		[JsonProperty(PropertyName = "icon_url")]
		public string IconUrl { get; set; }

		#endregion

		#region Build

		/// <summary>
		/// Converts the json embed footer into an <see cref="EmbedFooterBuilder"/>.
		/// </summary>
		/// <returns>The built embed footer builder.</returns>
		public EmbedFooterBuilder ToBuilder() {
			return new EmbedFooterBuilder {
				Text = Text,
				IconUrl = IconUrl,
			};
		}
		/// <summary>
		/// Converts the json embed footer into an <see cref="EmbedFooter"/>.
		/// </summary>
		/// <returns>The built embed footer.</returns>
		public EmbedFooter Build() {
			return ToBuilder().Build();
		}

		#endregion
	}
	/// <summary>
	/// A json Discord embed image that can be deserialized and converted into an <see cref="string"/> url.
	/// </summary>
	public class JsonEmbedImage {
		#region Converters

		internal class JsonEmbedImageConverter : JsonConverter {
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				serializer.Serialize(writer, value);
			}
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				if (reader.Value is string url) return new JsonEmbedImage { Url = url };
				return serializer.Deserialize(reader, typeof(JsonEmbedImage));
			}
			public override bool CanConvert(Type objectType) {
				return objectType == typeof(string) || objectType == typeof(JObject);
			}
		}

		#endregion

		#region Fields

		/// <summary>
		/// Gets the url for the embed image or thumbnail.
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		#endregion

		#region Casting

		public static implicit operator JsonEmbedImage(string url) {
			return new JsonEmbedImage { Url = url };
		}

		#endregion
	}
	/// <summary>
	/// A json Discord embed field that can be deserialized and converted into an <see cref="EmbedField"/>
	/// or <see cref="EmbedFieldBuilder"/>.
	/// </summary>
	public class JsonEmbedField {
		#region Fields

		/// <summary>
		/// Gets the header of the embed field.
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		/// <summary>
		/// Gets the content of the embed field. This cannot be set if <see cref="Pages"/> is set!
		/// </summary>
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
		/// <summary>
		/// Gets the content of the embed field as a list of pages. This cannot be set if <see cref="Value"/>
		/// is set!<para/>
		/// Pages use the <see cref="EmbedExtensions.PaginateField"/> method to stretch fields over multiple
		/// fields if needed.
		/// </summary>
		[JsonProperty(PropertyName = "pages")]
		public string[] Pages { get; set; }
		/// <summary>
		/// Gets if the field is inline with other embeds.
		/// </summary>
		[JsonProperty(PropertyName = "inline")]
		public bool Inline { get; set; }

		#endregion

		#region Build

		/// <summary>
		/// Converts the json embed field into an <see cref="EmbedFieldBuilder"/>.
		/// </summary>
		/// <returns>The built embed field builder.</returns>
		public EmbedFieldBuilder ToBuilder() {
			if (Pages != null)
				throw new InvalidOperationException("ToBuilder and Build cannot be used for fields with Pages!");
			return new EmbedFieldBuilder {
				Name = Name,
				Value = Value,
				IsInline = Inline,
			};
		}
		/// <summary>
		/// Converts the json embed field into an <see cref="EmbedField"/>.
		/// </summary>
		/// <returns>The built embed field.</returns>
		public EmbedField Build() {
			return ToBuilder().Build();
		}

		#endregion
	}
}
