﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using MALClient.Models.Enums;
using MALClient.Models.Models.Anime;
using MALClient.XShared.Comm.Manga;
using MALClient.XShared.Utils;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Anime
{
    public class AnimeGeneralDetailsQuery : Query
    {
        public async Task<AnimeGeneralDetailsData> GetAnimeDetails(bool force, string id, string title, bool animeMode,
            ApiType? apiOverride = null)
        {
			AnimeGeneralDetailsData output = null;

            var requestedApiType = apiOverride ?? CurrentApiType;
            try
            {
                switch (requestedApiType)
                {
                    case ApiType.Mal:
                        string data = null;
                        if (!string.IsNullOrEmpty(title))
                        {
                            data = animeMode
                            ? await new AnimeSearchQuery(Utils.Utilities.CleanAnimeTitle(title), requestedApiType)
                                .GetRequestResponse()
                            : await new MangaSearchQuery(Utils.Utilities.CleanAnimeTitle(title)).GetRequestResponse();
                        }
                  
                        if (string.IsNullOrEmpty(data) || !data.Contains(id))
                        {
                            //we are loading title from website because request came from mal url
                            var correctTitle = await AnimeTitleQuery.GetTitle(int.Parse(id), animeMode);
                            data = animeMode
                            ? await new AnimeSearchQuery(Utils.Utilities.CleanAnimeTitle(correctTitle), requestedApiType)
                                .GetRequestResponse()
                            : await new MangaSearchQuery(Utils.Utilities.CleanAnimeTitle(correctTitle)).GetRequestResponse();
                        }

                        data = WebUtility.HtmlDecode(data);

                        data = data.Replace("&", ""); //unparsable stuff ahaead :(
                        var parsedData = XDocument.Parse(data);

                        var elements = parsedData.Element(animeMode ? "anime" : "manga").Elements("entry");
                        var xmlObj = elements.First(element => element.Element("id").Value == id);

                        output = new AnimeGeneralDetailsData();
                        output.ParseXElement(xmlObj, animeMode, Settings.PreferEnglishTitles);

                        //DataCache.SaveAnimeSearchResultsData(id, output, animeMode);
                        break;
                    case ApiType.Hummingbird:
                        Request =
                            WebRequest.Create(
                                Uri.EscapeUriString($"https://hummingbird.me/api/v1/anime/{id}"));
                        Request.ContentType = "application/x-www-form-urlencoded";
                        Request.Method = "GET";

                        var raw = await GetRequestResponse();
                        if (string.IsNullOrEmpty(raw))
                            break;

                        dynamic jsonObj = JsonConvert.DeserializeObject(raw);
                        var allEps = 0;
                        if (jsonObj.episode_count != null)
                            allEps = Convert.ToInt32(jsonObj.episode_count.ToString());
                        output = new AnimeGeneralDetailsData
                        {
                            Title = jsonObj.title.ToString(),
                            ImgUrl = jsonObj.cover_image.ToString(),
                            Type = jsonObj.show_type.ToString(),
                            Id = Convert.ToInt32(jsonObj.id.ToString()),
                            MalId = Convert.ToInt32(jsonObj.mal_id.ToString()),
                            AllEpisodes = allEps,
                            StartDate = jsonObj.started_airing.ToString(),
                            EndDate = jsonObj.finished_airing.ToString(),
                            Status = jsonObj.status,
                            Synopsis = jsonObj.synopsis,
                            GlobalScore = jsonObj.community_rating,
                            Synonyms = new List<string> {jsonObj.alternate_title.ToString()}
                        };
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                // todo android notification nav bug
                // probably MAl garbled response
            }
            
            return output;
        }

        
    }
}