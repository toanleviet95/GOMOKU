namespace Gomoku.ViewModels
{
    using Gomoku.Command;
    using Gomoku.Models;
    using System;
    using System.Windows.Input;

    public class MessageViewModel
    {
        private Message _message;
        public Message Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public MessageViewModel()
        {
            Message = new Message("Type your message here...");
            SendCommand = new SendMessageCommand(this);
        }

        public bool CanSend 
        {
            get
            {
                if (Message == null)
                {
                    return false;
                }
                return !String.IsNullOrWhiteSpace(Message.Text);
            }
        }

        public ICommand SendCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Send message to server
        /// </summary>
        public void SendMessage()
        {
            //SocketModel.SendMessage(Message.Text);
        }
    }
}
