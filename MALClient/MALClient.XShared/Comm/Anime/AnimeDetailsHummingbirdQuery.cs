﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MALClient.Models.Models.Anime;
using MALClient.XShared.Utils;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Anime
{
    public class AnimeDetailsHummingbirdQuery : Query
    {
        private static readonly string _passPhrase = "72618e585c0d89f4db49"; //not really all that secret
        public static Dictionary<int, int> MalToHumId = new Dictionary<int, int>();
        private readonly int _id;

        public AnimeDetailsHummingbirdQuery(int id)
        {
            Request =
                WebRequest.Create(
                    Uri.EscapeUriString($"https://hummingbird.me/api/v2/anime/myanimelist:{id}"));
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = "GET";
            _id = id;
        }

        public async Task<AnimeDetailsData> GetAnimeDetails(bool force = false)
        {
			AnimeDetailsData possibleData = null;
            if (possibleData != null)
                return possibleData;
            Request.Headers["X-Client-Id"] = _passPhrase;
            var raw = await GetRequestResponse();

            try
            {
                dynamic obj = JsonConvert.DeserializeObject(raw);

                var current = new AnimeHummingbirdDetailsData();

                foreach (var genre in obj.anime.genres)
                    current.Genres.Add(genre.Value);

                var eps = new List<Tuple<string, int>>();

                foreach (var episode in obj.linked.episodes)
                    eps.Add(new Tuple<string, int>(episode.title.Value, (int) episode.number.Value));

                eps = eps.OrderBy(tuple => tuple.Item2).ToList();
                current.Episodes.AddRange(eps.Select(tuple => tuple.Item1));
                current.SourceId = obj.anime.id;
                current.AlternateCoverImgUrl = obj.anime.poster_image;

                var output = current.ToAnimeDetailsData();

                //DataCache.SaveAnimeDetails(_id, output);
                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<int> GetHummingbirdId(bool force = false)
        {
            Request.Headers["X-Client-Id"] = _passPhrase;
            var raw = await GetRequestResponse();

            try
            {
                dynamic jsonObj = JsonConvert.DeserializeObject(raw);
                int val = int.Parse(jsonObj.anime.id.ToString());
                if (!MalToHumId.ContainsKey(_id))
                    MalToHumId.Add(_id, val);
                return val;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}