using System;
using WebSocketSharp;

namespace ChatiqueClient
{
    class Program
    {
        static string name;
        static void Main(string[] args)
        {
            Console.WriteLine("state your name");
            name = Console.ReadLine();

            using (var ws = new WebSocket(args.Length != 0 ? "ws://"+ args[0] +":8087/" : "ws://localhost:8087/"))
            {
                ws.OnMessage += (sender, e) =>
                    Console.WriteLine(e.Data);

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
                    ws.Send(String.Format("[{0}]: {1}", name, message));
                }
                Console.ReadKey(true);
            }
        }
    }
}
