using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Chatique
{
    public static class HistoryLog<T>
    {
        static List<T> history;

        static HistoryLog()
        {
            history = new List<T>();
        }

        public static int Count
        {
            get
            {
                return history.Count;
            }
        }

        public static IEnumerable<T> History
        {
            get
            {
                foreach (T item in history)
                    yield return item;
            }
        }

        public static bool AddMessageToHistory(T message)
        {
            history.Add(message);
            return true;
        }
    }

    public class Laputa : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            HistoryLog<string>.AddMessageToHistory(e.Data);
            Sessions.Broadcast(e.Data);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Console.WriteLine("new connection");
            foreach (string item in HistoryLog<string>.History)
                Send(item);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(8087);
            //var wssv = new WebSocketServer(8087, true);

            wssv.AddWebSocketService<Laputa>("/");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }

    public static class Extensions
    {
        public static bool SendWithCheck(this WebSocket webSocket, string message)
        {
            bool sent = false;
            try
            {
                webSocket.Send(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                sent = true;
            }
            return sent;
        }
    }
}
