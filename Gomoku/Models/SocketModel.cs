namespace Gomoku.Models
{
    using System;
    using Quobject.SocketIoClientDotNet.Client;
    using System.ComponentModel;
    using Newtonsoft.Json.Linq;
    using System.Threading;

    public class SocketModel
    {
        private static string Uri = "ws://gomoku-lajosveres.rhcloud.com:8000";
        public static Socket socket;

        public event MessageHandler OnChatMessage;
        public event PlayAtHandler OnPlayAt;
        public event StartHandler OnStart;
        public event EndHandler OnEndGame;

        /// <summary>
        /// Khởi tạo một thể hiện mới cho class SocketModel
        /// </summary>
        public SocketModel()
        {
            socket = IO.Socket(Uri);
        }

        /// <summary>
        /// Tạo các sự kiện cho socket
        /// </summary>
        public void Init()
        {
            // Xử lý sự kiện ChatMessage từ server
            socket.On("ChatMessage", (data) =>
            {
                JObject jObject = (Newtonsoft.Json.Linq.JObject)data;
                if (jObject.Count == 1)
                {
                    string msg = jObject["message"].ToString();
                    if (msg.Contains("<br />"))
                    {
                        msg = msg.Replace("<br />", '\n'.ToString());
                        if (msg.Contains("first"))
                        {
                            OnStart(OCo.CellValues.Player2);
                        }
                        else
                        {
                            OnStart(OCo.CellValues.Player1);
                        }
                    }

                    OnChatMessage(from: "Server", message: msg);
                }
                else
                {
                    OnChatMessage(from: jObject["from"].ToString(), message: jObject["message"].ToString());
                }
            });

            // Xử lý sự kiện NextStepIs từ server
            socket.On("NextStepIs", (data) =>
            {
                JObject jObject = (Newtonsoft.Json.Linq.JObject)data;
                OnPlayAt(int.Parse(jObject["row"].ToString()), int.Parse(jObject["col"].ToString()));
            });

            socket.On("EndGame", (data) =>
            {
                JObject jObject = (Newtonsoft.Json.Linq.JObject)data;
                OnEndGame(message: jObject["message"].ToString());
            });
        }

        public void Connect()
        {
            socket = IO.Socket(Uri);
        }

        public void Disconnect()
        {
            socket.Disconnect();
        }

        public static void SendMessage(string message)
        {
            socket.Emit("ChatMessage", message);
        }

        public void ChangeName(string name)
        {
            socket.Emit("MyNameIs", name);
        }

        public void ConnectToOtherPlayer()
        {
            socket.Emit("ConnectToOtherPlayer");
        }

        public void PlayAt(int row, int col)
        {
            socket.Emit("MyStepIs", JObject.FromObject(new { row = row, col = col }));
        }
        
        public delegate void MessageHandler(string from, string message);
        public delegate void PlayAtHandler(int row, int col);
        public delegate void StartHandler(OCo.CellValues FirstPlayer);
        public delegate void EndHandler(string message);
    }
}
