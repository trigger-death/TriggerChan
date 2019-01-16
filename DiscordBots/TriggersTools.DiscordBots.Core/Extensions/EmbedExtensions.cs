using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Discord;

namespace TriggersTools.DiscordBots.Extensions {
	public static class EmbedExtensions {
		
		public static EmbedBuilder PaginateField(this EmbedBuilder embed, string name, IEnumerable<string> items, bool inline = false) {
			StringBuilder str = new StringBuilder();
			int pageIndex = 1;
			void AddField(bool forcePage) {
				string page = string.Empty;
				if (pageIndex != 1 || forcePage)
					page = $" `(pg. {pageIndex++})`";
				embed.AddField($"{name}{page}", str.ToString(), inline);
			}
			foreach (string item in items) {
				if (item.Length > 1024)
					throw new InvalidOperationException("Item length is too large for embed field!");
				if (str.Length + item.Length > 1024) {
					AddField(true);
					str.Clear();
				}
				if (str.Length > 0)
					str.AppendLine();
				str.Append(item);
			}
			if (str.Length > 0)
				AddField(false);
			return embed;
		}

		//private const string VariablePattern = @"http://triggerenum\.triggerenum/(?'var'.*)$";
		//private static readonly Regex VariableRegex = new Regex(VariablePattern);
		private const string VariablePrefix = @"http://info.discordapp.data/";
		private const string UserIdPrefix = @"http://user.discordapp.id/";

		private static string WithVariable(string value) {
			// Some arbitrary url that will never exist
			return string.Concat(VariablePrefix, HttpUtility.UrlEncode(value));
		}
		public static EmbedBuilder WithImageVariable(this EmbedBuilder embed, string value) {
			embed.ImageUrl = WithVariable(value);
			return embed;
		}
		public static EmbedBuilder WithThumbnailVariable(this EmbedBuilder embed, string value) {
			embed.ThumbnailUrl = WithVariable(value);
			return embed;
		}
		public static EmbedBuilder WithAuthorId(this EmbedBuilder embed, ulong id) {
			if (embed.Author == null)
				throw new InvalidOperationException("No author to add url variable to!");
			embed.Author = embed.Author.SetAuthorId(id);
			return embed;
		}
		public static EmbedAuthorBuilder SetAuthorId(this EmbedAuthorBuilder embedAuthor, ulong id) {
			return new EmbedAuthorBuilder {
				Name = embedAuthor.Name,
				Url = string.Concat(UserIdPrefix, id.ToString()),
				IconUrl = embedAuthor.IconUrl,
			};
		}
		private static string GetVariable(string url) {
			if (url.StartsWith(VariablePrefix))
				return HttpUtility.UrlDecode(url.Substring(VariablePrefix.Length));
			return null;
		}
		public static string GetImageVariable(this IEmbed embed) {
			if (embed.Image.HasValue)
				return GetVariable(embed.Image.Value.Url);
			return null;
		}
		public static string GetThumbnailVariable(this IEmbed embed) {
			if (embed.Thumbnail.HasValue)
				return GetVariable(embed.Thumbnail.Value.Url);
			return null;
		}
		public static ulong GetAuthorId(this IEmbed embed) {
			if (embed.Author.HasValue) {
				string url = embed.Author.Value.Url;
				if (url.StartsWith(UserIdPrefix) &&
					ulong.TryParse(url.Substring(UserIdPrefix.Length), out ulong id))
				{
					return id;
				}
			}
			return 0;
		}
	}
}
