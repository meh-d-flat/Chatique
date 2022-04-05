using System;
using WebSocketSharp.Server;

namespace Chatique
{
    class Program
    {
        static void Main(string[] args)
        {
            Vault.Configure();

            WebSocketServer wssv = args.Length > 0
                ? new WebSocketServer(Convert.ToInt32(args[0]))
                : new WebSocketServer(8087);
            wssv.AddWebSocketService<Chat>("/");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
