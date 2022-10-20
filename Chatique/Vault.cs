using System;
using System.Configuration;
using System.Management;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace Chatique
{
    public class Vault
    {
        static readonly string dbFile = ConfigurationManager.AppSettings["dbFile"].ToString();
        static readonly string connectionString = ConfigurationManager.AppSettings["connectionString"].ToString();

        public static void Configure()
        {
            if (!File.Exists(dbFile))
                SQLiteConnection.CreateFile(dbFile);

            using (var connection = new SQLiteConnection(connectionString))
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
                        Username VARCHAR(255) NOT NULL UNIQUE,
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
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string addMessageQuery = "INSERT INTO Posts ('Timestamp', 'Message', 'Username') VALUES(@a, @b, @c)";
                SQLiteCommand addMessage = new SQLiteCommand(addMessageQuery, connection);
                addMessage.Parameters.Add(new SQLiteParameter() { ParameterName = "@a", Value = message.Timestamp, DbType = DbType.DateTime });
                addMessage.Parameters.Add(new SQLiteParameter() { ParameterName = "@b", Value = message.Message, DbType = DbType.String });
                addMessage.Parameters.Add(new SQLiteParameter() { ParameterName = "@c", Value = message.Username, DbType = DbType.String});
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
            using (var connection = new SQLiteConnection(connectionString))
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

        public static string MakeHash(string password)
        {
            string hash = String.Empty;
            using (SHA512 sha = SHA512Managed.Create())
            {
                hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
                hash = hash.Replace("-", "");
                hash = hash.ToLower();
            }
            return hash;
        }

        public static bool CheckCredentialHashing(string username, string password)
        {
            string hash = MakeHash(password);
            return CheckCredential(username, hash);
        }

        public static bool CheckCredential(string username, string password)
        {
            bool exists = false;
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string credentialQuery = "SELECT COUNT(*) FROM Users WHERE Username = @a AND Password = @b";
                using (SQLiteCommand credential = new SQLiteCommand(credentialQuery, connection))
                {
                    credential.Parameters.Add(new SQLiteParameter() { ParameterName = "@a", Value = username, DbType = DbType.String });
                    credential.Parameters.Add(new SQLiteParameter() { ParameterName = "@b", Value = password, DbType = DbType.String });//AddWithValue("@b", password/*hash*/);
                    exists = Convert.ToBoolean(credential.ExecuteScalar());
                    credential.Dispose();
                }
                connection.Close();
            }
            return exists;
        }

        public static bool CreateCredential(string username, string password)
        {
            string hash = MakeHash(password);
            bool completed = false;
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string credentialQuery = "INSERT INTO Users ('Username', 'Password') VALUES(@a, @b)";
                using (SQLiteCommand credential = new SQLiteCommand(credentialQuery, connection))
                {
                    credential.Parameters.Add(new SQLiteParameter() { ParameterName = "@a", Value = username, DbType = DbType.String });
                    credential.Parameters.Add(new SQLiteParameter() { ParameterName = "@b", Value = hash, DbType = DbType.String });

                    try
                    {
                        completed = Convert.ToBoolean(credential.ExecuteNonQuery());
                    }
                    catch(SQLiteException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    credential.Dispose();
                }
                connection.Close();
            }
            return completed;
        }

        public static bool CreateCredential2(string username, string password)
        {
            bool created = false;
            string hash = MakeHash(password);
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string addMessageQuery = "INSERT INTO Users ('Username', 'Password') VALUES(@a, @b)";
                SQLiteCommand addMessage = new SQLiteCommand(addMessageQuery, connection);
                addMessage.Parameters.Add(new SQLiteParameter() { ParameterName = "@a", Value = username, DbType = DbType.String });
                addMessage.Parameters.Add(new SQLiteParameter() { ParameterName = "@b", Value = hash, DbType = DbType.String });
                try
                {
                    var result = addMessage.ExecuteNonQuery();
                    created = Convert.ToBoolean(result);
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex);
                    addMessage.Transaction.Rollback();
                }
                finally
                {
                    addMessage.Dispose();
                }
                connection.Close();
            }
            return created;
        }
    }
}
