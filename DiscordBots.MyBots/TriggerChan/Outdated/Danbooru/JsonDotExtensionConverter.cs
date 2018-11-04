using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TriggersTools.DiscordBots.TriggerChan.Danbooru {
	public class JsonDotExtensionConverter : JsonConverter {

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			string ext = (string) value;
			if (ext.StartsWith("."))
				ext = ext.Substring(1);
			writer.WriteValue(ext);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			string ext = (string) reader.Value;
			if (string.IsNullOrEmpty(ext))
				return "";
			return $".{ext}";
		}

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(string);
		}
	}
}
