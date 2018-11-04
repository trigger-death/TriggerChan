﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MALClient.Adapters
{
    public interface IImageDownloaderService
    {
        void DownloadImage(string url, string suggestedFilename,bool animeConver);
        void DownloadImageDefault(string url, string suggestedFilename ,bool animeCover);
    }
}
