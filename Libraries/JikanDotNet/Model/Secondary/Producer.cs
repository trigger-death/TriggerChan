﻿using Newtonsoft.Json;

namespace JikanDotNet
{
	/// <summary>
	/// Producer model class.
	/// </summary>
	public class Producer
	{
		/// <summary>
		/// Url to link.
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		/// <summary>
		/// Name of the producer.
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Overriden ToString method.
		/// </summary>
		/// <returns>Name if not null, base method elsewhere.</returns>
		public override string ToString()
		{
			return Name ?? base.ToString();
		}
	}
}