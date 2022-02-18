using System;
using System.Linq;
using System.Collections.Generic;
using WebSocketSharp;

namespace ChatiqueClient
{
    class Program
    {
        static string name;
        static void Main(string[] args)
        {
            Console.Write("state your name: ");
            name = Console.ReadLine();

            using (var ws = args.Length > 0
                ? new WebSocket(String.Format("ws://{0}:{1}/", args[0], args[1]))
                : new WebSocket("ws://localhost:8087/"))
            {
                ws.OnMessage += (sender, e) =>
                    Console.WriteLine(e.Data);
                ws.SetCookie(new WebSocketSharp.Net.Cookie("name", name));
                ws.Connect();
                while (true)
                {
                    var message = Console.ReadLine();
                    if (Console.CursorTop != 0)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    ws.Send(message);
                }
            }
        }
    }
}
