using System;

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
}
