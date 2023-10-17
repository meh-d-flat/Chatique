using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace ChatiqueWebGateway
{
    public static class Extensions
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

        public static async Task WriteResponse(HttpListenerResponse response, string text)
        {
            using (var writer = new StreamWriter(response.OutputStream))
                await writer.WriteAsync(text);
        }

        public static async Task HtmlResponse(HttpListenerResponse response, string htmlPath)
        {
            var html = File.ReadAllText(htmlPath);
            using (var writer = new StreamWriter(response.OutputStream))
                await writer.WriteAsync(html);
        }
    }
}
