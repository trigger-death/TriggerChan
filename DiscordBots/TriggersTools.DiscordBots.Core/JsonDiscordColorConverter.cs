using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace TriggersTools.DiscordBots {
	public class JsonDiscordColorConverter : JsonConverter {
		private static readonly Regex RgbRegex = new Regex(@"(?'r'\d{1,3}),(?'g'\d{1,3}),(?'b'\d{1,3})");
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			serializer.Serialize(writer, $"#{value:X6}");
			//serializer.Serialize(writer, ((Color) value).RawValue);
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (reader.Value is string s) {
				Match match = RgbRegex.Match(s);
				if (match.Success) {
					return new Color(
						byte.Parse(match.Groups["r"].Value),
						byte.Parse(match.Groups["g"].Value),
						byte.Parse(match.Groups["b"].Value)
					);
				}
				if (s.StartsWith("#"))
					return new Color(uint.Parse(s.Substring(1), NumberStyles.HexNumber, null));
			}
			else if (reader.Value is long i) {
				return new Color((uint) i);
			}
			return serializer.Deserialize(reader, typeof(Color));
		}
		public override bool CanConvert(Type objectType) {
			return objectType == typeof(string) || objectType == typeof(uint);
		}
	}
}
