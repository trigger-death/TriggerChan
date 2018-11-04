using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.AniList;
using TriggersTools.DiscordBots.TriggerChan.Profiles.Readers.Utils;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers {
	/// <summary>
	/// The anime database user profile reader for the AniList service.
	/// </summary>
	public class AniListProfileReader : IAnimeProfileReader {

		#region Constants

		private const string QueryUrl = @"https://graphql.anilist.co";
		private const string ProfileBaseUrl = @"https://anilist.co/user/";
		private const string AnimeList = @"/animelist";
		private const string MangaList = @"/animelist";

		private const string UsernamePattern = @"^(?:(?:https?\:\/\/)?(?:www.)?anilist\.co\/user\/)?((?:\-|\w)+)\/?$";
		private static readonly Regex UsernameRegex = new Regex(UsernamePattern, RegexOptions.IgnoreCase);

		/// <summary>
		/// The GraphQL Directory.
		/// </summary>
		private static readonly string Directory = Embedding.Combine("TriggersTools.DiscordBots.TriggerChan.Profiles", "Readers", "AniList", "GraphQL");

		#endregion

		#region Fields

		/// <summary>
		/// The query string GraphQL for getting a username from a userid.
		/// </summary>
		private readonly string UserByIdQuery = LoadQuery();
		/// <summary>
		/// The query string GraphQL for getting a userid from a username.
		/// </summary>
		private readonly string UserByNameQuery = LoadQuery();
		/// <summary>
		/// The query string GraphQL for getting a profile from a userid.
		/// </summary>
		private readonly string ProfileByIdQuery = LoadQuery();
		/// <summary>
		/// The query string GraphQL for getting a profile from a username.
		/// </summary>
		private readonly string ProfileByNameQuery = LoadQuery();

		#endregion

		#region Properties

		public DatabaseType Database => DatabaseType.AniList;

		#endregion

		/// <summary>
		/// Parses the user name from the input. Accepts urls, usernames, and user Ids.
		/// </summary>
		/// <param name="input">The input string to parse.</param>
		/// <returns>The parsed username result.</returns>
		public string ParseUsername(string input) {
			Match match = UsernameRegex.Match(input);
			if (match.Success && match.Groups.Count > 0)
				return match.Groups[1].Value;
			else
				return null;
		}

		/// <summary>
		/// Parses the user name from the input. Accepts urls, usernames, and user Ids.
		/// </summary>
		/// <param name="input">The input string to parse.</param>
		/// <returns>The parsed username result.</returns>
		public async Task<ParsedUsername> ParseUsernameAsync(string input) {
			string name = null;
			long? id = null;
			Match match = UsernameRegex.Match(input);
			if (match.Success && match.Groups.Count > 0)
				name = match.Groups[1].Value;
			else
				return ParsedUsername.Failed;

			// Is the name actually an Id? If so, let's get the real name.
			if (long.TryParse(name, out long parsedId)) {
				id = parsedId;
				try {
					name = await QueryUsernameByIdAsync(parsedId).ConfigureAwait(false);
				} catch (HttpStatusException ex) {
					if (ex.StatusCode == HttpStatusCode.NotFound)
						return ParsedUsername.Failed;
					throw;
				}
			}
			else {
				try {
					id = await QueryUserIdByNameAsync(name).ConfigureAwait(false);
				} catch (HttpStatusException ex) {
					if (ex.StatusCode == HttpStatusCode.NotFound)
						return ParsedUsername.Failed;
					throw;
				}
			}
			return new ParsedUsername {
				Name = name,
				Id = id,
			};
		}

		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		public Task<IAnimeProfile> QueryProfileAsync(string usernameOrId) {
			if (long.TryParse(usernameOrId, out long userId)) {
				var variables = new Dictionary<string, object> {
					{ "userid", userId },
				};
				return QueryProfileInternalAsync(ProfileByIdQuery, variables);
			}
			else {
				var variables = new Dictionary<string, object> {
					{ "username", usernameOrId },
				};
				return QueryProfileInternalAsync(ProfileByNameQuery, variables);
			}
		}
		async Task<IDatabaseUserProfile> IDatabaseUserProfileReader.QueryProfileAsync(string usernameOrId) {
			return await QueryProfileAsync(usernameOrId).ConfigureAwait(false);
		}

		#region Private QueryProfile

		private async Task<string> QueryUsernameByIdAsync(long id) {
			var variables = new Dictionary<string, object> {
				{ "userid", id },
			};
			var result = await QueryAsJObject(UserByIdQuery, variables).ConfigureAwait(false);
			User user = result["user"].ToObject<User>();
			return user?.Name;
		}
		private async Task<long?> QueryUserIdByNameAsync(string username) {
			var variables = new Dictionary<string, object> {
				{ "username", username },
			};
			var result = await QueryAsJObject(UserByIdQuery, variables).ConfigureAwait(false);
			User user = result["user"].ToObject<User>();
			return user?.Id;
		}

		private async Task<IAnimeProfile> QueryProfileInternalAsync(string query, Dictionary<string, object> variables) {
			var result = await QueryAsJObject(query, variables).ConfigureAwait(false);
			User user = result["user"].ToObject<User>();
			MediaListCollection anime = result["anime"].ToObject<MediaListCollection>();
			MediaListCollection manga = result["manga"].ToObject<MediaListCollection>();

			return new AnimeProfile {
				DatabaseType = DatabaseType.AniList,
				Username = user?.Name,
				Id = user?.Id,
				Url = GetProfileUrl(user),
				AvatarUrl = user?.Avatar?.Largest,
				Anime = ReadAnimeList(user, anime),
				Manga = ReadMangaList(user, manga),
			};
		}

		private AnimeList ReadAnimeList(User user, MediaListCollection media) {
			UserStats stats = user?.Stats;
			ListScoreStats scores = stats?.AnimeScores;

			AnimeList list = null;
			if (IsValidList(media)) {
				list = new AnimeList();

				ReadMediaList(list, user, scores, media, AnimeList);

				list.MinutesSpent = stats?.WatchedMinutes;
				list.Episodes = media.Episodes;
			}
			return list;
		}
		private MangaList ReadMangaList(User user, MediaListCollection media) {
			UserStats stats = user?.Stats;
			ListScoreStats scores = stats?.MangaScores;

			MangaList list = null;
			if (IsValidList(media)) {
				list = new MangaList();

				ReadMediaList(list, user, scores, media, MangaList);
				
				// TODO: Calculate time based on MAL rules
				list.Chapters = stats?.ChaptersRead;
				list.Volumes = media.Volumes;
			}
			return list;
		}
		private void ReadMediaList(MediaList list, User user, ListScoreStats scores, MediaListCollection media, string listPath) {
			list.MeanScore100 = scores?.MeanScore;
			list.StandardDeviation100 = scores?.StandardDeviation;

			list.Current = media.Current;
			list.Completed = media.Completed;
			list.OnHold = media.Paused;
			list.Dropped = media.Dropped;
			list.Planning = media.Planning;
			list.Repeating = media.Repeating;

			list.Url = GetListUrl(user, listPath);
		}

		private bool IsValidList(MediaListCollection list) => (list?.Lists != null && list.Lists.Any());

		private string GetProfileUrl(User user) {
			string userNameOrId = (user?.Name ?? user?.Id?.ToString());
			if (userNameOrId == null)
				return null;
			return $"{ProfileBaseUrl}{userNameOrId}" ?? user?.ProfileUrl;
		}
		private string GetListUrl(User user, string listPath) {
			string profileUrl = GetProfileUrl(user);
			if (profileUrl == null)
				return null;
			return $"{profileUrl}{listPath}";
		}

		#endregion

		#region Static Helpers

		private static Task<JObject> QueryAsJObject(string query, Dictionary<string, object> variables) {
			return GraphQL.QueryAsJObject(QueryUrl, query, variables);
		}

		private static string LoadQuery([CallerMemberName] string name = null) {
			return Embedding.ReadAllText(Embedding.Combine(Directory, $"{name}.graphql"));
		}

		#endregion
	}
}
