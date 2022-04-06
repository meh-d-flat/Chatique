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
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(@"http://+:9090/sup/");
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                listener.Start();

                while (true)
                {
                    var c = listener.GetContext();
                    var user = (HttpListenerBasicIdentity)c.User.Identity;

                    if (String.IsNullOrEmpty(user.Name) || String.IsNullOrWhiteSpace(user.Name))
                        Unauthorized(c.Response);

                    else
                    {
                        if (Chatique.Vault.CheckCredentialHashing(user.Name, user.Password))
                        {
                            c.Response.Cookies.Add(new Cookie("name", user.Name));
                            c.Response.Cookies.Add(new Cookie("authenticated", "true"));
                            Extensions.HtmlResponse(c.Response, "Index.html");
                        }

                        Unauthorized(c.Response);
                    }
                }
            }

        }

        public void Unauthorized(HttpListenerResponse response)
        {
            response.StatusCode = 401;
            Extensions.WriteResponse(response, "Login failure.\nIf you see this then refresh the page to try again.");
        }
    }
}
