using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace TriggersTools.DiscordBots.Utils {
	/// <summary>
	/// A builder for Urls with the ability to set the fragment and add values to the query.
	/// </summary>
	public class UrlBuilder {

		#region Fields

		/// <summary>
		/// Gets the base url for the builder. This should be null when unused.
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Gets the collection of query names and values.
		/// </summary>
		public NameValueCollection Query { get; }
		/// <summary>
		/// The decoded url fragment without a '#' infront. This should be null when unused.
		/// </summary>
		private string fragment;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the decoded url fragment without a '#' infront. This should be null when unused.
		/// </summary>
		public string Fragment {
			get => fragment;
			set {
				if (value == null)
					fragment = null;
				else if (value.StartsWith("#"))
					fragment = value.Substring(1);
				else
					fragment = value;
			}
		}
		/// <summary>
		/// Gets if <see cref="BaseUrl"/> is non-null.
		/// </summary>
		public bool HasBaseUrl => BaseUrl != null;
		/// <summary>
		/// Gets if <see cref="Fragment"/> is non-null.
		/// </summary>
		public bool HasFragment => fragment != null;
		/// <summary>
		/// Gets if <see cref="Query"/> has a count greater than 0.
		/// </summary>
		public bool HasQuery => Query.Count != 0;
		/// <summary>
		/// Gets the <see cref="string"/> representation of the full encoded url.
		/// </summary>
		public string Url => ToString();
		/// <summary>
		/// Gets the <see cref="Uri"/> representation of the url.
		/// </summary>
		public Uri Uri => new Uri(ToString());

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs an empty url to build from.
		/// </summary>
		public UrlBuilder() {
			Query = new NameValueCollection();
		}
		/// <summary>
		/// Deconstructs an existing url into baseUrl, query, and fragment.
		/// </summary>
		public UrlBuilder(string url) {
			int queryIndex = url.IndexOf('?');
			int fragmentIndex = url.IndexOf('#', Math.Max(0, queryIndex));
			int baseUrlLength = url.Length;
			if (fragmentIndex != -1) {
				baseUrlLength = fragmentIndex;
				fragment = HttpUtility.UrlDecode(url.Substring(fragmentIndex + 1));
			}
			if (queryIndex != -1) {
				baseUrlLength = queryIndex;
				Query = HttpUtility.ParseQueryString(url);
			}
			else {
				Query = new NameValueCollection();
			}
			BaseUrl = url.Substring(0, baseUrlLength);
		}

		#endregion

		#region Add

		public void Add(string name, string value) {
			Query.Add(name, value);
		}
		public void Add(string name, object value) {
			Query.Add(name, value.ToString());
		}
		public void Add(IReadOnlyDictionary<string, string> collection) {
			foreach (var pair in collection)
				Query.Add(pair.Key, pair.Value);
		}
		public void Add(IReadOnlyDictionary<string, object> collection) {
			foreach (var pair in collection)
				Query.Add(pair.Key, pair.Value.ToString());
		}
		public void Add(NameValueCollection collection) {
			Query.Add(collection);
		}

		#endregion

		#region ToString

		/// <summary>
		/// Gets the <see cref="string"/> representation of the full encoded url.
		/// </summary>
		/// <returns>The constructed url string.</returns>
		public override string ToString() {
			string query = string.Empty;
			StringBuilder str = new StringBuilder();
			if (BaseUrl != null)
				str.Append(BaseUrl);
			if (Query.Count != 0) {
				var encoded = Query.Cast<KeyValuePair<string, string>>()
								   .Select(p => EncodeKeyValue(p.Key, p.Value));
				str.Append('?');
				str.Append(string.Join("&", encoded));
			}
			if (Fragment != null) {
				str.Append('#');
				str.Append(HttpUtility.UrlEncode(Fragment));
			}
			return str.ToString();
		}

		#endregion

		#region Private Helpers

		/// <summary>
		/// Encodes a single name and value and creates a query assignment.
		/// </summary>
		/// <param name="name">The name to encode.</param>
		/// <param name="value">The value to encode.</param>
		/// <returns>The string representation of the encoded name value assignment.</returns>
		private static string EncodeKeyValue(string name, string value) {
			return string.Concat(HttpUtility.UrlEncode(name), "=", HttpUtility.UrlEncode(value));
		}

		#endregion
	}
}
