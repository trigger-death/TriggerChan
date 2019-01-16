using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JikanDotNet;
using JikanDotNet.Exceptions;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers {
	/// <summary>
	/// The anime database user profile reader for the MyAnimeList service.
	/// </summary>
	public class MyAnimeListProfileReader : IAnimeProfileReader {
		#region Constants
		
		private const string UsernamePattern = @"^(?:(?:https?\:\/\/)?(?:www.)?myanimelist\.net\/profile\/)?((?:\-|\w)+)\/?$";
		private static readonly Regex UsernameRegex = new Regex(UsernamePattern, RegexOptions.IgnoreCase);
		
		#endregion

		#region Fields

		private readonly Jikan jikan;

		#endregion

		#region Constructors

		public MyAnimeListProfileReader() {
			jikan = new Jikan(true, false);
		}

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
		public Task<ParsedUsername> ParseUsernameAsync(string input) {
			Match match = UsernameRegex.Match(input);
			if (match.Success && match.Groups.Count > 0)
				return Task.FromResult(new ParsedUsername { Name = match.Groups[1].Value });
			else
				return Task.FromResult(ParsedUsername.Failed);
		}
		/// <summary>
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		public Task<IAnimeProfile> QueryProfileAsync(string usernameOrId) {
			try {
				return QueryAnimeProfileInternal(usernameOrId);
			} catch (JikanRequestException ex) {
				throw new HttpStatusException(ex.ResponseCode, ex.Message, ex);
			}
		}
		async Task<IDatabaseUserProfile> IDatabaseUserProfileReader.QueryProfileAsync(string usernameOrId) {
			return await QueryProfileAsync(usernameOrId).ConfigureAwait(false);
		}


		private async Task<IAnimeProfile> QueryAnimeProfileInternal(string username) {
			User user = await jikan.GetUser(username).ConfigureAwait(false);

			return new AnimeProfile {
				DatabaseType = DatabaseType.MyAnimeList,
				Username = user.Username,
				AvatarUrl = user.ImageUrl,
				Url = user.Url,
				Anime = ReadAnime(user.AnimeStats),
				Manga = ReadManga(user.MangaStats),
			};
		}

		private AnimeList ReadAnime(AnimeStats stats) {
			return new AnimeList {
				MeanScore10 = stats.MeanScore,
				DaysSpent = stats.DaysWatched,
				Episodes = stats.EpisodesWatched,
				Current = stats.Watching,
				Completed = stats.Completed,
				OnHold = stats.OnHold,
				Dropped = stats.Dropped,
				Planning = stats.PlanToWatch,
			};
		}
		private MangaList ReadManga(MangaStats stats) {
			return new MangaList {
				MeanScore10 = stats.MeanScore,
				DaysSpent = stats.DaysRead,
				Chapters = stats.ChaptersRead,
				Volumes = stats.VolumesRead,
				Current = stats.Reading,
				Completed = stats.Completed,
				OnHold = stats.OnHold,
				Dropped = stats.Dropped,
				Planning = stats.PlanToRead,
			};
		}
	}
}
