using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AniListClient.Internal.Converters {
	public class WatchedTimeConverter : JsonConverter {

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			if (objectType == typeof(DateTime)) {

			}
		}

		public override bool CanConvert(Type objectType) {
			throw new NotImplementedException();
		}
	}
}
