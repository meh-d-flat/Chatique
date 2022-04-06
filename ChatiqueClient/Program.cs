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
        static string name, password = "";

        static string[] commands;

        static void Main(string[] args)
        {
            commands = args;
            Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;
            Chat();
        }

        private static void Chat()
        {
            Login();
            InitWebSocket();
        }

        static void Login()
        {
            string greeting = "what's your name: ";
            string askPass = "password: ";

            Console.Write(greeting);
            name = Console.ReadLine();
            while (String.IsNullOrWhiteSpace(name) && String.IsNullOrEmpty(name))
            {
                Console.SetCursorPosition(greeting.Length, 0);
                name = Console.ReadLine();
            }

            Console.WriteLine(askPass);
            password = Console.ReadLine();
            while (String.IsNullOrWhiteSpace(password) && String.IsNullOrEmpty(password))
            {
                Console.SetCursorPosition(askPass.Length, 0);
                password = Console.ReadLine();
            }
            using (SHA512 sha = SHA512Managed.Create())
            {
                password = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
                password = password.Replace("-", "");
                password = password.ToLower();
            }
            Console.Clear();
        }

        private static void InitWebSocket()
        {
            string location = commands.Length > 0
                ? String.Format("ws://{0}:{1}/", commands[0], commands[1])
                : "ws://localhost:8087/";

            using (var webSocket = new WebSocket(location))
            {
                webSocket.SetCookie(new WebSocketSharp.Net.Cookie("name", name));
                webSocket.SetCookie(new WebSocketSharp.Net.Cookie("password", password));
                webSocket.OnMessage += Received;
                webSocket.OnError += Error;
                webSocket.OnClose += Close;
                webSocket.Connect();
                if (webSocket.IsAlive)
                {
                    while (true)
                    {
                        var message = Console.ReadLine();

                        if (Console.CursorTop != 0)
                            MoveCarriage();

                        if (webSocket.ReadyState == WebSocketState.Open)
                            webSocket.Send(message);
                        else
                        {
                            webSocket.OnMessage -= Received;
                            webSocket.OnError -= Error;
                            webSocket.OnClose -= Close;
                            break;
                        }

                    }
                }
            }

            return;
        }

        private static void Received(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void Close(object sender, CloseEventArgs e)
        {
            Console.WriteLine(e.Reason);
            Console.WriteLine("wait for reconnect and try again");
            Chat();
        }

        private static void Error(object sender, ErrorEventArgs e)
        {
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
