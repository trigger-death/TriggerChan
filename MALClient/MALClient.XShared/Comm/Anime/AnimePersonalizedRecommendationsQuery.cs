using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Models.AnimeScrapped;
//using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Anime {
	public class AnimePersonalizedRecommendationsQuery : Query {
		private readonly bool _anime;

		private static string _animePlacement;
		private static string _mangaPlacement;

		public AnimePersonalizedRecommendationsQuery(bool anime) {
			_anime = anime;
		}

		private async Task InitPlacements() {
			HtmlDocument doc;
			using (var client = new HttpClient()) {
				var raw = await (await client.GetAsync("")).Content.ReadAsStringAsync();
				doc = new HtmlDocument();
				doc.LoadHtml(raw);
			}
			try {
				_animePlacement =
					doc.FirstOfDescendantsWithId("div", "v-auto-recommendation-personalized_anime").Attributes[
						"data-placement"].Value;
			}
			catch (Exception) {
				//not present
			}

			try {
				_mangaPlacement =
					doc.FirstOfDescendantsWithId("div", "v-auto-recommendation-personalized_manga").Attributes[
						"data-placement"].Value;
			}
			catch (Exception) {
				//not present
			}

			if (_animePlacement == null && _mangaPlacement == null) {
				throw new Exception();
			}
		}

		public async Task<List<AnimePersonalizedRecommendationData>> GetPersonalizedRecommendations(bool force = false) {
			List<AnimePersonalizedRecommendationData> possibleData = null;
			if (possibleData?.Any() ?? false)
				return possibleData;

			if (_animePlacement == null && _mangaPlacement == null) {
				try {
					await InitPlacements();
				}
				catch (Exception) {
					return new List<AnimePersonalizedRecommendationData>();
				}
			}

			List<AnimePersonalizedRecommendationData> data;
			using (var client = new HttpClient()) {
				var raw =
					await (await client.GetAsync(
							$"/auto_recommendation/personalized_suggestions.json?placement={(_anime ? _animePlacement : _mangaPlacement)}"))
						.Content.ReadAsStringAsync();
				data = JsonConvert.DeserializeObject<List<AnimePersonalizedRecommendationData>>(raw) ?? new List<AnimePersonalizedRecommendationData>();
			}
			//DataCache.SaveData(data, "personalized_suggestions", _anime ? "Anime" : "Manga");

			return data;
		}
	}
}
