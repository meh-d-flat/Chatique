using System;
//using SuperSocket.SocketBase.Config;
//using SuperWebSocket;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Chatique
{
    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Sessions.Broadcast(e.Data);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(8087);

            wssv.AddWebSocketService<Laputa>("/");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
