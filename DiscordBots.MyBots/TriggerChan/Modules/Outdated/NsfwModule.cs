using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Commands;
using TriggersTools.DiscordBots.TriggerChan.Danbooru;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("NSFW")]
	[Summary("Commands for looking up NSFW material")]
	[IsLockable(true)]
	[RequiresNsfw]
	public class NsfwModule : TriggerModule {

		private readonly DanbooruService danbooru;

		public NsfwModule(TriggerServiceContainer services,
						  DanbooruService danbooru)
			: base(services)
		{
			this.danbooru = danbooru;
		}

		[Command("danbooru")]
		[Usage("[rating=sqe] [[tags=]tag1,tag2,...] [minscore=5] [retries=1-10]")]
		[Summary("Looks up a random image on danbooru matching the search terms")]
		[Remarks("Danbooru only allows searching for two main tags at once. " +
			"Everything specified in extras will be manually searched for based on retries x 200 posts. " +
			"Tags can start with '-' to be blacklisted and use '*' as a wildcard. " +
			"Ratings can be any combination of 's' (safe), 'q' (questionable), and 'e' (explicit).")]
		public Task<RuntimeResult> DanbooruSearch([Remainder] string arguments = "") {
			return danbooru.CreateSearchAsync(Context, false, arguments);
			//return DanbooruSearchAsync(true, DanbooruRating.Any, arguments);
		}

		[Command("hentai")]
		[Usage("[rating=sqe] [[tags=]tag1,tag2,...] [minscore=5] [retries=1-10]")]
		[Summary("Looks up a random (nsfw by default) image on danbooru matching the search terms")]
		[Remarks("Danbooru only allows searching for two main tags at once. " +
			"Everything specified after the first two will be manually searched for based on retries x 200 posts. " +
			"Tags can start with '-' to be blacklisted and use '*' as a wildcard. " +
			"Ratings can be any combination of 's' (safe), 'q' (questionable), and 'e' (explicit).")]
		public Task<RuntimeResult> HentaiSearch([Remainder] string arguments = "") {
			return danbooru.CreateSearchAsync(Context, true, arguments);
			//return DanbooruSearchAsync(true, DanbooruRating.NotSafe, arguments);
		}

		private async Task<RuntimeResult> DanbooruSearchAsync(bool nsfw, DanbooruRating defaultRating, string arguments) {
			IUserMessage searchMessage = null;
			DanbooruPost post = null;
			try {
				/*if (arguments.Contains('&') || arguments.Contains('?') || arguments.Contains('/')) {
					return EmoteResults.FromInvalidArgument("Invalid characters in arguments");
				}*/
				bool tagsSpecified = false;
				int? retries = null;
				int? minScore = null;
				DanbooruRating? rating = null;
				List<string> tags = new List<string>();
				//List<string> extraTags = null;
				string[] args = arguments.Split(new char[0]); // This splits by whitespace
				foreach (string arg in args) {
					string[] parts = arg.Split(new char[] { '=' });
					if (parts.Length == 1 || parts[0].ToLower() == "tags") {
						if (tagsSpecified) {
							return EmoteResults.FromInvalidArgument("Main tags already specified");
						}
						tagsSpecified = true;
						if (!string.IsNullOrEmpty(parts.Last())) {
							tags.AddRange(parts[parts.Length - 1].Split(','));
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
							if (!nsfw) {
								return EmoteResults.FromInvalidArgument("Ratings cannot be specified in a sfw channel");
							}
							else if (!parts[1].Any()) {
								return EmoteResults.FromInvalidArgument("No ratings specified");
							}
							if (rating.HasValue) {
								return EmoteResults.FromInvalidArgument("Rating already specified");
							}
							DanbooruRating newRating = DanbooruRating.None;
							foreach (char c in parts[1]) {
								switch (c) {
								case 's': newRating |= DanbooruRating.Safe; break;
								case 'q': newRating |= DanbooruRating.Questionable; break;
								case 'e': newRating |= DanbooruRating.Explicit; break;
								default:
									return EmoteResults.FromInvalidArgument($"Unknown rating character `{c}`");
								}
							}
							rating = newRating;
							break;
						/*case "extra":
						case "extras":
						case "extratags":
							if (extraTags != null) {
								return EmoteResults.FromInvalidArgument("Extra tags already specified");
							}
							extraTags = new List<string>();
							extraTags.AddRange(parts[1].Split(','));
							break;*/
						case "retries":
							if (retries.HasValue) {
								return EmoteResults.FromInvalidArgument("Retries already specified");
							}
							if (int.TryParse(parts[1], out int result) && result >= 1) {
								retries = result;
							}
							else {
								return EmoteResults.FromInvalidArgument("Invalid retries");
							}
							break;
						case "minscore":
							if (minScore.HasValue) {
								return EmoteResults.FromInvalidArgument("Min Score already specified");
							}
							if (int.TryParse(parts[1], out int result2)) {
								minScore = result2;
							}
							else {
								return EmoteResults.FromInvalidArgument("Invalid min score");
							}
							break;
						default:
							return EmoteResults.FromInvalidArgument($"Unknown paramtere `{parts[0]}`");
						}
					}
				}
				try {
					//extraTags = extraTags ?? new List<string>();
					rating = rating ?? defaultRating;
					searchMessage = await ReplyAsync("🔎 Searching for a random danbooru image...").ConfigureAwait(false);
					//post = danbooru.Search(rating.Value, tags.ToArray(), minScore ?? 5, retries ?? 2);

				} catch (InvalidTagsException ex) {
					if (searchMessage != null)
						await searchMessage.DeleteAsync().ConfigureAwait(false);
					await ReplyAsync(ex.Message).ConfigureAwait(false);
					return NormalResult.FromSuccess();
				} catch (IllegalNsfwTagsException ex) {
					if (searchMessage != null)
						await searchMessage.DeleteAsync().ConfigureAwait(false);
					await ReplyAsync(ex.Message).ConfigureAwait(false);
					return NormalResult.FromSuccess();
				}
			} catch (Exception) {
				if (searchMessage != null)
					await searchMessage.DeleteAsync().ConfigureAwait(false);
				await ReplyAsync("An error occurred while searching. The input may have been invalid.").ConfigureAwait(false);
				return NormalResult.FromSuccess();
			}
			var embed = danbooru.MakeBaseEmbed(null);
			if (post == null) {
				embed.WithDescription("No matching results found");
			}
			else {
				if (string.IsNullOrWhiteSpace(arguments))
					arguments = "*none*";
				else
					arguments = $"`{Format.Sanitize(arguments)}`";
				embed.WithUrl(post.PostUrl);
				embed.WithDescription(//$"**Post:** <{post.PostUrl}>\n" +
					$"Arguments: {arguments}\n" +
					$"Score: **{post.Score}**{(nsfw ? $" | Rating: **{post.Rating}**" : "")}\n" +
					$"{post.LargeFileUrl}");
				embed.WithImageUrl(post.FileUrl);
			}
			IUserMessage message = await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
			//if (post != null)
			//	await danbooru.AddReactions(message).ConfigureAwait(false);
			//await searchMessage.DeleteAsync().ConfigureAwait(false);
			return NormalResult.FromSuccess();
		}
	}
}
