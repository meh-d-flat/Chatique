using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Data.SQLite;
using System.IO;

namespace Chatique
{
    public class Post
    {
        public Post(string message, string user)
        {
            Message = message;
            Username = user;
            Timestamp = DateTime.Now;
        }

        public Post(DateTime timestamp, string user, string message)
        {
            Message = message;
            Username = user;
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }
        public string Message { get; }
        public string Username { get; }

        public override string ToString()
        {
            return String.Format("[{0}]|{1}: {2}",
                        Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        Username,
                        Message);
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

    public class Vault
    {
        public static void Configure()
        {
            if (!File.Exists("database.sqlite3"))
                SQLiteConnection.CreateFile("database.sqlite3");

            using (var connection = new SQLiteConnection("Data Source=database.sqlite3"))
            {
                connection.Open();
                string createTableQuery = 
                    @"CREATE TABLE IF NOT EXISTS Posts
                    (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME,
                        Message VARCHAR(255),
                        Username VARCHAR(255)
                    )";
                using (SQLiteCommand createTable = new SQLiteCommand(createTableQuery, connection))
                    createTable.ExecuteNonQuery();

                connection.Close();
            }
        }

        public static void AddMessageToDB(Post message)
        {
            using (var connection = new SQLiteConnection("Data Source=database.sqlite3"))
            {
                connection.Open();
                string addMessageQuery = "INSERT INTO Posts ('Timestamp', 'Message', 'Username') VALUES(@a, @b, @c)";
                using (SQLiteCommand addMessage = new SQLiteCommand(addMessageQuery, connection))
                {
                    addMessage.Parameters.AddWithValue("@a", message.Timestamp);
                    addMessage.Parameters.AddWithValue("@b", message.Message);
                    addMessage.Parameters.AddWithValue("@c", message.Username);
                    addMessage.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public static List<Post> GetAllMessages()
        {
            var allPosts = new List<Post>();
            using (var connection = new SQLiteConnection("Data Source=database.sqlite3"))
            {
                connection.Open();
                string historyQuery = "SELECT Timestamp, Username, Message FROM Posts";
                using (SQLiteCommand history = new SQLiteCommand(historyQuery, connection))
                {
                    var posts = history.ExecuteReader();
                    if (posts.HasRows)
                    {
                        while (posts.Read())
                            allPosts.Add(new Post(posts.GetDateTime(0), posts.GetString(1), posts.GetString(2)));
                    }
                }
                connection.Close();
            }
            return allPosts;
        }
    }

    public class Chat : WebSocketBehavior
    {
        string userName;

        void SaveMessage(Post message)
        {
            HistoryLog<Post>.AddMessageToHistory(message);
            Vault.AddMessageToDB(message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var chatMessage = new Post(e.Data, userName);
            SaveMessage(chatMessage);
            Sessions.Broadcast(chatMessage.ToString());
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            userName = Sessions[this.ID].Context.CookieCollection["name"]?.Value;
            Console.WriteLine("{0} : new connection, user name: {1}",Sessions[this.ID].StartTime, userName);
            //foreach (var item in HistoryLog<Post>.History)
            //    Send(item.ToString());
            foreach (var item in Vault.GetAllMessages())
                Send(item.ToString());
        }
    }

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
