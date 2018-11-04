using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AniListClient {
	public class AniListProfile {

		private const string BaseUrl = @"https://graphql.anilist.co";
		const string UserQuery = @"
			query($username: String) {
				User(name: $username) {
					avatar {
						large
						medium
					}
					siteUrl
					stats {
						watchedTime
						chaptersRead
						animeListScores {
							meanScore
						}
						mangaListScores {
							meanScore
						}
					}
				}
			}";
		const string AnimeQuery = @"
			query($username: String) {
				MediaListCollection(userName: $username, type: ANIME) {
					lists {
						entries {
							progress
						}
						status
					}
				}
			}";
		const string MangaQuery = @"
			query($username: String) {
				MediaListCollection(userName: $username, type: MANGA) {
					lists {
						entries {
							progressVolumes
						}
						status
					}
				}
			}";


		private static WebRequest CreateRequest(string url) {
			WebRequest request = WebRequest.Create(url);
			request.Method = "POST";
			return request;
		}

		private static async Task<JObject> GetJObject(string url) {
			WebRequest request = CreateRequest(url);
			string text = ReadStreamFromResponse(await request.GetResponseAsync());
			return JObject.Parse(text);
		}

		private static async Task<JArray> GetJArray(string url) {
			WebRequest request = CreateRequest(url);
			string text = ReadStreamFromResponse(await request.GetResponseAsync());
			return JArray.Parse(text);
		}

		private static JObject GetJson(WebResponse response) {
			string text = ReadStreamFromResponse(response);
			return JObject.Parse(text);
		}

		private static JArray GetJsonArray(WebResponse response) {
			string text = ReadStreamFromResponse(response);
			return JArray.Parse(text);
		}

		private static string ReadStreamFromResponse(WebResponse response) {
			using (Stream responseStream = response.GetResponseStream())
			using (var sr = new StreamReader(responseStream))
				return sr.ReadToEnd();
		}

		private static HttpRequestMessage CreateRequest(string query, Dictionary<string, string> variables) {
			return new HttpRequestMessage(HttpMethod.Post, BaseUrl) {
				Content = new FormUrlEncodedContent(new Dictionary<string, string> {
					{ "query", query },
					{ "variables", JsonConvert.SerializeObject(variables) },
				}),
			};
		}

		private static async Task<T> FromJObject<T>(string query, Dictionary<string, string> variables) {
			using (HttpClient client = new HttpClient())
			using (HttpRequestMessage request = CreateRequest(query, variables)) {
				HttpResponseMessage response = await client.SendAsync(request);
				if (response.IsSuccessStatusCode) {
					string json = await response.Content.ReadAsStringAsync();
					return JObject.Parse(json)["data"][typeof(T).Name].ToObject<T>();
				}
				throw new HttpRequestException($"HTTP Status Code: {response.StatusCode}");
			}
		}
		
		public static async Task<AniListProfile> LoadProfile(string username) {
			var variables = new Dictionary<string, string> {
				{ "username", username },
			};

			User user = await FromJObject<User>(UserQuery, variables);
			MediaListCollection anime = await FromJObject<MediaListCollection>(AnimeQuery, variables);
			MediaListCollection manga = await FromJObject<MediaListCollection>(MangaQuery, variables);

			return new AniListProfile {
				UserName = username,
				ProfileUrl = user.ProfileUrl,
				AvatarUrl = user.Avatar.Medium,
				AnimeList = new AniListStats {
					DaysSpent = user.Stats.WatchedTime.TotalDays,
					MeanScore = user.Stats.AnimeListScores.MeanScore10,
					Completed = anime.Completed,
					Episodes = anime.Episodes,
				},
				MangaList = new AniListStats {
					MeanScore = user.Stats.MangaListScores.MeanScore10,
					Completed = manga.Completed,
					Episodes = user.Stats.ChaptersRead,
					Volumes = manga.Volumes,
				},
			};
		}

		public string UserName { get; set; }
		public string AvatarUrl { get; set; }
		public string ProfileUrl { get; set; }

		public AniListStats AnimeList { get; set; }
		public AniListStats MangaList { get; set; }
	}
}
