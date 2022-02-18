using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Chatique
{
    public class Message
    {
        public Message(string message, string user)
        {
            Text = message;
            Username = user;
            Timestamp = DateTime.Now;
        }
        public DateTime Timestamp { get; }
        public string Text { get; }
        public string Username { get; }

        public override string ToString()
        {
            return String.Format("[{0}]|{1}: {2}",
                        Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        Username,
                        Text);
        }
    }

    public static class HistoryLog<T>
    {
        static List<T> history;

        static HistoryLog()
        {
            history = new List<T>();
        }

        public static IEnumerable<T> History
        {
            get
            {
                foreach (T item in history)
                    yield return item;
            }
        }

        public static int HistoryCount()
        {
            return history.Count;
        }

        public static bool AddMessageToHistory(T message)
        {
            history.Add(message);
            return true;
        }
    }

    public class Laputa : WebSocketBehavior
    {
        bool credentialsNotReceived = true;
        string userName;

        protected override void OnMessage(MessageEventArgs e)
        {
            if (credentialsNotReceived && !Sessions[this.ID].Context.Headers["User-Agent"].Contains("websocket-sharp"))
            {
                userName = e.Data;
                credentialsNotReceived = false;
            }
            else
            {
                var chatMessage = new Message(e.Data, userName);
                HistoryLog<Message>.AddMessageToHistory(chatMessage);
                Sessions.Broadcast(chatMessage.ToString());
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Console.WriteLine("new connection");
            //userName = Sessions[this.ID].Context.Headers["User-Agent"].Contains("websocket-sharp")
            //    ? Sessions[this.ID].Context.CookieCollection["name"]?.Value
            //    : Sessions[this.ID].Context.QueryString["name"];
            userName = Sessions[this.ID].Context.CookieCollection["name"]?.Value;
            foreach (var item in HistoryLog<Message>.History)
                Send(item.ToString());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer wssv = args.Length > 0 
                ? new WebSocketServer(Convert.ToInt32(args[0]))
                : new WebSocketServer(8087);
            wssv.AddWebSocketService<Laputa>("/");
            wssv.Start();
            Console.ReadKey(true);
            wssv.Stop();
        }
    }
}
