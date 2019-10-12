using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace ClassBoostDownloader
{
    class ClassBoost
    {
        public static readonly string CLASSBOOST_URL = "https://www.classboost.co.il/";
        private webRequest webReq;

        public ClassBoost()
        {
            webReq = new webRequest();
        }
        public bool login(string id, string password)
        {
            Console.WriteLine("Get parameters from login page...");
            string reHtml = webReq.getRequest(CLASSBOOST_URL, "Pages/Login/Login.aspx");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("__EVENTTARGET", String.Empty);
            data.Add("__EVENTARGUMENT", String.Empty);
            data.Add("__VIEWSTATE", String.Empty);
            data.Add("__VIEWSTATEGENERATOR", String.Empty);
            data.Add("__EVENTVALIDATION", String.Empty);
            data.Add("hdnRedirect", "regularApp");
            data.Add("hdnInitParams", "True");
            data.Add("hdnHideChangPwd", "False");
            data.Add("btnLogin", "התחבר");

            string[] keyArray = new string[data.Keys.Count];
            data.Keys.CopyTo(keyArray, 0);
            foreach (var dp in keyArray)
            {
                string dataVal = reHtml.Substring(reHtml.IndexOf(string.Format("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"", dp)) + 42 + 2 * dp.Length);
                dataVal = dataVal.Substring(0, dataVal.IndexOf("\""));
                data[dp] = dataVal;
            }
            data.Add("txtEmail", id);
            data.Add("txtPassword", password);

            Console.WriteLine("Login to " + id + " with password " + password);
            webReq.postRequest(CLASSBOOST_URL, "Pages/Login/Login.aspx?logout=1", data);

            return webReq.checkCookies(CLASSBOOST_URL, "ASP.NET_SessionId") && webReq.checkCookies(CLASSBOOST_URL, "PBLOGIN_SUMMARY");
        }

        public PlaylistM3U8 getMeeting(uint meetingId)
        {
            Console.WriteLine("Loading meeting id " + meetingId + " ...");
            string reHtml = webReq.getRequest(CLASSBOOST_URL, "/Pages/VideoPage.aspx?MeetingID=" + meetingId);

            Console.WriteLine("Searching meeting data...");
            try
            {
                string playlist = reHtml.Substring(reHtml.IndexOf("InitScreenOriginalSrc =  new Array('") + 36);
                playlist = playlist.Substring(0, playlist.IndexOf("');"));

                string initname = reHtml.Substring(reHtml.IndexOf("InitName =  new Array('") + 23);
                initname = initname.Substring(0, initname.IndexOf("');"));

                string rePlaylist = webReq.getRequest(playlist,string.Empty);

                char[] charArray = initname.ToCharArray();
                Array.Reverse(charArray);
                Console.Write(@"Meeting id: " + meetingId + @"
Meeting name: ");
                Program.hebrewPrint(initname);
                Console.WriteLine();

                return new PlaylistM3U8(meetingId, initname, new Uri(playlist), rePlaylist);
            }
            catch (Exception)
            {
                throw new Exception("Cannot reading data from meeting id");
            }
        }

        public List<VideoChunk> loadPlaylist(PlaylistM3U8 pl)
        {
            Console.WriteLine("Loading meeting id " + pl.meetingID + " video chunks ...");
            string reCL = webReq.getRequest(pl.chunklist.AbsoluteUri, String.Empty);

            uint i = 0;
            List<VideoChunk> lvc = new List<VideoChunk>();
            Uri localPath = pl.getLocalUri();
            RefreshingLine rl = new RefreshingLine("Creates chunks list...");
            try
            {
                string line;
                StringReader reader = new StringReader(reCL);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("media"))
                    {
                        rl.WriteRefreshLine(string.Format("Add chunk number {0}", i));
                        lvc.Add(new VideoChunk(new Uri(localPath.AbsoluteUri + Uri.UnescapeDataString(line))));
                        i++;
                    }
                };
                rl.NewWriteLine(string.Format("Finsh - added {0} chunks", i));
                return lvc;
            }
            catch (Exception)
            {
                throw new Exception("Cannot create chunks list");
            }
        }

        public List<VideoChunk> downloadMeeting(PlaylistM3U8 pl, List<VideoChunk> lvc)
        {
            string dirName = Path.GetTempPath() + "\\ClassBoostDownloader_" + DateTime.Now.ToFileTime() + "_" + pl.meetingID;
            List<VideoChunk> reLvc = new List<VideoChunk>();
            if (Directory.CreateDirectory(dirName).Exists)
            {
                try
                {
                    uint i = 0;
                    RefreshingLine rl = new RefreshingLine(string.Format("Starting download {0} chunkds", lvc.Count));
                    foreach (VideoChunk vc in lvc)
                    {
                        using (var client = new WebClient())
                        {
                            string chunkFile = Path.GetFileName(vc.file.AbsoluteUri).Replace(vc.file.Query, String.Empty);
                            rl.WriteRefreshLine(string.Format("Download chunk {0} / {1} / {2:0.00}%", i, chunkFile, ((i++ + 1) / (float)lvc.Count) * 100));
                            chunkFile = dirName + "\\" + chunkFile;
                            client.DownloadFile(vc.file, chunkFile);
                            reLvc.Add(new VideoChunk(new Uri(chunkFile)));
                        }
                    }
                    rl.NewWriteLine("Download complete");
                    return reLvc;
                }
                catch (Exception)
                {
                    throw new Exception("Cannot download chunk");
                }
            }
            else
            {
                throw new Exception("Cannot create download directory");
            }           
        }
        public void mergeChunks(PlaylistM3U8 pl, List<VideoChunk> lvc)
        {
            try
            {
                uint i = 0;
                RefreshingLine rl = new RefreshingLine(string.Format("Start merging {0} chunkds", lvc.Count));
                using (var outputStream = File.Create("mid_" + pl.meetingID + ".ts"))
                {
                    foreach (VideoChunk vc in lvc)
                    {
                        string chunkFile = Path.GetFileName(vc.file.AbsolutePath);
                        rl.WriteRefreshLine(string.Format("Merging chunk {0} / {1} / {2:0.00}%", i, chunkFile, ((i++ + 1) / (float)lvc.Count) * 100));
                        using (var inputStream = File.OpenRead(vc.file.LocalPath))
                        {
                            int readCount;
                            byte[] buffer = new byte[1024];
                            while ((readCount = inputStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                outputStream.Write(buffer, 0, readCount);
                            }
                            inputStream.Close();
                            File.Delete(vc.file.LocalPath);
                        }
                    }
                    outputStream.Close();
                    rl.NewWriteLine("Merging is complete");
                    Process.Start("mid_" + pl.meetingID + ".ts");
                }
            }
            catch (Exception)
            {
                throw new Exception("Cannot merging chunks");
            }
        }
    }
}
