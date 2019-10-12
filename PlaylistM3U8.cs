using System;

namespace ClassBoostDownloader
{
    class PlaylistM3U8
    {
        public uint meetingID { get; private set; }
        public string name { get; private set; }
        public Uri playlist { get; private set; }
        public Uri chunklist { get; private set; }

        public PlaylistM3U8(uint meetingID, string name, Uri playlist, string data)
        {
            this.meetingID = meetingID;
            this.name = name;
            this.playlist = playlist;

            try
            {
                string chunkPlaylist = data.Substring(data.IndexOf("chunklist"));
                chunkPlaylist = Uri.UnescapeDataString(chunkPlaylist.Substring(0, chunkPlaylist.IndexOf("\n")));
                this.chunklist = new Uri(getLocalUri() + chunkPlaylist);
            }
            catch (Exception)
            {
                throw new Exception("Cannot read chunklist");
            }
            
        }  
        public Uri getLocalUri()
        {
            return new Uri(playlist.AbsoluteUri.Substring(0, playlist.AbsoluteUri.LastIndexOf("/") + 1));
        }
    }
}
