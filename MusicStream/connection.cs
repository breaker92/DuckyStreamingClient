using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

namespace MusicStream
{
    public static class connection
    {
        public static Cookie loginCookie;

        public static string domain;//= "https://www.embeddedreality.de/music/";
        public static string user;//= "sebastian";
        public static string passwd;// = "Nahallo12";

        public static HttpWebRequest prepRequestforPost(string PostData)
        {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(domain);
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = "Bernd";
                request.Method = "POST";
                if (loginCookie != null)
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(new Uri(domain), loginCookie);
                }
                byte[] data = Encoding.ASCII.GetBytes(PostData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                
                return request;
        }

        public static HttpWebRequest prepRequestforGet(string PostData)
        {
            Uri sUri = new Uri(string.Concat(domain, "?", PostData));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUri);
            request.UserAgent = "Bernd";
            request.Method = "GET";
            if (loginCookie != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(new Uri(domain), loginCookie);
            }
            request.ContentType = "text/plain";
            request.ContentLength = 0;
            return request;
        }

        public static HttpWebRequest prepRequestforPost(string quary, string PostData)
        {
            Uri sUri = new Uri(string.Concat(domain, "?", quary));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUri);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.UserAgent = "Bernd";
            request.Method = "POST";
            if (loginCookie != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(new Uri(domain), loginCookie);
            }
            byte[] data = Encoding.ASCII.GetBytes(PostData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            return request;
        }

    }
}
