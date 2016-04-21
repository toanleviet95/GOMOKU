namespace Gomoku.Models
{
    using System.ComponentModel;

    public class Message : INotifyPropertyChanged
    {
        private string _text;
        public string Text 
        { 
            get
            {
                return _text;
            }
            set 
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        /// <summary>
        /// Khởi tạo một thể hiện mới cho class Message
        /// </summary>
        /// <param name="message"></param>
        public Message(string message)
        {
            Text = message;
        }

        #region InotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
