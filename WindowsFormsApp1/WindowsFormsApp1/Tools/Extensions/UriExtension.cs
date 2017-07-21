using System;
using System.Net;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1.Tools.Extensions
{
    static class UriExtension
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        public static bool SetCookie(this Uri uri, string cookieData)
        {
            Cookie ck = new Cookie("dosid", cookieData, uri.AbsolutePath, "/");
            return InternetSetCookie(uri.ToString(), ck.Name, ck.Value);
        }
    }
}
