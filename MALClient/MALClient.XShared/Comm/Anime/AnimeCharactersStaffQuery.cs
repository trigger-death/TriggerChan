﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Models.AnimeScrapped;
using MALClient.Models.Models.Favourites;
using MALClient.XShared.Utils;

namespace MALClient.XShared.Comm.Anime {
	public class AnimeStaffData {
		public List<AnimeCharacterStaffModel> AnimeCharacterPairs { get; set; } = new List<AnimeCharacterStaffModel>();
		public List<AnimeStaffPerson> AnimeStaff { get; set; } = new List<AnimeStaffPerson>();
	}

	public class AnimeCharactersStaffQuery : Query {
		private readonly int _animeId;
		private readonly bool _animeMode;

		public AnimeCharactersStaffQuery(int id, bool anime = true) {
			Request =
				WebRequest.Create(
					Uri.EscapeUriString(
						$"https://myanimelist.net/{(anime ? "anime" : "manga")}/{id}/whatever/characters"));
			Request.ContentType = "application/x-www-form-urlencoded";
			Request.Method = "GET";
			_animeId = id;
			_animeMode = anime;
		}

		public async Task<List<AnimeCharacter>> GetMangaCharacters(bool force) //html is malformed
		{
			if (_animeMode)
				throw new InvalidOperationException("You fed constructor with anime, remember?");

			var output = /*force
				? new List<AnimeCharacter>()
				: await DataCache.RetrieveData<List<AnimeCharacter>>($"staff_{_animeId}", "MangaDetails", 7) ??*/
				  new List<AnimeCharacter>();

			var raw = await GetRequestResponse();
			if (string.IsNullOrEmpty(raw))
				return null;

			var doc = new HtmlDocument();
			doc.LoadHtml(raw);
			try {
				var mainContainer = doc.FirstOfDescendantsWithClass("div", "js-scrollfix-bottom-rel");
				var tables = mainContainer.ChildNodes.Where(node => node.Name == "table").ToList();

				foreach (var table in tables) {
					try {
						var current = new AnimeCharacter();

						var imgs = table.Descendants("img").ToList();
						var infos = table.Descendants("td").ToList();

						//character
						var img = imgs[0].Attributes["data-src"].Value;
						if (!img.Contains("questionmark")) {
							img = img.Replace("/r/46x64", "");
							current.ImgUrl = img.Substring(0, img.IndexOf('?'));
						}

						current.FromAnime = _animeMode;
						current.ShowId = _animeId.ToString();
						current.Name = WebUtility.HtmlDecode(imgs[0].Attributes["alt"].Value.Replace(",", ""));
						current.Id = infos[0].ChildNodes[1].ChildNodes[0].Attributes["href"].Value.Split('/')[2];
						current.Notes = ""; //malformed html here TODO: Check if fixed 
											//table.Descendants("small").First().InnerText;
						output.Add(current);
					}
					catch (Exception) {
						//
					}

				}
			}
			catch (Exception) {
				//html strikes again
			}
			return output;
		}

		public async Task<AnimeStaffData> GetCharStaffData(bool force = false) {
			if (!_animeMode)
				throw new InvalidOperationException("Umm you said it's going to be manga...");
			var output = /*force
				? new AnimeStaffData()
				: await DataCache.RetrieveData<AnimeStaffData>($"staff_{_animeId}", "AnimeDetails", 7) ??*/
				  new AnimeStaffData();
			if (output.AnimeCharacterPairs.Count > 0 || output.AnimeStaff.Count > 0)
				return output;

			var raw = await GetRequestResponse();
			if (string.IsNullOrEmpty(raw))
				return null;

			var doc = new HtmlDocument();
			doc.LoadHtml(raw);

			try {
				var mainContainer = doc.FirstOfDescendantsWithClass("div", "js-scrollfix-bottom-rel");
				List<HtmlNode> charTables = new List<HtmlNode>();
				List<HtmlNode> staffTables = new List<HtmlNode>();
				bool nowStaff = false;
				int headerCount = 0;
				foreach (var node in mainContainer.ChildNodes) {
					if (!nowStaff) {
						if (node.Name == "table")
							charTables.Add(node);
						else if (node.Name == "h2") {
							headerCount++;
							if (headerCount == 2)
								nowStaff = true;
						}
					}
					else {
						if (node.Name == "table")
							staffTables.Add(node);
					}
				}
				int i = 0;
				foreach (var table in charTables) {
					try {
						var current = new AnimeCharacterStaffModel();

						var imgs = table.Descendants("img").ToList();
						var infos = table.Descendants("td").ToList(); //2nd is character 4th is person

						//character
						var img = imgs[0].Attributes["data-src"].Value;
						if (!img.Contains("questionmark")) {
							img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
							current.AnimeCharacter.ImgUrl = img.Substring(0, img.IndexOf('?'));
						}

						current.AnimeCharacter.FromAnime = _animeMode;
						current.AnimeCharacter.ShowId = _animeId.ToString();
						current.AnimeCharacter.Name =
							WebUtility.HtmlDecode(imgs[0].Attributes["alt"].Value.Replace(",", ""));
						current.AnimeCharacter.Id = infos[1].ChildNodes[1].Attributes["href"].Value.Split('/')[4];
						current.AnimeCharacter.Notes = infos[1].ChildNodes[3].InnerText.Trim();

						//voiceactor
						try {
							img = imgs[1].Attributes["data-src"].Value;
							if (!img.Contains("questionmark")) {
								img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
								current.AnimeStaffPerson.ImgUrl = img.Substring(0, img.IndexOf('?'));
							}
							current.AnimeStaffPerson.Name = WebUtility.HtmlDecode(imgs[1].Attributes["alt"].Value.Replace(",", ""));
							current.AnimeStaffPerson.Id = infos[3].ChildNodes[1].Attributes["href"].Value.Split('/')[4];
							current.AnimeStaffPerson.Notes = infos[3].ChildNodes[4].InnerText.Trim();
						}
						catch (Exception) {
							//no voice actor
							current.AnimeStaffPerson.Name = "Unknown";
							current.AnimeStaffPerson.IsUnknown = true;
						}


						output.AnimeCharacterPairs.Add(current);
						if (i++ > 30)
							break;
					}
					catch (Exception) {
						//oddities
					}

				}
				i = 0;
				foreach (var staffRow in staffTables) {
					try {
						var current = new AnimeStaffPerson();
						var imgs = staffRow.Descendants("img").ToList();
						var info = staffRow.Descendants("td").Last(); //we want last

						var img = imgs[0].Attributes["data-src"].Value;
						if (!img.Contains("questionmark")) {
							img = Regex.Replace(img, @"\/r\/\d+x\d+", "");
							current.ImgUrl = img.Substring(0, img.IndexOf('?'));
						}
						var link = info.Descendants("a").First();
						current.Name = WebUtility.HtmlDecode(link.InnerText.Trim().Replace(",", ""));
						current.Id = link.Attributes["href"].Value.Split('/')[4];
						current.Notes = staffRow.FirstOfDescendantsWithClass("div", "spaceit_pad").InnerText.Trim();

						output.AnimeStaff.Add(current);
						if (i++ > 30)
							break;
					}
					catch (Exception) {
						//what can I say?
					}
				}
			}
			catch (Exception) {
				//mysteries of html
			}


			//DataCache.SaveData(output, $"staff_{_animeId}", "AnimeDetails");

			return output;
		}

	}
}
