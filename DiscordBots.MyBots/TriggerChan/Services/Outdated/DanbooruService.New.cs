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
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Danbooru;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Reactions;
using TriggersTools.DiscordBots.Utils;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	partial class DanbooruService {
		#region Constants

		private const string Domain = @"https://danbooru.donmai.us";

		private const string FooterText = "danbooru.donmai.us";
		
		private const int PostLimit = 200; // Hard value
		private const int PageLimit = 1000; // Hard value
		private const int RetryLimit = 10; // Reduce load
		
		// You're not suppose to be here, you need to leave
		private const string Danger2Pattern = @"^(loli(?!ta)|shota|bestiality|kids?|kindergarten|preschool|baby|babies)$";
		private static readonly Regex Danger2Regex = new Regex(Danger2Pattern, RegexOptions.IgnoreCase);



		// Bug where \- isn't escaped when used in a range, add it as its own entry.
		//private const string Tag2Pattern = @"^[!-+\-.-~]+$";
		private const string Tag2Pattern = @"^[!-\[\]-_a-}]+$";
		private static readonly Regex Tag2Regex = new Regex(Tag2Pattern);

		private const string ArgumentsPattern = @"^Arguments: `(?'args'.+?)`\n";
		private static readonly Regex ArgumentsRegex = new Regex(ArgumentsPattern);

		#endregion

		#region Fields

		private readonly RestClient rest;
		private readonly int maxTags;

		#endregion

		#region Constructors

		public DanbooruService(TriggerServiceContainer services) : base(services) {
			rest = new RestClient(Domain);
			maxTags = 2;
		}

		#endregion

		public override void Initialize() {
			Client.ReactionAdded += OnReactionAddedAsync;
		}

		private static string RatingToTag(DanbooruRating rating) {
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
		}
		private string ValidateTag(string tag, DanbooruRating rating) {
			tag = tag.ToLower().Replace(' ', '_');
			if (!Tag2Regex.IsMatch(tag))
				throw new InvalidTagsException(tag);
			if (rating != DanbooruRating.Safe && Danger2Regex.IsMatch(tag))
				throw new IllegalNsfwTagsException(tag);
			return tag;
		}

		public async Task<RuntimeResult> CreateSearchAsync(ICommandContext context, bool hentai, string arguments) {
			if (!TryParseArguments(hentai, ref arguments, out DanbooruRating rating, out string[] tags, out int minScore, out int retries))
				return EmoteResults.FromInvalidArgument();
			EmbedAuthorBuilder author = new EmbedAuthorBuilder {
				Name = context.User.Username,
				IconUrl = context.User.GetAvatarUrl(),
			}.SetAuthorId(context.User.Id);
			Embed embed = BuildSearchingEmbed(author, arguments);
			IUserMessage message = await context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
			Task task = Task.Run(async () => {
				await PerformSearchAsync(author, message, arguments, rating, tags, minScore, retries, true).ConfigureAwait(false);
			});

			return NormalResult.FromSuccess();
		}
		private struct UserReacts {
			public IUser User { get; set; }
			public List<IEmote> Emotes { get; }

			public UserReacts(IUser user) {
				User = user;
				Emotes = new List<IEmote>();
			}
			public UserReacts(IUser user, IEmote emote) {
				User = user;
				Emotes = new List<IEmote> { emote };
			}
		}

		private async Task PerformSearchAsync(EmbedAuthorBuilder author, IUserMessage message, string arguments, DanbooruRating rating, string[] tags, int minScore, int retries, bool first = false) {
			DanbooruPost post = Search(rating, tags, minScore, retries, out bool pagesLeft);
			await message.ModifyAsync(m => {
				if (post == null)
					m.Embed = BuildNoPostEmbed(author, arguments, pagesLeft);
				else
					m.Embed = BuildPostEmbed(author, post, arguments);
			}).ConfigureAwait(false);
			if (!first)
				await message.RemoveAllReactionsAsync().ConfigureAwait(false);
			List<IEmote> newReactions = new List<IEmote> {
				TriggerReactions.Retry,
			};
			if (post != null) {
				newReactions.Add(TriggerReactions.DoAgain);
				//newReactions.Add(TriggerReactions.Agreeable);
				newReactions.Add(TriggerReactions.NoGood);
			}
			if (post != null || pagesLeft)
				await message.AddReactionsAsync(newReactions.ToArray()).ConfigureAwait(false);
		}

		private bool TryParseArguments(bool hentai, ref string arguments, out DanbooruRating rating, out string[] tags, out int minScore, out int retries) {
			if (hentai)
				rating = DanbooruRating.NotSafe;
			else
				rating = DanbooruRating.Safe;
			tags = new string[0];
			minScore = 5;
			retries = 2;
			bool tagsSpecified = false;
			bool ratingSpecified = false;
			bool minScoreSpecified = false;
			bool retriesSpecified = false;
			List<string> tagsList = new List<string>();
			string[] args = arguments.Split(new char[0]); // This splits by whitespace
			foreach (string arg in args) {
				string[] parts = arg.Split(new char[] { '=' });
				if (parts.Length == 1 || parts[0].ToLower() == "tags") {
					if (tagsSpecified) {
						return false;
						//return EmoteResults.FromInvalidArgument("Main tags already specified");
					}
					tagsSpecified = true;
					if (!string.IsNullOrEmpty(parts.Last())) {
						tagsList.AddRange(parts[parts.Length - 1].Split(','));
						/*if (tags.Length > 2) {
							return EmoteResults.FromInvalidArgument("More than two main tags");
						}
						else {
							tag1 = tags[0];
							if (tags.Length == 2)
								tag2 = tags[1];
						}*/
					}
				}
				else {
					switch (parts[0].ToLower()) {
					case "rating":
						if (parts[1].Length == 0) {
							return false;
							//return EmoteResults.FromInvalidArgument("No ratings specified");
						}
						if (ratingSpecified) {
							return false;
							//return EmoteResults.FromInvalidArgument("Rating already specified");
						}
						rating = DanbooruRating.None;
						foreach (char c in parts[1]) {
							switch (c) {
							case 's': rating |= DanbooruRating.Safe; break;
							case 'q': rating |= DanbooruRating.Questionable; break;
							case 'e': rating |= DanbooruRating.Explicit; break;
							default:
								return false;
								//return EmoteResults.FromInvalidArgument($"Unknown rating character `{c}`");
							}
						}
						ratingSpecified = true;
						break;
					case "retries":
						if (retriesSpecified) {
							return false;
							//return EmoteResults.FromInvalidArgument("Retries already specified");
						}
						if (int.TryParse(parts[1], out retries) && retries >= 1) {
							retriesSpecified = true;
						}
						else {
							return false;
							//return EmoteResults.FromInvalidArgument("Invalid retries");
						}
						break;
					case "minscore":
						if (minScoreSpecified) {
							return false;
							//return EmoteResults.FromInvalidArgument("Min Score already specified");
						}
						if (int.TryParse(parts[1], out minScore)) {
							minScoreSpecified = true;
						}
						else {
							return false;
							//return EmoteResults.FromInvalidArgument("Invalid min score");
						}
						break;
					default:
						return false;
						//return EmoteResults.FromInvalidArgument($"Unknown paramtere `{parts[0]}`");
					}
				}
			}
			if (!ratingSpecified) {
				StringBuilder ratingStr = new StringBuilder("rating=");
				if (rating.HasFlag(DanbooruRating.Safe)) ratingStr.Append('s');
				if (rating.HasFlag(DanbooruRating.Questionable)) ratingStr.Append('q');
				if (rating.HasFlag(DanbooruRating.Explicit)) ratingStr.Append('e');
				arguments = $"{ratingStr} {arguments}";
			}
			tags = tagsList.ToArray();
			return true;
		}


		private string PrepareTags(string[] tags, DanbooruRating rating, out Regex[] extraTags) {
			IEnumerable<string> tagsEnumerable = tags;
			int takeLength = Math.Min(maxTags, tags.Length);

			/*if (rating == DanbooruRating.Safe || tags.Length < maxTags) {
				tagsEnumerable = new[] { RatingToTag(rating) }.Concat(tagsEnumerable);
				takeLength = Math.Min(maxTags + 1, tags.Length + 1);
			}*/
			tagsEnumerable = tagsEnumerable.Take(takeLength)
										   .Select(t => ValidateTag(t, rating))
										   .Select(t => HttpUtility.UrlEncode(t));
			extraTags = tagsEnumerable.Skip(takeLength)
									  .Select(t => ValidateTag(t, rating))
									  .Select(t => ExtraTagToRegex(t))
									  .ToArray();
			return string.Join("+", tagsEnumerable);
		}

		private bool IsLiveDanbooruEmbed(IUserMessage message, out string arguments, out ulong authorId, out EmbedAuthorBuilder author) {
			arguments = "";
			authorId = 0;
			author = null;
			if (message.Author.Id != Client.CurrentUser.Id) return false;
			if (message.Embeds.Count != 1) return false;
			IEmbed embed = message.Embeds.First();
			if (!embed.Footer.HasValue) return false;
			if (embed.Description == null) return false;
			EmbedFooter footer = embed.Footer.Value;
			if (footer.Text != FooterText) return false;
			Match match = ArgumentsRegex.Match(embed.Description);
			if (!match.Success) return false;
			if (embed.Author.HasValue) {
				authorId = embed.GetAuthorId();
				author = new EmbedAuthorBuilder {
					Name = embed.Author.Value.Name,
					Url = embed.Author.Value.Url,
					IconUrl = embed.Author.Value.IconUrl,
				};
			}
			Group group = match.Groups["args"];
			if (group.Success)
				arguments = /*DesanitizeQuote(*/group.Value;//);
			return true;
		}

		private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction) {
			if (reaction.UserId == Client.CurrentUser.Id) return;
			if (!reaction.User.IsSpecified) return;
			var emoji = reaction.Emote;
			IUser user = reaction.User.Value;
			var message = msg.Value ?? await msg.DownloadAsync().ConfigureAwait(false);
			if (!IsLiveDanbooruEmbed(message, out string arguments, out ulong authorId, out var author)) return;
			// Old Police Car 🚔
			if ((authorId == 0 && emoji.Equals(new Emoji("\U0001F694"))) || emoji.Equals(TriggerReactions.NoGood)) {
				IEmbed oldEmbed = message.Embeds.First();
				/*var embed = MakeBaseEmbed();
				if (string.IsNullOrWhiteSpace(arguments))
					arguments = "*none*";
				else
					arguments = $"`{Format.Sanitize(arguments)}`";
				embed.Description = $"Arguments: {arguments}\n" +
									$"Post deemed against Discord's policies by {Format.Sanitize(user.Username)}#{user.Discriminator}";*/
				string url = oldEmbed.Url;
				if (url == null)
					url = oldEmbed.Author.Value.Url;
				await message.ModifyAsync(m => {
					m.Embed = BuildDiscardedEmbed(author, url, arguments, user);
				}).ConfigureAwait(false);
				await message.RemoveAllReactionsAsync().ConfigureAwait(false);
			}
			else if (authorId == user.Id && emoji.Equals(TriggerReactions.Retry)) {
				await message.ModifyAsync(m => {
					m.Embed = BuildSearchingEmbed(author, arguments);
				}).ConfigureAwait(false);
				if (!TryParseArguments(false, ref arguments, out DanbooruRating rating, out string[] tags, out int minScore, out int retries))
					return;
				Task task = Task.Run(async () => {
					await PerformSearchAsync(author, message, arguments, rating, tags, minScore, retries).ConfigureAwait(false);
				});
			}
			else if (authorId == user.Id && emoji.Equals(TriggerReactions.DoAgain)) {
				if (!TryParseArguments(false, ref arguments, out DanbooruRating rating, out string[] tags, out int minScore, out int retries))
					return;
				author = new EmbedAuthorBuilder {
					Name = user.Username,
					IconUrl = user.GetAvatarUrl(),
				}.SetAuthorId(user.Id);
				Embed embed = BuildSearchingEmbed(author, arguments);
				message = await channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
				Task task = Task.Run(async () => {
					await PerformSearchAsync(author, message, arguments, rating, tags, minScore, retries, true).ConfigureAwait(false);
				});
			}
		}

		public DanbooruPost Search(DanbooruRating rating, string[] tags, int minScore, int retries, out bool pagesLeft) {
			pagesLeft = false;
			string tagsStr = PrepareTags(tags, rating, out var extraTags);
			var countRequest = new RestRequest("counts/posts.json", Method.GET);
			//if (tagsStr.Length != 0)
				countRequest.AddQueryParameter("tags", tagsStr, false);
			
			var result = rest.Execute(countRequest);
			if (!result.IsSuccessful)
				return null;

			int count = JsonConvert.DeserializeObject<DanbooruCount>(result.Content);
			if (count == 0)
				return null;

			int pageCount = (int) Math.Ceiling((double) count / PageLimit);
			int allowedRetries = retries;
			// If the last page has less results, we'll be generous
			// and give it a chance at being searched too.
			if (count % PostLimit != 0)
				allowedRetries++;
			allowedRetries = Math.Min(allowedRetries, pageCount);

			var postRequest = new RestRequest("posts.json", Method.GET);
			postRequest.AddQueryParameter("tags", tagsStr, false);
			postRequest.AddQueryParameter("limit", PostLimit.ToString());
			postRequest.AddQueryParameter("page", "0");

			HashSet<int> triedPages = new HashSet<int>();
			for (int i = 0; i < allowedRetries; i++) {
				int page = Random.Next(pageCount) + 1;
				if (triedPages.Add(page)) {
					DanbooruPost post = SearchPage(postRequest, page, rating, minScore, extraTags);
					if (post != null)
						return post;
				}
			}
			pagesLeft = allowedRetries < pageCount;
			return null;
		}

		private Regex ExtraTagToRegex(string tag) {
			return new Regex($"^{Regex.Escape(tag).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase);
		}

		public EmbedBuilder MakeBaseEmbed(EmbedAuthorBuilder author) {
			var embed = new EmbedBuilder {
				Title = "Danbooru Post Result",
				Color = new Color(186, 149, 113),
				Author = author,
			};
			embed.WithFooter(FooterText, @"https://i.imgur.com/fjMmTn4.png");
			return embed;
		}
		private string FormatArguments(string arguments) {
			return Regex.Replace(arguments, @"\s+", " ").Trim();
		}

		public Embed BuildSearchingEmbed(EmbedAuthorBuilder author, string arguments) {
			var embed = MakeBaseEmbed(author);
			arguments = FormatArguments(arguments);
			embed.Title = null;
			embed.Description =
				$"🔎 Searching for a random danbooru image...";
			return embed.Build();
		}
		public Embed BuildDiscardedEmbed(EmbedAuthorBuilder author, string postUrl, string arguments, IUser user) {
			var embed = MakeBaseEmbed(author);
			arguments = FormatArguments(arguments);
			embed.Url = postUrl;
			embed.Description =
				$"Arguments: `{arguments}`\n\n" +
				$"Post deemed against Discord's policies by {Format.Sanitize(user.Username)}#{user.Discriminator}";
			return embed.Build();
		}
		public Embed BuildPostEmbed(EmbedAuthorBuilder author, DanbooruPost post, string arguments) {
			var embed = MakeBaseEmbed(author);
			arguments = FormatArguments(arguments);
			embed.Url = post.PostUrl;
			embed.Description =
				$"Arguments: `{arguments}`\n" +
				$"Score: **{post.Score}** | Rating: **{post.Rating}** | Image: **[Link]({post.FileUrl})**";
			embed.ImageUrl = post.LargeFileUrl;
			return embed.Build();
		}
		public Embed BuildNoPostEmbed(EmbedAuthorBuilder author, string arguments, bool pagesLeft) {
			var embed = MakeBaseEmbed(author);
			arguments = FormatArguments(arguments);
			embed.Description =
				$"Arguments: `{arguments}`\n\n" +
				$"No matching results found";
			if (pagesLeft)
				embed.Description += " - Some pages were left unchecked";
			return embed.Build();
		}

		public DanbooruPost SearchPage(RestRequest request, int page, DanbooruRating rating, int minScore, Regex[] extraTags) {
			request.AddOrUpdateParameter("page", page);
			
			var result = rest.Execute(request);
			if (!result.IsSuccessful)
				return null;
			List<DanbooruPost> posts = JsonConvert.DeserializeObject<List<DanbooruPost>>(result.Content);
			if (posts.Count == 0)
				return null;
			
			// Randomly choose posts that match
			int count = posts.Count;
			for (int i = 0; i < count; i++) {
				int index = Random.Next(posts.Count);
				if (IsValidPost(posts[index], rating, minScore, extraTags))
					return posts[index];
				posts.RemoveAt(index);
			}
			return null;
		}
		
		public bool IsValidPost(DanbooruPost post, DanbooruRating rating, int minScore, Regex[] extraTags) {
			if (post.IsBanned)
				return false; // We won't be able to see the image. NEXT!
			if (post.Score < minScore)
				return false; // We don't want the post to be this bad. NEXT!
			if (!rating.HasFlag(post.Rating))
				return false; // This is for church honey! NEXT!
			
			List<Regex> extraTagsList = new List<Regex>(extraTags);

			string[] tags = post.Tags;
			for (int i = 0; i < tags.Length; i++) {
				string tag = tags[i];

				// Stranger Danger Detector
				if (Danger2Regex.IsMatch(tag))
					return false; // No NSFW loli shit

				// Extra tags for people who are greedy with their search results
				for (int j = 0; j < extraTagsList.Count; j++) {
					if (extraTagsList[j].IsMatch(tag)) {
						extraTagsList.RemoveAt(j);
						break;
					}
				}
			}
			// Make sure all extra tags are satisfied
			return extraTagsList.Count == 0;
		}
	}
}
