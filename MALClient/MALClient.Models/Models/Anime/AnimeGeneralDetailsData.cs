﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MALClient.Models.Models.Anime
{
    public class AnimeGeneralDetailsData
    {
        private int _id = -1;

        public int Id
        {
            get { return _id == -1 ? MalId : _id; }
            set { _id = value; }
        }

        public int MalId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Synopsis { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ImgUrl { get; set; }
        public int AllEpisodes { get; set; }
        public int AllVolumes { get; set; }
        public float GlobalScore { get; set; }
        public string AlternateTitle { get; set; }
        public List<string> Synonyms { get; set; } = new List<string>();

        public void ParseXElement(XElement xmlObj, bool anime,bool preferEnglishTitle)
        {
            float score;
            if (!float.TryParse(xmlObj.Element("score").Value,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture, out score))
                score = 0;
            MalId = Convert.ToInt32(xmlObj.Element("id").Value);
            var title = "";
            string alternateTitle = null;
            if (preferEnglishTitle)
            {
                var elem = xmlObj.Element("english");
                if (!string.IsNullOrWhiteSpace(elem?.Value))
                {
                    title = elem.Value;
                    alternateTitle = xmlObj.Element("title").Value;
                }
                else
                {
                    title = xmlObj.Element("title").Value;
                }
            }
            else
            {
                var elem = xmlObj.Element("english");
                if (!string.IsNullOrWhiteSpace(elem?.Value))
                {
                    alternateTitle = elem.Value;
                }
                title = xmlObj.Element("title").Value;
            }
            Title = title;
            AlternateTitle = alternateTitle;

            GlobalScore = score;
            Type = xmlObj.Element("type").Value;
            Status = xmlObj.Element("status").Value;
            Synopsis = xmlObj.Element("synopsis").Value;
            //Synopsis cleaning
            Synopsis =
                new StringBuilder(Synopsis).Replace("quot;", "\"")
                    .Replace("ndash;", "-")
                    .Replace("rdquo;", "\"")
                    .Replace("ldquo;", "\"")
                    .Replace("rsquo;", "'")
                    .Replace("#039;", "'")
                    .Replace("radic;", "√")
                    .Replace("szlig;", "ß")
                    .Replace("auml;", "ä")
                    .Replace("uuml;", "ü")
                    .Replace("ouml;", "ö")
                    .Replace("euml;", "ë")
                    .Replace("amp;", "")
                    .Replace("mdash;", "-")
                    .Replace("iuml;", "ï")
                    .Replace("<br />", "").ToString();
            Synopsis = Regex.Replace(Synopsis, @"\[[^\]]*\]", string.Empty).Trim(); //get rid of bbcode


            StartDate = xmlObj.Element("start_date").Value;
            EndDate = xmlObj.Element("end_date").Value;
            ImgUrl = xmlObj.Element("image").Value;
            Synonyms = xmlObj.Element("synonyms").Value.Split(',').ToList();
            AllEpisodes = Convert.ToInt32(xmlObj.Element(anime ? "episodes" : "chapters").Value);
            AllVolumes = !anime ? Convert.ToInt32(xmlObj.Element("volumes").Value) : 0;
        }
    }
}