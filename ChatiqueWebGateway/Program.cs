using System;
using System.Net;

namespace ChatiqueWebGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(new BasicAuth());
        }

        static void Run(IAuth listener)
        {
            listener.Go();
        }
    }

    public static class Ext
    {
        public static bool TryGetRequestCookie(this HttpListenerRequest request, string cookieName)
        {
            bool exists = false;
            try
            {
                var cookie = request.Cookies[cookieName];
                exists = cookie != null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                exists = false;
            }
            return exists;
        }
    }
}