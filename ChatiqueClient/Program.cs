using System;
using System.Linq;
using System.Collections.Generic;
using WebSocketSharp;
using System.Text;

namespace ChatiqueClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

            string greeting = "what's your name: ";
            Console.Write(greeting);

            string name = Console.ReadLine();
            while (String.IsNullOrWhiteSpace(name) && String.IsNullOrEmpty(name))
            {
                Console.SetCursorPosition(greeting.Length, 0);
                name = Console.ReadLine();
            }
            Console.Clear();

            string location = args.Length > 0
                ? String.Format("ws://{0}:{1}/", args[0], args[1])
                : "ws://localhost:8087/";

            using (var ws = new WebSocket(location))
            {
                ws.OnMessage += (sender, e) =>
                    Console.WriteLine(e.Data);
                ws.SetCookie(new WebSocketSharp.Net.Cookie("name", name));
                ws.Connect();

                Console.WriteLine("welcome!");

                while (true)
                {
                    var message = Console.ReadLine();

                    if (Console.CursorTop != 0)
                        MoveCarriage();

                    ws.Send(message);
                }
            }
        }

        static void MoveCarriage()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}
