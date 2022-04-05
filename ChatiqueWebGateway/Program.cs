using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ChatiqueWebGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(new CustomAuth());
        }

        static void Run(IAuth listener)
        {
            Console.WriteLine("Gateway's listening on: {0}", Ext.MyIP());
            listener.Go();
        }
    }

    public static class Ext
    {
        public static IPAddress MyIP()
        {
            IPAddress machineIP = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    machineIP = ip;
            }
            return machineIP;
        }

        public static bool TryGetRequestCookie(this HttpListenerRequest request, string cookieName)
        {
            bool exists = false;
            try
            {
                var cookie = request.Cookies[cookieName];
                exists = cookie != null;
            }
            catch
            {
                exists = false;
            }
            return exists;
        }

        public static bool TryGetRequestHeader(this HttpListenerRequest request, string headerName)
        {
            bool exists = false;
            try
            {
                var cookie = request.Headers[headerName];
                exists = true;
            }
            catch
            {
                exists = false;
            }
            return exists;
        }

        public static void WriteResponse(HttpListenerResponse response, string text)
        {
            using (var writer = new StreamWriter(response.OutputStream))
                writer.Write(text);
        }

        public static void HtmlResponse(HttpListenerResponse response, string htmlPath)
        {
            var html = File.ReadAllText(htmlPath);
            using (var writer = new StreamWriter(response.OutputStream))
                writer.Write(html);
        }
    }
}