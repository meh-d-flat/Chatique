using System;
using System.Linq;
using System.Collections.Generic;
using WebSocketSharp;
using System.Text;
using System.Security.Cryptography;

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
            string password = Console.ReadLine();
            while (String.IsNullOrWhiteSpace(password) && String.IsNullOrEmpty(password))
            {
                Console.SetCursorPosition(greeting.Length, 0);
                password = Console.ReadLine();

                using (SHA512 sha = SHA512Managed.Create())
                {
                    password = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
                    password = password.Replace("-", "");
                    password = password.ToLower();
                }
            }
            Console.Clear();

            string location = args.Length > 0
                ? String.Format("ws://{0}:{1}/", args[0], args[1])
                : "ws://localhost:8087/";

            using (var webSocket = new WebSocket(location))
            {
                webSocket.OnMessage += (sender, e) => Console.WriteLine(e.Data);
                webSocket.OnError += WebSocket_OnError; ;
                webSocket.SetCookie(new WebSocketSharp.Net.Cookie("name", name));
                webSocket.SetCookie(new WebSocketSharp.Net.Cookie("password", password));
                webSocket.Connect();

                Console.WriteLine("welcome!");

                while (true)
                {
                    var message = Console.ReadLine();

                    if (Console.CursorTop != 0)
                        MoveCarriage();

                    webSocket.Send(message);
                }
            }
        }

        private static void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            if (e.Exception is AccessViolationException)
            {
                //re-prompt to login
            }
            Console.WriteLine("{0}\n{1}", e.Message, e.Exception.Message);
        }

        static void MoveCarriage()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}
