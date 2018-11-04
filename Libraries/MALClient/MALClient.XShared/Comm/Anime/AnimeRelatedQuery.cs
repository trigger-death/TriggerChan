﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Enums;
using MALClient.Models.Models.AnimeScrapped;
using MALClient.XShared.Utils;

namespace MALClient.XShared.Comm.Anime {
	public class AnimeRelatedQuery : Query {
		private readonly int _animeId;
		private readonly bool _animeMode;

		public AnimeRelatedQuery(int id, bool anime = true) {
			Request =
				WebRequest.Create(Uri.EscapeUriString($"https://myanimelist.net/{(anime ? "anime" : "manga")}/{id}/"));
			Request.ContentType = "application/x-www-form-urlencoded";
			Request.Method = "GET";
			_animeId = id;
			_animeMode = anime;
		}

		public async Task<List<RelatedAnimeData>> GetRelatedAnime(bool force = false) {
			var output = /*force
                ? new List<RelatedAnimeData>()
                : await DataCache.RetrieveRelatedAnimeData(_animeId, _animeMode) ??*/ new List<RelatedAnimeData>();
			if (output.Count != 0)
				return output;

			var raw = await GetRequestResponse();
			if (string.IsNullOrEmpty(raw))
				return null;

			var doc = new HtmlDocument();
			doc.LoadHtml(raw);
			try {
				var relationsNode = doc.DocumentNode.Descendants("table")
					.First(
						node =>
							node.Attributes.Contains("class") &&
							node.Attributes["class"].Value ==
							"anime_detail_related_anime");


				try {
					var tds = relationsNode.Descendants("tr").First().Descendants("td").ToList();
					for (int i = 0; i < tds.Count; i += 2) {
						var relation = WebUtility.HtmlDecode(tds[i].InnerText.Trim());
						foreach (var linkNode in tds[i + 1].Descendants("a")) {
							var current = new RelatedAnimeData();
							current.WholeRelation = relation;
							var link = linkNode.Attributes["href"].Value.Split('/');
							current.Type = link[1] == "anime"
								? RelatedItemType.Anime
								: link[1] == "manga" ? RelatedItemType.Manga : RelatedItemType.Unknown;
							current.Id = Convert.ToInt32(link[2]);
							current.Title = WebUtility.HtmlDecode(linkNode.InnerText.Trim());
							output.Add(current);
						}
					}


				}
				catch (Exception) {
					//mystery
				}

			}
			catch (Exception) {
				//no recom
			}
			//DataCache.SaveRelatedAnimeData(_animeId, output, _animeMode);

			return output;
		}
	}
}