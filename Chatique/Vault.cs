using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Chatique
{
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
                    );
                    CREATE TABLE IF NOT EXISTS Users
                    (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username VARCHAR(255) NOT NULL,
                        Password VARCHAR(255) NOT NULL
                    )";
                using (SQLiteCommand createTable = new SQLiteCommand(createTableQuery, connection))
                    createTable.ExecuteNonQuery();

                connection.Close();
            }
        }

        public static SQLiteException AddMessageToDB(Post message)
        {
            SQLiteException sqlException = null;
            using (var connection = new SQLiteConnection("Data Source=database.sqlite3"))
            {
                connection.Open();
                string addMessageQuery = "INSERT INTO Posts ('Timestamp', 'Message', 'Username') VALUES(@a, @b, @c)";
                SQLiteCommand addMessage = new SQLiteCommand(addMessageQuery, connection);
                addMessage.Parameters.AddWithValue("@a", message.Timestamp);
                addMessage.Parameters.AddWithValue("@b", message.Message);
                addMessage.Parameters.AddWithValue("@c", message.Username);
                try
                {
                    addMessage.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    sqlException = ex;
                    addMessage.Transaction.Rollback();
                }
                finally
                {
                    addMessage.Dispose();
                }
                connection.Close();
            }
            return sqlException;
        }

        //add pagination
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

        public static bool CheckCredentialHashing(string username, string password)
        {
            string hash = String.Empty;
            using (SHA512 sha = SHA512Managed.Create())
            {
                hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
                hash = hash.Replace("-", "");
                hash = hash.ToLower();
            }
            return CheckCredential(username, hash);
        }

        public static bool CheckCredential(string username, string password)
        {
            bool exists = false;
            using (var connection = new SQLiteConnection("Data Source=database.sqlite3"))
            {
                connection.Open();
                string credentialQuery = "SELECT COUNT(*) FROM Users WHERE Username = @a AND Password = @b";
                using (SQLiteCommand credential = new SQLiteCommand(credentialQuery, connection))
                {
                    credential.Parameters.AddWithValue("@a", username);
                    credential.Parameters.AddWithValue("@b", password/*hash*/);
                    exists = Convert.ToBoolean(credential.ExecuteScalar());
                }
                connection.Close();
            }
            return exists;
        }
    }
}
