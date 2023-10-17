using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ChatiqueWebGateway
{
    class CustomAuth : IAuth
    {
        public async Task GoAsync()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(@"http://+:9090/sup/");
                listener.Start();

                while (true)
                {
                    var context = await listener.GetContextAsync();

                    if (context.Request.Cookies.Count == 0 & context.Request.HttpMethod == "GET")
                        await Extensions.HtmlResponse(context.Response, "Login.html");

                    else if (context.Request.TryGetRequestCookie("signup"))
                    {
                        context.Request.Cookies["signup"].Expires = DateTime.Now.AddDays(-5);
                        context.Response.Cookies.Add(new Cookie("signing-in", "true"));
                        await Extensions.HtmlResponse(context.Response, "Signup.html");
                    }

                    else if (context.Request.TryGetRequestCookie("signing-in") & context.Request.HttpMethod == "POST")
                        await ProcessSignUpForm(context);

                    else if (context.Request.TryGetRequestCookie("authenticated"))
                        await Extensions.HtmlResponse(context.Response, "Index.html");

                    else if (context.Request.HttpMethod == "POST")
                        await ProcessLoginForm(context);
                }
            }
        }

        async Task ProcessSignUpForm(HttpListenerContext ctx)
        {
            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                var parsed = HttpUtility.ParseQueryString(reader.ReadToEnd());

                ctx.Response.Cookies.Add(new Cookie("signing-in", "true"));

                if (parsed.Count == 0)
                    await Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (parsed["username"] == null || parsed["username"] == String.Empty)
                    await Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (parsed["password"] == null || parsed["password"] == String.Empty)
                    await Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (parsed["password"] != parsed["repeat-password"])
                    await Extensions.HtmlResponse(ctx.Response, "Signup.html");

                if (Chatique.Vault.CreateCredential2(parsed["username"], parsed["password"]))
                {
                    Console.WriteLine("done signing up");
                    ctx.Response.Cookies.Add(new Cookie("authenticated", "true"));
                    ctx.Response.Cookies.Add(new Cookie("name", parsed["username"]));
                    ctx.Response.StatusCode = 303;
                    ctx.Response.RedirectLocation = ctx.Request.Url.ToString();
                    await Extensions.WriteResponse(ctx.Response, "Logging you in");
                }
                else
                    await Extensions.WriteResponse(ctx.Response, "Something went wrong!");
            }
        }

        async Task ProcessLoginForm(HttpListenerContext ctx)
        {
            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                var parsed = HttpUtility.ParseQueryString(reader.ReadToEnd());

                if (parsed["username"] == null || parsed["username"] == String.Empty)
                   await Extensions.HtmlResponse(ctx.Response, "Login.html");

                else
                {
                    if (Chatique.Vault.CheckCredentialHashing(parsed["username"], parsed["password"]))
                    {
                        ctx.Response.Cookies.Add(new Cookie("authenticated", "true"));
                        ctx.Response.Cookies.Add(new Cookie("name", parsed["username"]));
                        ctx.Response.StatusCode = 303;
                        ctx.Response.RedirectLocation = ctx.Request.Url.ToString();
                        await Extensions.WriteResponse(ctx.Response, "Logging you in");
                    }

                    await Extensions.HtmlResponse(ctx.Response, "Login.html");
                }
            }
        }

        public void Go()
        {
            throw new NotImplementedException();
        }
    }
}
