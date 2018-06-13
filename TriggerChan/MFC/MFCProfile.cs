using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace TriggersTools.DiscordBots.TriggerChan.MFC {

	public enum MFCCategoryType {
		Figures,
		Goods,
		Media,
	}

	public struct MFCCategory {
		public MFCCategoryType Type;
		public int Owned;
		public int Ordered;
		public int Wished;
		public int Favorites;

		public int OwnedAndOrdered {
			get { return Owned + Ordered; }
		}

		public bool Any {
			get { return Owned > 0 || Ordered > 0 || Wished > 0 || Favorites > 0; }
		}

		public override string ToString() {
			string str = "";
			if (Owned > 0) {
				str = $"Owned: {Owned}\n";
			}
			if (Ordered > 0) {
				str += $"Ordered: {Ordered}\n";
			}
			if (Wished > 0) {
				str += $"Wished: {Wished}\n";
			}
			if (Favorites > 0) {
				str += $"Favorites: {Favorites}\n";
			}
			if (string.IsNullOrEmpty(str)) {
				str = "*none*";
			}
			return str;
		}
	}

	public class MFCProfile {

		public string AvatarUrl;

		public string Username;
		public string Status;

		public MFCCategory Figures;
		public MFCCategory Goods;
		public MFCCategory Media;

		public IEnumerable<MFCCategory> Categories {
			get {
				//if (Figures.Any)
					yield return Figures;
				//if (Goods.Any)
					yield return Goods;
				//if (Media.Any)
					yield return Media;
			}
		}

		public bool Any {
			get { return Figures.Any || Goods.Any || Media.Any; }
		}

		public static async Task<MFCProfile> LoadProfile(string username) {
			MFCProfile current = new MFCProfile() {
				Username = username,
			};
			HtmlDocument doc;
			using (var client = new HttpClient()) {
				var raw = await (await client.GetAsync($"https://myfigurecollection.net/profile/{username}")).Content.ReadAsStringAsync();
				doc = new HtmlDocument();
				doc.LoadHtml(raw);
			}

			var content = doc.FirstOfDescendantsWithId("div", "content");
			var wide = content.FirstOfDescendantsWithId("div", "wide");
			var wrapper = wide.FirstOfDescendantsWithClass("div", "wrapper");

			#region User
			var userSection = wrapper.ChildNodes[0];

			var avatar = userSection.FirstOfDescendantsWithClass("img", "the-avatar");
			current.AvatarUrl = avatar.Attributes["src"].Value;

			var status = userSection.FirstOfDescendantsWithClass("span", "tbx-target-STATUS");
			current.Status = HttpUtility.HtmlDecode(status.InnerHtml);
			#endregion

			for (int i = 0; i < 3; i++) {
				try {
					var section = wrapper.ChildNodes[i + 1];
					var sectionTitle = section.FirstChild.ChildNodes[1];
					if (sectionTitle == null)
						break;
					if (sectionTitle.InnerHtml.StartsWith("Figures")) {
						current.Figures = ParseCategory(MFCCategoryType.Figures, section);
					}
					else if (sectionTitle.InnerHtml.StartsWith("Goods")) {
						current.Goods = ParseCategory(MFCCategoryType.Goods, section);
					}
					else if (sectionTitle.InnerHtml.StartsWith("Media")) {
						current.Media = ParseCategory(MFCCategoryType.Media, section);
					}
				}
				catch {
					break;
				}
			}

			var user = doc.FirstOfDescendantsWithClass("div", "user-object");
			
			return current;
		}

		public static MFCCategory ParseCategory(MFCCategoryType type, HtmlNode section) {
			MFCCategory cat = new MFCCategory();
			cat.Type = type;
			var subtab = section.FirstOfDescendantsWithClass("ul", "subtab");
			foreach (var il in subtab.ChildNodes) {
				var label = il.FirstChild;
				var countSpan = label.FirstOfDescendantsWithClass("span", "count");
				int count = int.Parse(countSpan.InnerHtml);
				if (label.InnerHtml.StartsWith("Owned")) {
					cat.Owned = count;
				}
				else if (label.InnerHtml.StartsWith("Ordered")) {
					cat.Ordered = count;
				}
				else if (label.InnerHtml.StartsWith("Wished")) {
					cat.Wished = count;
				}
				else if (label.InnerHtml.StartsWith("Favorites")) {
					cat.Favorites = count;
				}
			}
			return cat;
		}
	}

	public static class MFCExtensions {

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
			var index = 0;
			foreach (T element in source) {
				if (element.Equals(obj))
					return index;
				index++;
			}
			return -1;
		}

		public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> matchPredicate) {
			var index = 0;
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
