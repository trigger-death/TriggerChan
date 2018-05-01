﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MALClient.Models.Enums;
using MALClient.Models.Models.Forums;
using MALClient.XShared.Utils;

namespace MALClient.XShared.Comm.Forums
{
    public class ForumBoardTopicsQuery : Query
    {
        private static readonly Dictionary<ForumBoards, Dictionary<int, ForumBoardContent>> _boardCache =
            new Dictionary<ForumBoards, Dictionary<int, ForumBoardContent>>();

        private static readonly Dictionary<int, Dictionary<int, ForumBoardContent>> _animeBoardCache =
            new Dictionary<int, Dictionary<int, ForumBoardContent>>();

        private static readonly Dictionary<string, Dictionary<int, ForumBoardContent>> _clubBoardCache =
            new Dictionary<string, Dictionary<int, ForumBoardContent>>();

        private ForumBoards _board;
        private int _animeId;
        private readonly string _clubId;
        private int _page;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="page">From 0</param>
        public ForumBoardTopicsQuery(ForumBoards board,int page)
        {
            Request =
                WebRequest.Create(Uri.EscapeUriString($"https://myanimelist.net/forum/{GetEndpoint(board)}&show={page*50}"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
            _board = board;
            _page = page;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animeId"></param>
        /// <param name="page">From 0</param>
        public ForumBoardTopicsQuery(int animeId,int page,bool anime)
        {
            Request =
                WebRequest.Create(Uri.EscapeUriString($"https://myanimelist.net/forum/?{(anime ? "anime" : "manga")}id={animeId}&show={page*50}"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
            _animeId = animeId;
            _page = page;
        }

        public ForumBoardTopicsQuery(string clubId,int page)
        {
            Request =
                WebRequest.Create(Uri.EscapeUriString($"https://myanimelist.net/forum/?clubid={clubId}&show={page*50}"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
            _clubId = clubId;
            _page = page;
        }

        

        public async Task<ForumBoardContent> GetTopicPosts(int? lastPage,bool force = false)
        {
            if(!force)
                try
                {
                    if (_clubId == null)
                    {
                        if (_animeId == 0)
                        {
                            if (_boardCache.ContainsKey(_board) && _boardCache[_board].ContainsKey(_page))
                                return _boardCache[_board][_page];
                        }
                        else
                        {
                            if (_animeBoardCache.ContainsKey(_animeId) && _animeBoardCache[_animeId].ContainsKey(_page))
                                return _animeBoardCache[_animeId][_page];
                        }
                    }
                    else
                    {
                        if (_clubBoardCache.ContainsKey(_clubId) && _clubBoardCache[_clubId].ContainsKey(_page))
                            return _clubBoardCache[_clubId][_page];
                    }

                }
                catch (Exception e)
                {
                    //
                }
            else //clear all pages
            {
                if (_clubId == null)
                {
                    if (_animeId == 0)
                    {
                        if (_boardCache.ContainsKey(_board))
                            _boardCache[_board] = new Dictionary<int, ForumBoardContent>();
                    }
                    else
                    {
                        if (_animeBoardCache.ContainsKey(_animeId))
                            _animeBoardCache[_animeId] = new Dictionary<int, ForumBoardContent>();
                    }
                }
                else
                {
                    if (_clubBoardCache.ContainsKey(_clubId))
                        _clubBoardCache[_clubId] = new Dictionary<int, ForumBoardContent>();
                }

            }



            var output = new ForumBoardContent();
            var raw = await GetRequestResponse();
            if (string.IsNullOrEmpty(raw))
                return new ForumBoardContent();
            var doc = new HtmlDocument();
            doc.LoadHtml(raw);

            try
            {
                try
                {
                    output.Pages = lastPage ??
                                   int.Parse(
                                       doc.FirstOfDescendantsWithClass("span", "di-ib")
                                           .Descendants("a")
                                           .Last()
                                           .Attributes["href"]
                                           .Value.Split('=').Last())/50;
                }
                catch (Exception)
                {
                    output.Pages = 0;
                }


                var topicContainer =
                doc.DocumentNode.Descendants("table")
                    .First(node => node.Attributes.Contains("id") && node.Attributes["id"].Value == "forumTopics");

                foreach (var topicRow in topicContainer.Descendants("tr").Skip(1)) //skip forum table header
                {
                    try
                    {
                        output.ForumTopicEntries.Add(ParseHtmlToTopic(topicRow));
                    }
                    catch (Exception)
                    {
                        //hatml
                    }

                }
            }
            catch (Exception)
            {
                //
            }

            if (_clubId == null)
            {
                if (_animeId == 0)
                {
                    if (!_boardCache.ContainsKey(_board))
                        _boardCache[_board] = new Dictionary<int, ForumBoardContent>();
                    if (!_boardCache[_board].ContainsKey(_page))
                        _boardCache[_board].Add(_page, output);
                    else
                        _boardCache[_board][_page] = output;
                }
                else
                {
                    if (!_animeBoardCache.ContainsKey(_animeId))
                        _animeBoardCache[_animeId] = new Dictionary<int, ForumBoardContent>();
                    if (!_animeBoardCache[_animeId].ContainsKey(_page))
                        _animeBoardCache[_animeId].Add(_page, output);
                    else
                        _animeBoardCache[_animeId][_page] = output;
                }
            }
            else
            {
                if (!_clubBoardCache.ContainsKey(_clubId))
                    _clubBoardCache[_clubId] = new Dictionary<int, ForumBoardContent>();
                if (!_clubBoardCache[_clubId].ContainsKey(_page))
                    _clubBoardCache[_clubId].Add(_page, output);
                else
                    _clubBoardCache[_clubId][_page] = output;
            }



            return output;
        }

        public static ForumTopicEntry ParseHtmlToTopic(HtmlNode topicRow,int tdOffset = 0)
        {
            var current = new ForumTopicEntry();
            var tds = topicRow.Descendants("td").ToList();

            current.Type = tds[1].ChildNodes[0].InnerText;

            var titleLinks = tds[1].Descendants("a").ToList();
            var titleLink = titleLinks[0].InnerText.Length == 0 || titleLinks[0].InnerText.Contains("»") ? titleLinks[1] : titleLinks[0];

            current.Title = WebUtility.HtmlDecode(titleLink.InnerText);
            var link = titleLink.Attributes["href"].Value;
            if (link.Contains("&goto="))
            {
                var pos = link.IndexOf("&goto=");
                link = link.Substring(0, pos);
            }

            current.Id = link.Split('=').Last();
            

            var spans = tds[1].Descendants("span").Where(node => !string.IsNullOrEmpty(node.InnerText)).ToList();
            current.Op = spans[0].InnerText;
            current.Created = spans[1].InnerText;

            current.Replies = tds[2+tdOffset].InnerText;

            current.LastPoster = tds[3+tdOffset].Descendants("a").First().InnerText;
            current.LastPostDate = tds[3+tdOffset].ChildNodes.Last().InnerText;

            return current;
        }

        private static string GetEndpoint(ForumBoards board)
        {
            if (board == ForumBoards.AnimeSeriesDisc || board == ForumBoards.MangaSeriesDisc)
                return $"?subboard={(int) board - 100}"; //100 is offset to differentiate from other boards
            return $"?board={(int) board}";
        }
    }
}
