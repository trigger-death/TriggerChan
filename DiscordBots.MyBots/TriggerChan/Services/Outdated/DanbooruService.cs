using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Danbooru;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public partial class DanbooruService : TriggerService {

		#region Constants

		//private const string Domain = @"https://danbooru.donmai.us";

		//private const string FooterText = "danbooru.donmai.us";

		private const string BaseCountsUrl = @"https://danbooru.donmai.us/counts/posts.json";
		private const string BasePostsUrl = @"https://danbooru.donmai.us/posts.json?";
		//private const int PostLimit = 200; // Hard value
		//private const int PageLimit = 1000; // Hard value
		//private const int RetryLimit = 10; // Reduce load

		// You're not suppose to be here, you need to leave
		private static readonly Regex DangerRegex =
			new Regex(@"(^|\s)(loli(?!ta)|shota|bestiality|kids?|kindergarten|preschool|baby|babies)($|\s)",
				RegexOptions.IgnoreCase);
		private static readonly Regex TagRegex =
			new Regex(@"^(?!(_|~))(\w|_|~|-|\(|\))+(?<!_)(|\*)$",
				RegexOptions.IgnoreCase);
		private static readonly Regex RatingRegex =
			new Regex(@"(?:^|\+)(|-)rating\:(s|safe|q|questionable|e|explicit)(?:$|\+)",
				RegexOptions.IgnoreCase);

		// You're not suppose to be here, you need to leave
		/*private const string Danger2Pattern = @"^(loli(?!ta)|shota|bestiality|kids?|kindergarten|preschool|baby|babies)$";
		private static readonly Regex Danger2Regex = new Regex(Danger2Pattern, RegexOptions.IgnoreCase);

		// Bug where \- isn't escaped when used in a range, add it as its own entry.
		private const string Tag2Pattern = @"^[!-+\-.-~]+$";
		private static readonly Regex Tag2Regex = new Regex(Tag2Pattern);

		private const string ArgumentsPattern = @"^Arguments: (?:\*none\*|`(?'args'.+?)`\n";
		private static readonly Regex ArgumentsRegex = new Regex(ArgumentsPattern);*/

		#endregion

		/*#region Fields

		private readonly RestClient rest;
		private readonly int maxTags;

		#endregion

		#region Constructors

		public DanbooruService(TriggerServiceContainer services) : base(services) {
			rest = new RestClient(Domain);
			maxTags = 2;
		}

		public override void Initialize() {
			Credentials = new NetworkCredential() {
				UserName = Config["danbooru_user"],
				Password = Config["danbooru_pass"],
				Domain = Domain,
			};
			Client.ReactionAdded += OnReactionAdded;
		}

#endregion*/


	private WebRequest CreateRequest(string url) {
			WebRequest request = WebRequest.Create(url);
			request.Method = "GET";
			return request;
		}
		private async Task<JObject> GetJObject(string url) {
			WebRequest request = CreateRequest(url);
			string text = ReadStreamFromResponse(await request.GetResponseAsync().ConfigureAwait(false));
			return JObject.Parse(text);
		}
		private async Task<JArray> GetJArray(string url) {
			WebRequest request = CreateRequest(url);
			string text = ReadStreamFromResponse(await request.GetResponseAsync().ConfigureAwait(false));
			return JArray.Parse(text);
		}
		private static string ReadStreamFromResponse(WebResponse response) {
			using (Stream responseStream = response.GetResponseStream())
			using (var sr = new StreamReader(responseStream))
				return sr.ReadToEnd();
		}

		/*private static string RatingToTag(DanbooruRating rating) {
			switch (rating) {
			case DanbooruRating.Any:			 return "";
			case DanbooruRating.Safe:			 return "rating:s";
			case DanbooruRating.Questionable:	 return "rating:q";
			case DanbooruRating.Explicit:		 return "rating:e";
			case DanbooruRating.NotSafe:		 return "-rating:s";
			case DanbooruRating.NotQuestionable: return "-rating:q";
			case DanbooruRating.NotExplicit:	 return "-rating:e";
			default: return null;
			}
		}*/
		
		private string CheckTag(DanbooruRating rating, string tag) {
			if (string.IsNullOrWhiteSpace(tag))
				return null;
			string fmtTag = tag.ToLower().Replace(' ', '_');
			if (!TagRegex.IsMatch(fmtTag))
				throw new InvalidTagsException(tag);
			if (rating != DanbooruRating.Safe && DangerRegex.IsMatch(fmtTag))
				throw new IllegalNsfwTagsException(tag);
			return fmtTag;
		}

		private string CombineTags(DanbooruRating rating, params string[] tags) {
			string tagsStr = "";
			int count = 0;
			foreach (string tag in tags) {
				if (!string.IsNullOrWhiteSpace(tag)) {
					if (!string.IsNullOrEmpty(tagsStr))
						tagsStr += "+";
					tagsStr += tag;
					count++;
				}
			}
			// Safe rating can be applied no matter the limit
			if (rating == DanbooruRating.Safe || count <= 1) {
				if (!string.IsNullOrEmpty(tagsStr))
					tagsStr += "+";
				tagsStr += RatingToTag(rating);
			}
			if (string.IsNullOrWhiteSpace(tagsStr))
				return null;
			return tagsStr.ToLower();
		}

		/*private async Task OnReactionAdded2(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
			var emoji = reaction.Emote;
			if (reaction.UserId == Client.CurrentUser.Id) return;
			if (!IsLiveDanbooruEmbed(msg.Value, out string arguments, out ulong isOld, out var author)) return;
			var message = msg.Value ?? await msg.DownloadAsync().ConfigureAwait(false);
			if (emoji.Equals(TriggerReactions.Agreeable) || emoji.Equals(TriggerReactions.Dangerous)) {
				//var message = msg.Value ?? await msg.DownloadAsync().ConfigureAwait(false);
				bool disagree = emoji.Equals(TriggerReactions.Dangerous);
				// Confirm this is a danbooru post made by the bot
				if (message.Author.Id == Client.CurrentUser.Id &&
					message.Embeds.Any() && message.Embeds.First().Url.StartsWith(Domain) &&
					message.Embeds.First().Description.Contains("Score:")) // Some method to know that its an active image
				{
					IEmbed oldEmbed = message.Embeds.First();
					if ((await ScoreReactions(message).ConfigureAwait(false)) < 0) {
						try {
							await message.ModifyAsync(m => {
								IUser user = reaction.User.Value;
								var embed = MakeBaseEmbed(author);
								int index = oldEmbed.Description.IndexOf("Score:");
								string desc = oldEmbed.Description.Substring(0, index);
								if (disagree) {
									if (!desc.EndsWith("\n"))
										desc += "\n";
									desc += $"Post deemed against Discord's policies by {Format.Sanitize(user.Username)}#{user.Discriminator}";
								}
								embed.WithDescription(desc);
								m.Embed = embed.Build();
							}).ConfigureAwait(false);
							await message.RemoveAllReactionsAsync().ConfigureAwait(false);
						}
						catch (Exception ex) {
							Console.WriteLine(ex);
						}
					}
				//}
			}
			else if (emoji.Equals(TriggerReactions.Retry)) {

			}
		}*/

		/*public async Task<int> ScoreReactions(IUserMessage message) {
			var agreesEnum = await message.GetReactionUsersAsync(TriggerReactions.Agreeable, 100).FirstOrDefault().ConfigureAwait(false);
			var disagreesEnum = await message.GetReactionUsersAsync(TriggerReactions.Dangerous, 100).FirstOrDefault().ConfigureAwait(false);
			using (var db = GetDb<TriggerDbContext>()) {
				int agrees = 0;
				int disagrees = 0;
				if (agreesEnum != null) {
					foreach (var user in agreesEnum) {
						if (user.IsBot)
							continue;
						if (!(await db.FindUserAsync(user.Id).ConfigureAwait(false)).Banned)
							agrees++;
					}
				}
				if (disagreesEnum != null) {
					foreach (var user in disagreesEnum) {
						if (user.IsBot)
							continue;
						if (!(await db.FindUserAsync(user.Id).ConfigureAwait(false)).Banned)
							disagrees++;
					}
				}
				return agrees - disagrees;
			}
			//int agrees = (await message.GetReactionUsersAsync(TriggerReactions.Agreeable, 100).FirstOrDefault().ConfigureAwait(false))?.Count ?? 0;
			//int disagrees = (await message.GetReactionUsersAsync(TriggerReactions.Dangerous, 100).FirstOrDefault().ConfigureAwait(false))?.Count ?? 0;
			//return agrees - disagrees;
		}*/

		private static void AppendToQuery(ref string url, string name, object parameter) {
			if (parameter == null)
				return;
			if (url.EndsWith("?") || url.EndsWith("&"))
				url += $"{name}={parameter}";
			else if (url.Contains('?')) // First parameter already exists
				url += $"&{name}={parameter}";
			else
				url += $"?{name}={parameter}";
		}
		
		public async Task<DanbooruPost> SearchAsync(DanbooruRating rating, string tag1, string tag2, int minScore, int retries, IEnumerable<string> extraTagsIn) {
			
			// Perform checks and formatting on tags
			tag1 = CheckTag(rating, tag1);
			tag2 = CheckTag(rating, tag2);

			List<string> extraTags = new List<string>();
			foreach (string tag in extraTagsIn) {
				if (string.IsNullOrWhiteSpace(tag))
					continue;
				extraTags.Add(CheckTag(rating, tag));
			}


			string countsQueryUrl = BaseCountsUrl;
			string postsQueryUrl = BasePostsUrl;

			string tags = CombineTags(rating, tag1, tag2);
			UrlBuilder countQuery = new UrlBuilder { BaseUrl = BaseCountsUrl };
			UrlBuilder postsQuery = new UrlBuilder { BaseUrl = BasePostsUrl };
			countQuery.Add("tags", tags);
			postsQuery.Add("tags", tags);
			postsQuery.Add("limit", PostLimit);
			AppendToQuery(ref countsQueryUrl, "tags", tags);
			AppendToQuery(ref postsQueryUrl, "tags", tags);

			AppendToQuery(ref postsQueryUrl, "limit", PostLimit);

			JObject counts = await GetJObject(countsQueryUrl).ConfigureAwait(false);
			int count = int.Parse((string) counts["counts"]["posts"]);
			if (count == 0)
				return null;

			int maxPages = Math.Min(PageLimit, count / PostLimit);
			// Avoid searching the last page if it has less results
			if (count % PostLimit != 0 && retries > count / PostLimit)
				maxPages++;

			// Method that relies less on Random
			retries = Math.Min(RetryLimit, retries);
			if (maxPages <= 100) {
				List<int> availablePages = new List<int>();
				for (int i = 1; i <= maxPages; i++)
					availablePages.Add(i);
				for (int i = 0; i < retries && availablePages.Any(); i++) {
					int listIndex = Random.Next(availablePages.Count);
					int page = availablePages[listIndex];
					DanbooruPost post = await SearchPage(page, rating, minScore, postsQueryUrl, extraTags).ConfigureAwait(false);
					if (post != null)
						return post;
					availablePages.RemoveAt(listIndex);
				}
			}
			else {
				HashSet<int> triedPages = new HashSet<int>();
				for (int i = 0; i < retries; i++) {
					int page = Random.Next(maxPages) + 1;
					if (triedPages.Add(page)) {
						DanbooruPost post = await SearchPage(page, rating, minScore, postsQueryUrl, extraTags).ConfigureAwait(false);
						if (post != null)
							return post;
					}
				}
			}
			return null;
			/*JArray results = await GetJArray(postsQueryUrl);

			if (results.Count == 0)
				return null;

			List<int> available = new List<int>();
			for (int i = 0; i < results.Count; i++)
				available.Add(i);

			int index = Random.Next(results.Count);
			int listIndex = index;
			JToken result = results[index];
			
			while (!IsValid(result, rating, extraTags)) {
				results.Remove(index.ToString());
				available.RemoveAt(listIndex);
				if (!available.Any())
					return null;
				listIndex = Random.Next(available.Count);
				index = available[listIndex];
				result = results[index];
			}

			return result.ToObject<DanbooruPost>();*/
		}
		

		/*public EmbedBuilder MakeBaseEmbed() {
			var embed = new EmbedBuilder() {
				Title = "Danbooru Post Result",
				Color = new Color(186, 149, 113),
				//Color = new Color(164, 128, 95),
			};
			embed.WithFooter(FooterText, @"https://i.imgur.com/fjMmTn4.png");
			return embed;
		}*/
		
		public async Task<DanbooruPost> SearchPage(int page, DanbooruRating rating, int minScore, string queryUrl, IEnumerable<string> extraTags) {
			AppendToQuery(ref queryUrl, "page", page);
			
			JArray results = await GetJArray(queryUrl).ConfigureAwait(false);

			if (results.Count == 0)
				return null;

			List<int> available = new List<int>();
			for (int i = 0; i < results.Count; i++)
				available.Add(i);

			int index = Random.Next(results.Count);
			int listIndex = index;
			JToken result = results[index];

			while (!IsValid(result, rating, minScore, extraTags)) {
				results.Remove(index.ToString());
				available.RemoveAt(listIndex);
				if (!available.Any())
					return null;
				listIndex = Random.Next(available.Count);
				index = available[listIndex];
				result = results[index];
			}

			return result.ToObject<DanbooruPost>();
		}

		/*public async Task AddReactions(IUserMessage message) {
			//await message.AddReactionAsync(TriggerReactions.Agreeable).ConfigureAwait(false);
			await message.AddReactionAsync(TriggerReactions.Dangerous).ConfigureAwait(false);
			await message.AddReactionAsync(TriggerReactions.Retry).ConfigureAwait(false);
		}*/

		public bool IsValid(JToken json, DanbooruRating rating, int minScore, IEnumerable<string> extraTags) {
			int score = int.Parse((string) json["score"]);
			if (score < minScore)
				return false;
			string ratingStr = (string) json["rating"];
			switch (ratingStr) {
			case "s":
				if (!rating.HasFlag(DanbooruRating.Safe))
					return false;
				break;
			case "q":
				if (!rating.HasFlag(DanbooruRating.Questionable))
					return false;
				break;
			case "e":
				if (!rating.HasFlag(DanbooruRating.Explicit))
					return false;
				break;
			}
			string tags = (string) json["tag_string"];

			// Stranger Danger Detector
			if (DangerRegex.IsMatch(tags))
				return false; // No NSFW loli shit

			foreach (string tag in extraTags) {
				bool openEnded = tag.EndsWith("*");
				string lower = tag.ToLower();
				if (openEnded)
					lower = lower.Substring(0, lower.Length - 1);
				Regex regex = new Regex(@"(^|\s)" + tag + @"($|\s)");
				if (tag.StartsWith("-")) {
					lower = lower.Substring(1);
					if (openEnded) {
						if (tags.Contains(lower))
							return false;
					}
					else if (regex.IsMatch(tags))
						return false;
				}
				else if (openEnded) {
					if (!tags.Contains(lower))
						return false;
				}
				else if (!regex.IsMatch(tags))
					return false;
			}
			return true;
		}
	}
}
