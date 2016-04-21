namespace Gomoku.Command
{
    using Gomoku.ViewModels;
    using System.Windows.Input;

    internal class SendMessageCommand : ICommand
    {
        private MessageViewModel _ViewModel;

        public SendMessageCommand(MessageViewModel viewModel)
        {
            _ViewModel = viewModel;
        }

        #region Icommand Members

        public event System.EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return _ViewModel.CanSend;
        }

        public void Execute(object parameter)
        {
            _ViewModel.SendMessage();
        }

        #endregion
    }
}
