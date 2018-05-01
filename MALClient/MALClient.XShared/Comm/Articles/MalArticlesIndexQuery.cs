﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Enums;
using MALClient.Models.Models.MalSpecific;
using MALClient.XShared.Utils;

namespace MALClient.XShared.Comm.Articles
{
    public class MalArticlesIndexQuery : Query
    {
        private ArticlePageWorkMode _mode;

        private static Dictionary<ArticlePageWorkMode, List<MalNewsUnitModel>> _cachedData =
            new Dictionary<ArticlePageWorkMode, List<MalNewsUnitModel>>();

        public MalArticlesIndexQuery(ArticlePageWorkMode mode)
        {
            _mode = mode;
            Request =
                WebRequest.Create(
                    Uri.EscapeUriString(mode == ArticlePageWorkMode.Articles
                        ? "https://myanimelist.net/featured"
                        : "https://myanimelist.net/news"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
        }

        public async Task<List<MalNewsUnitModel>> GetArticlesIndex(bool force = false)
        {
            if (!force)
            {
                if (_cachedData.ContainsKey(_mode))
                    return _cachedData[_mode];
                var possibleData = await DataCache.RetrieveArticleIndexData(_mode);
                if (possibleData != null && possibleData.Count > 0)
                    return possibleData;
            }
            var output = new List<MalNewsUnitModel>();
            var raw = await GetRequestResponse();
            if (string.IsNullOrEmpty(raw))
                return null;
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);
            switch (_mode)
            {
                case ArticlePageWorkMode.Articles:
                    foreach (var featuredNewsUnit in doc.WhereOfDescendantsWithClass("div", "featured-pickup-unit"))
                    {
                        try
                        {
                            var current = new MalNewsUnitModel();
                            var img = featuredNewsUnit.Descendants("a").First();
                            var imgUrl = img.Attributes["data-bg"].Value;
                            if (!imgUrl.Contains("questionmark"))
                            {
                                imgUrl = Regex.Replace(imgUrl, @"\/r\/\d+x\d+", "");
                                current.ImgUrl = imgUrl.Substring(0, imgUrl.IndexOf('?'));
                            }
                            current.Url = img.Attributes["href"].Value;
                            current.Highlight = WebUtility.HtmlDecode(featuredNewsUnit.Descendants("p").First().InnerText.Trim());
                            current.Title = WebUtility.HtmlDecode(img.InnerText.Trim());
                            if(current.Title.ToLower().Contains("giveaway")) // emm I'll pass on these
                                continue;
                            var infoDiv = featuredNewsUnit.FirstOfDescendantsWithClass("div", "information");
                            var infoDivsParagraphs = infoDiv.Descendants("p").ToList();
                            current.Author = WebUtility.HtmlDecode(infoDivsParagraphs[0].InnerText.Trim());
                            current.Views = WebUtility.HtmlDecode(infoDivsParagraphs[1].InnerText.Trim());
                            current.Tags = "New";

                            output.Add(current);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    foreach (var newsUnit in doc.WhereOfDescendantsWithClass("div", "news-unit clearfix"))
                    {
                        try
                        {
                            var current = new MalNewsUnitModel();
                            var img = newsUnit.Descendants("a").First();
                            current.Url = img.Attributes["href"].Value;
                            try
                            {
                                current.ImgUrl = img.Descendants("img").First().Attributes["data-src"].Value;
                            }
                            catch (Exception)
                            {
                                //html here is messy, there may be change here soon
                            }
                            var contentDivs = newsUnit.Descendants("div").ToList();
                            current.Title =
                                WebUtility.HtmlDecode(contentDivs[0].Descendants("p").First().InnerText.Trim());
                            current.Highlight = WebUtility.HtmlDecode(contentDivs[1].InnerText.Trim());
                            var infos = contentDivs[2].Descendants("p").ToList();
                            current.Author = infos[0].InnerText.Trim();
                            current.Views = infos[1].InnerText.Trim();
                            try
                            {
                                current.Tags = string.Join(", ",
                                    contentDivs[3].Descendants("a").Select(node => node.InnerText.Trim()));
                            }
                            catch (Exception)
                            {
                                //no tags
                            }
                            current.Type = MalNewsType.Article;
                            output.Add(current);
                        }
                        catch (Exception)
                        {
                            //hatml
                        }
                    }

                    break;
                case ArticlePageWorkMode.News:
                    foreach (var newsUnit in doc.WhereOfDescendantsWithClass("div", "news-unit clearfix rect"))
                    {
                        try
                        {
                            var current = new MalNewsUnitModel();
                            var img = newsUnit.Descendants("a").First();
                            current.Url = img.Attributes["href"].Value;
                            try
                            {
                                current.ImgUrl = img.Descendants("img").First().Attributes["srcset"].Value.Split(' ', ' ')[2];
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    current.ImgUrl = img.Descendants("img").First().Attributes["data-src"].Value;
                                }
                                catch (Exception)
                                {
                                    //it may work this way, my predicition
                                }
                                //html here is messy, there may be change here soon
                            }
                            var contentDivs = newsUnit.Descendants("div").ToList();
                            current.Title = WebUtility.HtmlDecode(contentDivs[0].Descendants("p").First().InnerText.Trim());
                            current.Highlight = WebUtility.HtmlDecode(contentDivs[1].InnerText.Trim());
                            var infos = contentDivs[2].Descendants("p").ToList();
                            current.Author = "By: " + infos[0].ChildNodes[1].InnerText.Trim();
                            current.Views = infos[0].ChildNodes[3].InnerText.Trim();
                            try
                            {
                                current.Tags = string.Join(", ", contentDivs[2].ChildNodes[3].Descendants("a").Select(node => node.InnerText.Trim()));
                            }
                            catch (Exception)
                            {
                                //no tags
                            }
                            current.Type = MalNewsType.News;
                            output.Add(current);
                        }
                        catch (Exception)
                        {
                            //hatml
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _cachedData[_mode] = output;
            DataCache.SaveArticleIndexData(_mode, output);
            return output;
        }
    }
}