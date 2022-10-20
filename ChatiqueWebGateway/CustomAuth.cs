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

                    if (context.Request.TryGetRequestCookie("signup"))
                    {
                        context.Request.Cookies["signup"].Expires = DateTime.Now.AddSeconds(5);
                        context.Request.Cookies["signup"].Expired = true;
                        //context.Response.Cookies["signup"].Expired = true;
                        context.Response.Cookies.Add(new Cookie("signing-in", "true"));
                        Extensions.HtmlResponse(context.Response, "Signup.html");
                        continue;
                    }

                    if (context.Request.TryGetRequestCookie("authenticated"))
                        Extensions.HtmlResponse(context.Response, "Index.html");

                    else if (context.Request.TryGetRequestCookie("signing-in"))
                        ProcessSignUpForm(context);

                    else
                        ProcessLoginForm(context);
                }
            }
        }

        void ProcessSignUpForm(HttpListenerContext ctx)
        {
            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                var parsed = HttpUtility.ParseQueryString(reader.ReadToEnd());

                ctx.Response.Cookies.Add(new Cookie("signing-in", "true"));

                if (parsed.Count == 0)
                {
                    Extensions.HtmlResponse(ctx.Response, "Signup.html");
                    //return;
                }

                if (parsed["username"] == null || parsed["username"] == String.Empty)
                    Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (parsed["password"] == null || parsed["password"] == String.Empty)
                    Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (parsed["password"] != parsed["repeat-password"])
                    Extensions.HtmlResponse(ctx.Response, "Signup.html");

                //if//else
                //{
                    if (Chatique.Vault.CreateCredential2(parsed["username"], parsed["password"]))
                    {
                        Console.WriteLine("done signing up");
                    var username = new Cookie("name", parsed["username"]);
                    ctx.Response.Cookies.Add(new Cookie("authenticated", "true"));
                    ctx.Response.Cookies.Add(username);
                    ctx.Response.StatusCode = 303;
                    ctx.Response.RedirectLocation = ctx.Request.Url.ToString();
                    Extensions.WriteResponse(ctx.Response, "Logging you in");
                    //var username = new Cookie("name", parsed["username"]);
                    //ctx.Response.Cookies.Add(new Cookie("authenticated", "true"));
                    //ctx.Response.Cookies.Add(username);
                    //ctx.Response.StatusCode = 303;
                    //ctx.Response.RedirectLocation = ctx.Request.Url.ToString();
                    //Extensions.WriteResponse(ctx.Response, "Logging you in");
                }
                    else
                        Extensions.WriteResponse(ctx.Response, "Something went wrong!");
                    //Extensions.HtmlResponse(ctx.Response, "Login.html");
                //}
            }
        }

        void ProcessLoginForm(HttpListenerContext ctx)
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
