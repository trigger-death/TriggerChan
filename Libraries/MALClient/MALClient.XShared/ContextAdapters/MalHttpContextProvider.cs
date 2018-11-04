﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MALClient.XShared.BL;
using MALClient.XShared.Comm.MagicalRawQueries;

namespace MALClient.UWP.Adapters
{
    public class MalHttpContextProvider : MalHttpContextProviderBase
    {
        protected override async Task<HttpClient> ObtainContext()
        {
            var httpHandler = new HttpClientHandler()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AllowAutoRedirect = false,
            };
            _httpClient = new CsrfHttpClient(httpHandler) { BaseAddress = new Uri(MalBaseUrl) };
            await _httpClient.GetToken();

            var response = await _httpClient.PostAsync("/login.php", LoginPostBody);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.RedirectMethod)
            {
                _contextExpirationTime = DateTime.Now.Add(TimeSpan.FromHours(.5));
                return _httpClient; //else we are returning client that can be used for next queries
            }

            throw new WebException("Unable to authorize");
        }
    }
}
