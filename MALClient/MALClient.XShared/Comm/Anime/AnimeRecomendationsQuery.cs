﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Models.AnimeScrapped;

namespace MALClient.XShared.Comm.Anime
{
    public class AnimeRecomendationsQuery : Query
    {
        private readonly bool _anime;

        public AnimeRecomendationsQuery(bool anime = true)
        {
            _anime = anime;
            Request =
                WebRequest.Create(Uri.EscapeUriString($"https://myanimelist.net/recommendations.php?s=recentrecs&t={(anime ? "anime" : "manga")}"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
        }

        public async Task<List<RecommendationData>> GetRecomendationsData()
        {
            var output = new List<RecommendationData>();
            var raw = await GetRequestResponse();
            if (string.IsNullOrEmpty(raw))
                return null;
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);
            var recomNodes =
                doc.DocumentNode.Descendants("div")
                    .Where(
                        node =>
                            node.Attributes.Contains("class") &&
                            node.Attributes["class"].Value ==
                            "spaceit borderClass").Take(30);
                //constant 20 recommendations

            foreach (var recomNode in recomNodes)
            {
                try
                {
                    var desc =
                        recomNode.ChildNodes.First(
                            node =>
                                node.Name == "div" &&
                                node.Attributes["class"].Value ==
                                "spaceit recommendations-user-recs-text");
                    if (desc != null)
                    {
                        var titleNodes =
                            recomNode.Descendants("a").Where(node => node.Attributes.Count == 2).Take(2).ToArray();
                        var titles = titleNodes.Select(node => WebUtility.HtmlDecode(node.InnerText.Trim())).ToArray();
                        var ids =
                            titleNodes.Select(
                                node => TryGetIdFromThisFreakingInconsistentHtmlTag(node.Attributes["href"].Value))
                                .ToArray();

                        output.Add(new RecommendationData
                        {
                            IsAnime = _anime,
                            DependentId = ids[0],
                            RecommendationId = ids[1],
                            DependentTitle = titles[0],
                            RecommendationTitle = titles[1],
                            Description = WebUtility.HtmlDecode(desc.InnerText).Trim()
                        });


                        int TryGetIdFromThisFreakingInconsistentHtmlTag(string value)
                        {
                            var tokens = value.Split('/');
                            if (tokens.Length == 6)
                                return Convert.ToInt32(tokens[4]);

                            return Convert.ToInt32(tokens[2]);
                        }
                    }
                }
                catch (Exception e)
                {
                    //
                }
            }

            return output;
        }
    }
}