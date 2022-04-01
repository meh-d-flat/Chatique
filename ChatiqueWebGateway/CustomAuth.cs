using System;
using System.IO;
using System.Net;
using System.Web;

namespace ChatiqueWebGateway
{
    class CustomAuth
    {
        public void Go()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(@"http://+:9090/sup/");
                listener.Start();

                while (true)
                {
                    var c = listener.GetContext();

                    if (!c.Request.TryGetRequestCookie("beenBefore"))
                    {
                        c.Response.Cookies.Add(new Cookie("beenBefore", "true"));
                        WriteResponse(c.Response, "*Login form*");
                    }

                    if (c.Request.TryGetRequestCookie("authenticated"))
                        WriteResponse(c.Response, "*Chatique page*");

                    else
                    {
                        Console.WriteLine("is not authenticated");
                        using (var reader = new StreamReader(c.Request.InputStream, c.Request.ContentEncoding))
                        {
                            var parsed = HttpUtility.ParseQueryString(reader.ReadToEnd());

                            if (parsed["username"] == null || parsed["username"] == String.Empty)
                                WriteResponse(c.Response, "*Login form*");

                            else
                            {
                                var username = new Cookie("name", parsed["username"]);
                                c.Response.Cookies.Add(new Cookie("authenticated", "true"));
                                c.Response.Cookies.Add(username);

                                c.Response.StatusCode = 303;
                                c.Response.RedirectLocation = c.Request.Url.ToString();
                                WriteResponse(c.Response, "Logging you in");
                            }
                        }
                    }
                }
            }
        }

        void WriteResponse(HttpListenerResponse response, string text)
        {
            using (var writer = new StreamWriter(response.OutputStream))
                writer.Write(text);
        }
    }
}
