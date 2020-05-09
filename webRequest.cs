using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace ClassBoostDownloader
{
    class webRequest
    {
        private CookieContainer cookiesContainer;
        static public readonly string REQUEST_USERAGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.100 Safari/537.36";
        static private readonly string REQUEST_ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

        public webRequest()
        {
            cookiesContainer = new CookieContainer();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls;
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public string getRequest(string url, string parms)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + parms);
                request.CookieContainer = cookiesContainer;
                request.Accept = REQUEST_ACCEPT;
                request.UserAgent = REQUEST_USERAGENT;
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                response.Close();
                cookiesContainer = request.CookieContainer;
                return responseFromServer;
            }catch(WebException we)
            {
                Console.WriteLine("Failed to create get request");
                Console.WriteLine("Error: " + we.ToString());
                System.Threading.Thread.Sleep(1000 * 10);
                return getRequest(url, parms);
            }

        }
        public string postRequest(string url, string parms, string data)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + parms);
                request.CookieContainer = cookiesContainer;
                request.Method = "POST";
                request.Accept = REQUEST_ACCEPT;
                request.UserAgent = REQUEST_USERAGENT;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                request.ContentLength = bytes.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(bytes, 0, bytes.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                cookiesContainer = request.CookieContainer;
                return responseFromServer;
            }
            catch (WebException we)
            {
                Console.WriteLine("Failed to create post request");
                Console.WriteLine("Error: " + we.ToString());
                System.Threading.Thread.Sleep(1000 * 10);
                return postRequest(url, parms, data);
            }
    }

        public string postRequest(string url, string parms, Dictionary<string, string> data)
        {
            string sdata = String.Empty;
            foreach (var dk in data)
                sdata += dk.Key + "=" + Uri.EscapeDataString(dk.Value) + "&";
            sdata = sdata.Substring(0, sdata.Length - 1);

            return postRequest(url, parms, sdata);
        }

        public bool checkCookies(string url, string name)
        {
            CookieCollection cc = cookiesContainer.GetCookies(new Uri(url));
            if (cc == null)
                return false;
            for(int i = 0; i < cc.Count; i++)
            {
                if (cc[i].Name.Equals(name))
                    return !String.IsNullOrEmpty(cc[i].Value);
            }
            return false;
        }
    }
}
