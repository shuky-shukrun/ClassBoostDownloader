using System;
using System.Collections.Generic;
using System.Text;

namespace ClassBoostDownloader
{
    class VideoChunk
    {
        public Uri file { get; private set; }
        public VideoChunk(Uri file)
        {
            this.file = file;
        }
    }
}
