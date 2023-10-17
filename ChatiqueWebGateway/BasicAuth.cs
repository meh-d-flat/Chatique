using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

//netsh http add urlacl url=http://+:9090/ user=Everyone
//netsh http show urlacl
//netsh http delete urlacl http://+:9090/
namespace ChatiqueWebGateway
{
    class BasicAuth : IAuth
    {
        public async Task GoAsync()
        {
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(@"http://+:9090/sup/");
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                listener.Start();

                while (true)
                {
                    var c = await listener.GetContextAsync();
                    var user = (HttpListenerBasicIdentity)c.User.Identity;

                    if (String.IsNullOrEmpty(user.Name) || String.IsNullOrWhiteSpace(user.Name))
                        await Unauthorized(c.Response);

                    else
                    {
                        if (Chatique.Vault.CheckCredentialHashing(user.Name, user.Password))
                        {
                            c.Response.Cookies.Add(new Cookie("name", user.Name));
                            c.Response.Cookies.Add(new Cookie("authenticated", "true"));
                            await Extensions.HtmlResponse(c.Response, "Index.html");
                        }

                        await Unauthorized(c.Response);
                    }
                }
            }

        }

        public async Task Unauthorized(HttpListenerResponse response)
        {
            response.StatusCode = 401;
            await Extensions.WriteResponse(response, "Login failure.\nIf you see this then refresh the page to try again.");
        }

        public void Go()
        {
            throw new NotImplementedException();
        }
    }
}
