using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Danbooru {
	public class JsonStringEnumConverter : JsonConverter {

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			string enumStr = (string) value;
			foreach (var field in value.GetType().GetFields()) {
				string realName = field.Name;
				var attr = field.GetCustomAttribute<JsonPropertyAttribute>();
				if (attr?.PropertyName != null) {
					realName = attr.PropertyName;
				}
				if (enumStr == realName)
					writer.WriteValue(realName);
			}
			throw new Exception("Unknown enum value");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			string enumStr = (string) reader.Value;
			foreach (var field in objectType.GetFields()) {
				string realName = field.Name;
				var attr = field.GetCustomAttribute<JsonPropertyAttribute>();
				if (attr?.PropertyName != null) {
					realName = attr.PropertyName;
				}
				if (enumStr == realName)
					return field.GetValue(null);
			}
			throw new Exception("Unknown enum name");
		}

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(string);
		}
	}
}
