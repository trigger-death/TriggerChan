using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles.Readers {
	/// <summary>
	/// The merchandise database user profile reader for the MyFigureCollection service.
	/// </summary>
	public class MyFigureCollectionProfileReader : IMerchProfileReader {

		#region Constants
		
		private const string ProfileBaseUrl = @"https://myfigurecollection.net/profile/";

		private const string UsernamePattern = @"^(?:(?:https?\:\/\/)?(?:www.)?myfigurecollection\.net\/profile\/)?((?:\-|\w)+)\/?$";
		private static readonly Regex UsernameRegex = new Regex(UsernamePattern, RegexOptions.IgnoreCase);

		private readonly Regex RootIdRegex = new Regex(@"\brootId=(\d)\b");
		private readonly Regex StatusRegex = new Regex(@"\bstatus=(\d)\b");
		private readonly Regex TabRegex = new Regex(@"\btab=(\d)\b");

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
		/// Queries the database user's profile from the username or Id.
		/// </summary>
		/// <param name="usernameOrId">The username or Id as a string.</param>
		/// <returns>The parsed user profile.</returns>
		public Task<IMerchProfile> QueryProfileAsync(string usernameOrId) {
			return QueryProfileInternalAsync(usernameOrId);
		}
		async Task<IDatabaseUserProfile> IDatabaseUserProfileReader.QueryProfileAsync(string usernameOrId) {
			return await QueryProfileAsync(usernameOrId).ConfigureAwait(false);
		}

		private async Task<IMerchProfile> QueryProfileInternalAsync(string username) {
			MerchProfile profile = new MerchProfile() {
				Username = username,
				Url = $"https://myfigurecollection.net/profile/{username}",
			};
			HtmlDocument doc;
			using (var client = new HttpClient()) {
				string raw = await (await client.GetAsync(profile.Url).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
				doc = new HtmlDocument();
				doc.LoadHtml(raw);
			}

			var content = doc.FirstOfDescendantsWithId("div", "content");
			var wide = content.FirstOfDescendantsWithId("div", "wide");
			var wrapper = wide.FirstOfDescendantsWithClass("div", "wrapper");
			
			ReadUserSection(profile, wrapper.ChildNodes[0]);

			foreach (var section in wrapper.ChildNodes.Skip(1)) {
				ReadMerchSection(profile, section);
			}

			return profile;
		}

		private void ReadUserSection(MerchProfile profile, HtmlNode section) {
			var h1UserRank = section.FirstOfDescendantsWithClassContaining("h1", "user-rank");
			if (h1UserRank != null)
				profile.Username = h1UserRank.InnerText;


			var imgAvatar = section.FirstOfDescendantsWithClass("img", "the-avatar");
			if (imgAvatar != null)
				profile.AvatarUrl = imgAvatar.Attributes["src"].Value;

			var spanStatus = section.FirstOfDescendantsWithClass("span", "tbx-target-STATUS");
			if (spanStatus != null) {
				profile.Status = WebUtility.HtmlDecode(spanStatus.InnerHtml);
				if (profile.Status.Length == 2)
					profile.Status = null;
				else
					profile.Status = profile.Status.Substring(1, profile.Status.Length - 2);
			}
		}

		private MerchList ReadListType(HtmlNode section) {
			var h2 = section.FirstOfDescendants("h2");
			if (h2 == null) return null;
			var aCount = h2.FirstOrDefaultOfDescendantsWithClass("a", "count");
			if (aCount == null) return null;
			var match = RootIdRegex.Match(aCount.Attributes["href"].Value);
			if (match.Success && match.Groups.Count >= 2) {
				int listId = int.Parse(match.Groups[1].Value);
				switch (listId) {
				case 0: return new FigureList();
				case 1: return new GoodsList();
				case 2: return new AllMediaList();
				}
			}
			return null;
		}
		private ListStatus? ReadListStatus(HtmlNode section, out int count) {
			count = 0;
			var a = section.FirstOrDefaultOfDescendantsWithClass("a", "selected") ??
					section.FirstOrDefaultOfDescendantsWithClass("a", "tbx-click");
			if (a == null) return null;
			var spanCount = a.FirstOrDefaultOfDescendantsWithClass("span", "count");
			if (spanCount == null) return null;

			count = int.Parse(spanCount.InnerText);
			Match match;
			if (a.HasClass("selected")) {
				match = StatusRegex.Match(a.Attributes["href"].Value);
			}
			else {
				var metaVars = a.FirstOrDefaultOfDescendantsWithAttribute("meta", "name", "vars");
				if (metaVars == null) return null;
				match = TabRegex.Match(metaVars.Attributes["content"].Value);
			}
			if (match.Success && match.Groups.Count >= 2) {
				int listId = int.Parse(match.Groups[1].Value);
				switch (listId) {
				case 2: return ListStatus.Owned;
				case 1: return ListStatus.Ordered;
				case 0: return ListStatus.Wished;
				case 3: return ListStatus.Favorites;
				}
			}
			return null;
		}

		private void ReadMerchSection(MerchProfile profile, HtmlNode section) {
			MerchList list = ReadListType(section);
			if (list == null)
				return;
			var subtab = section.FirstOfDescendantsWithClass("ul", "subtab");
			foreach (var il in subtab.ChildNodes) {
				ListStatus? status = ReadListStatus(il, out int count);
				if (status.HasValue) {
					switch (status) {
					case ListStatus.Owned: list.Owned = count; break;
					case ListStatus.Ordered: list.Ordered = count; break;
					case ListStatus.Wished: list.Wished = count; break;
					case ListStatus.Favorites: list.Favorites = count; break;
					}
				}
			}
			switch (list.ListType) {
			case ListType.Figures: profile.Figures = (FigureList) list; break;
			case ListType.Goods: profile.Goods = (GoodsList) list; break;
			case ListType.Media: profile.Media = (AllMediaList) list; break;
			}
		}
	}

	internal static class MFCExtensions {

		public static HtmlNode FirstWithClass(this IEnumerable<HtmlNode> doc, string targettedClass) {
			return
				doc.First(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass); //trim because mal like to leave stary spaces from time to time
		}
		
		public static HtmlNode FirstOfDescendantsWithClass(this HtmlDocument doc, string descendants,
			string targettedClass) {
			return
				doc.DocumentNode.Descendants(descendants)
					.First(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static HtmlNode FirstOrDefaultOfDescendantsWithClass(this HtmlDocument doc, string descendants,
			string targettedClass) {
			return
				doc.DocumentNode.Descendants(descendants)
					.FirstOrDefault(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static HtmlNode FirstOfDescendantsWithClassContaining(this HtmlDocument doc, string descendants,
			string targettedClass) {
			return
				doc.DocumentNode.Descendants(descendants)
					.First(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Contains(targettedClass));
		}

		public static HtmlNode FirstOfDescendantsWithId(this HtmlDocument doc, string descendants, string targettedId) {
			return
				doc.DocumentNode.Descendants(descendants)
					.First(node => node.Attributes.Contains("id") && node.Attributes["id"].Value.Trim() == targettedId);
		}

		public static HtmlNode FirstOfDescendantsWithId(this HtmlNode doc, string descendants, string targettedId) {
			return
				doc.Descendants(descendants)
					.First(node => node.Attributes.Contains("id") && node.Attributes["id"].Value.Trim() == targettedId);
		}

		public static HtmlNode FirstOfDescendantsWithClass(this HtmlNode doc, string descendants, string targettedClass) {
			return
				doc.Descendants(descendants)
					.First(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static HtmlNode FirstOfDescendantsWithClassContaining(this HtmlNode doc, string descendants,
			string targettedClass) {
			return
				doc.Descendants(descendants)
					.First(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Contains(targettedClass));
		}
		public static HtmlNode FirstOfDescendantsWithAttribute(this HtmlNode doc, string descendants, string attribute, string targettedClass) {
			return
				doc.Descendants(descendants)
					.First(node => node.Attributes.Contains(attribute) && node.Attributes[attribute].Value.Trim() == targettedClass);
		}
		public static HtmlNode FirstOrDefaultOfDescendantsWithAttribute(this HtmlNode doc, string descendants, string attribute, string targettedClass) {
			return
				doc.Descendants(descendants)
					.FirstOrDefault(node => node.Attributes.Contains(attribute) && node.Attributes[attribute].Value.Trim() == targettedClass);
		}

		public static HtmlNode FirstOfDescendants(this HtmlNode doc, string descendants) {
			return
				doc.Descendants(descendants).First();
		}
		public static HtmlNode FirstOrDefaultOfDescendants(this HtmlNode doc, string descendants) {
			return
				doc.Descendants(descendants).FirstOrDefault();
		}

		public static HtmlNode FirstOrDefaultOfDescendantsWithClass(this HtmlNode doc, string descendants, string targettedClass) {
			return
				doc.Descendants(descendants)
					.FirstOrDefault(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static IEnumerable<HtmlNode> WhereOfDescendantsWithClass(this HtmlDocument doc, string descendants,
			string targettedClass) {
			return
				doc.DocumentNode.Descendants(descendants)
					.Where(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static IEnumerable<HtmlNode> WhereOfDescendantsWithPartialClass(this HtmlDocument doc, string descendants,
			string targettedClass) {
			return
				doc.DocumentNode.Descendants(descendants)
					.Where(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Contains(targettedClass));
		}

		public static IEnumerable<HtmlNode> WhereOfDescendantsWithPartialId(this HtmlNode doc, string descendants,
			string targettedId) {
			return
				doc.Descendants(descendants)
					.Where(node => node.Attributes.Contains("id") && node.Attributes["id"].Value.Contains(targettedId));
		}

		public static IEnumerable<HtmlNode> WhereOfDescendantsWithClass(this HtmlNode doc, string descendants,
			string targettedClass) {
			return
				doc.Descendants(descendants)
					.Where(node => node.Attributes.Contains("class") && node.Attributes["class"].Value.Trim() == targettedClass);
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
			foreach (T element in source)
				action(element);
		}

		public static void IndexedForEach<T>(this IEnumerable<T> source, Action<T> action) {
			foreach (T element in source)
				action(element);
		}

		public static int FindIndex<T>(this IEnumerable<T> source, T obj) {
			int index = 0;
			foreach (T element in source) {
				if (element.Equals(obj))
					return index;
				index++;
			}
			return -1;
		}

		public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> matchPredicate) {
			int index = 0;
			foreach (T element in source) {
				if (matchPredicate.Invoke(element))
					return index;
				index++;
			}
			return -1;
		}

		public static string Wrap(this string s, string start, string end) {
			return $"{start}{s}{end}";
		}

		public static string TrimWhitespaceInside(this string str, bool allWhitespce = true) {
			return Regex.Replace(str, (allWhitespce ? @"\s" : " ") + @"{2,}", " ");
		}
	}
}
