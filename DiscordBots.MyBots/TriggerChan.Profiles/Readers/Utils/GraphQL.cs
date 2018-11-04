using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Utils {
	/// <summary>
	/// Static helper methods for GraphQL.
	/// </summary>
	internal static class GraphQL {
		/// <summary>
		/// Runs a GraphQL query and gets the result as a <see cref="JObject"/>.
		/// </summary>
		/// <param name="url">The request url.</param>
		/// <param name="query">The graphQL query.</param>
		/// <param name="variables">The list of variables to add.</param>
		/// <returns>The loaded <see cref="JObject"/></returns>
		/// 
		/// <exception cref="HttpStatusException">
		/// An Http error occurred.
		/// </exception>
		/// <exception cref="GraphQLException">
		/// A GraphQL error occurred.
		/// </exception>
		public static async Task<JObject> QueryAsJObject(string url, string query, Dictionary<string, object> variables) {
			using (HttpClient client = new HttpClient())
			using (HttpRequestMessage request = CreateRequest(url, query, variables)) {
				HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
				if (response.IsSuccessStatusCode) {
					string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					var result = JObject.Parse(json);
					if (result["data"] != null)
						return (JObject) result["data"];
					var errors = result["errors"].ToObject<List<GraphQLError>>();
					if (errors.Any())
						throw new HttpStatusException((HttpStatusCode) errors[0].Status);
					throw new GraphQLException(errors);
				}
				throw new HttpStatusException(response.StatusCode);
			}
		}

		/// <summary>
		/// Creates a GraphQL query Http request.
		/// </summary>
		/// <param name="url">The request url.</param>
		/// <param name="query">The graphQL query.</param>
		/// <param name="variables">The list of variables to add.</param>
		/// <returns>The created <see cref="HttpRequestMessage"/>.</returns>
		private static HttpRequestMessage CreateRequest(string url, string query, Dictionary<string, object> variables) {
			return new HttpRequestMessage(HttpMethod.Post, url) {
				Content = new FormUrlEncodedContent(new Dictionary<string, string> {
					{ "query", query },
					{ "variables", JsonConvert.SerializeObject(variables) },
				}),
			};
		}
	}
}
