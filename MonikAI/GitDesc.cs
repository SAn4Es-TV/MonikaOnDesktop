using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SolicenTEAM
{
    public static class GitDesc
    {
        public static string gitURL = "";
        public static string resultString = "";
        public static void GetGitDesc()
        {
            WebClient web = new WebClient();
            web.Proxy = new WebProxy();
            resultString = web.DownloadString(gitURL);
        }
    }
}
