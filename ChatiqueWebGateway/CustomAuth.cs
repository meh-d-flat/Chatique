using System;
using System.IO;
using System.Net;
using System.Web;

namespace ChatiqueWebGateway
{
    class CustomAuth : IAuth
    {
        public void Go()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(@"http://+:9090/sup/");
                listener.Start();

                while (true)
                {
                    var context = listener.GetContext();

                    if (!context.Request.TryGetRequestCookie("beenBefore"))
                    {
                        context.Response.Cookies.Add(new Cookie("beenBefore", "true"));
                        Extensions.HtmlResponse(context.Response, "Login.html");
                    }

                    if (context.Request.TryGetRequestCookie("authenticated"))
                        Extensions.HtmlResponse(context.Response, "Index.html");

                    else
                        ProcessForm(context);
                }
            }
        }

        void ProcessForm(HttpListenerContext ctx)
        {
            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                var parsed = HttpUtility.ParseQueryString(reader.ReadToEnd());

                if (parsed["username"] == null || parsed["username"] == String.Empty)
                    Extensions.HtmlResponse(ctx.Response, "Login.html");

                else
                {
                    if (Chatique.Vault.CheckCredentialHashing(parsed["username"], parsed["password"]))
                    {
                        var username = new Cookie("name", parsed["username"]);
                        ctx.Response.Cookies.Add(new Cookie("authenticated", "true"));
                        ctx.Response.Cookies.Add(username);
                        ctx.Response.StatusCode = 303;
                        ctx.Response.RedirectLocation = ctx.Request.Url.ToString();
                        Extensions.WriteResponse(ctx.Response, "Logging you in");
                    }

                    Extensions.HtmlResponse(ctx.Response, "Login.html");
                }
            }
        }
    }
}
