using System;
using System.IO;
using System.Net;

//netsh http add urlacl url=http://+:9090/ user=Everyone
//netsh http show urlacl
//netsh http delete urlacl http://+:9090/
namespace ChatiqueWebGateway
{
    class BasicAuth : IAuth
    {
        public void Go()
        {
            while (true)
            {
                using (HttpListener listener = new HttpListener())
                {
                    listener.Prefixes.Add(@"http://+:9090/sup/");
                    listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                    listener.Start();

                    var c = listener.GetContext();
                    var user = c.User.Identity;

                    if (c.Request.TryGetRequestCookie("auth"))
                        Console.WriteLine("cookie: {0}", c.Request.Cookies["auth"].Value);

                    if (String.IsNullOrEmpty(user.Name) || String.IsNullOrWhiteSpace(user.Name))
                    {
                        c.Response.StatusCode = 401;
                        c.Response.AppendCookie(new Cookie("auth", "pending"));
                        using (var writer = new StreamWriter(c.Response.OutputStream))
                            writer.Write("Login failure.\nIf you see this then refresh the page to try again.");
                    }
                    else
                    {
                        c.Response.AppendCookie(new Cookie("auth", "passed"));
                        c.Response.Cookies.Add(new Cookie("name", user.Name));
                        c.Request.Headers["Authorization"] = "";
                        using (var writer = new StreamWriter(c.Response.OutputStream))
                            writer.Write("You're logged in, welcome!");
                    }
                }
            }
        }
    }
}
