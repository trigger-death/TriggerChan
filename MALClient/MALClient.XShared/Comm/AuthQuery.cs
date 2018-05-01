﻿using System;
using System.Net;
using MALClient.Models.Enums;
using MALClient.XShared.Utils;

namespace MALClient.XShared.Comm
{
    public class AuthQuery : Query
    {
        public AuthQuery(ApiType forApi)
        {
            switch (forApi)
            {
                case ApiType.Mal:
                    Request =
                        WebRequest.Create(
                            Uri.EscapeUriString("https://myanimelist.net/api/account/verify_credentials.xml"));
                    Request.Credentials = Credentials.GetHttpCreditentials();
                    Request.Method = "GET";
                    break;
                case ApiType.Hummingbird:
                    Request =
                        WebRequest.Create(
                            Uri.EscapeUriString(
                                $"https://hummingbird.me/api/v1/users/authenticate?{Credentials.GetHummingbirdCredentialChain()}"));
                    Request.ContentType = "application/x-www-form-urlencoded";
                    Request.Method = "POST";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}