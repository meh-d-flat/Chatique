using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Chatique
{
    public class Chat : WebSocketBehavior
    {
        string userName;

        void SaveMessage(Post message)
        {
            HistoryLog<Post>.AddMessageToHistory(message);
            var possibleError = Vault.AddMessageToDB(message);
            if (possibleError != null)
            {
                Console.WriteLine("{0} | code - {1}in:\nname, sessionid,\n{2}, {3}\n{4}", possibleError.Message, possibleError.ResultCode, this.userName, this.ID, possibleError.StackTrace);
                this.Error("an unexpected error occured!", possibleError);
            }
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
            var password = Sessions[this.ID].Context.CookieCollection["password"]?.Value;
            if (!Vault.CheckCredential(userName, password) && this.Context.CookieCollection["authenticated"]?.Value != "true")
            {
                this.Context.WebSocket.Close(CloseStatusCode.ServerError, "incorrect username or password!");
                return;
            }
            Console.WriteLine("{0} : new connection, user name: {1}", Sessions[this.ID].StartTime, userName);
            //foreach (var item in HistoryLog<Post>.History)
            //    Send(item.ToString());
            foreach (var item in Vault.GetAllMessages())
                Send(item.ToString());
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Console.WriteLine(e.Reason);
        }
    }
}
