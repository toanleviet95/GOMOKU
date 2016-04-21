using Gomoku.Properties;
using Gomoku.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Gomoku.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BanCoViewModel BanCoViewModel;

        public MainWindow()
        {
            InitializeComponent();
            new LoadingScreen().ShowDialog();

            SetButtonClick();

            BanCoViewModel = new BanCoViewModel();
            BanCoViewModel.CurrentBanCo.OnPlayerAt += CurrentBanCo_OnPlayerAt;
            BanCoViewModel.CurrentBanCo.OnPlayerWin += CurrentBanCo_OnPlayerWin;
            BanCoViewModel.CurrentBanCo.OnUndo += CurrentBanCo_OnUndo;
            BanCoViewModel.CurrentBanCo.OnRedo += CurrentBanCo_OnRedo;
            BanCoViewModel.CurrentBanCo.OnLoad += CurrentBanCo_OnLoad;

            BanCoViewModel.CurrentBanCo.CurrentSocket.OnChatMessage += CurrentSocket_OnChatMessage;
            BanCoViewModel.CurrentBanCo.CurrentSocket.OnPlayAt += CurrentSocket_OnPlayAt;
            BanCoViewModel.CurrentBanCo.CurrentSocket.OnEndGame += CurrentSocket_OnEndGame;
            BanCoViewModel.CurrentBanCo.CurrentSocket.OnStart += CurrentSocket_OnStart;

            btnPvP.Foreground = new SolidColorBrush(Colors.Yellow);
            BanCoViewModel.PvP();

            changeName.IsEnabled = false;
            chatMessage.IsEnabled = false;

            btnChange.Content = "Start!";
        }

        /// <summary>
        /// Xử lý sự kiện bắt đầu
        /// </summary>
        void CurrentSocket_OnStart(Models.OCo.CellValues FirstPlayer)
        {
            if (BanCoViewModel.CurrentBanCo.CheDoChoi == Gomoku.Models.CheDoChoi.Computer && FirstPlayer == Models.OCo.CellValues.Player2)
            {
                BanCoViewModel.CurrentBanCo.AutoPlay();
            }
        }

        /// <summary>
        /// Xử lý sự kiện đánh tại một ô nào đó từ Server gửi về
        /// </summary>
        void CurrentSocket_OnPlayAt(int row, int col)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                SetButtonContent(board.Children[row * Settings.Default.BOARD_SIZE + col] as Button);

                if (BanCoViewModel.CurrentBanCo.CheDoChoi == Gomoku.Models.CheDoChoi.Computer &&
                    BanCoViewModel.CurrentBanCo.ActivePlayer == Models.OCo.CellValues.Player2 &&
                    BanCoViewModel.CurrentBanCo.Won == Models.OCo.CellValues.None)
                {
                    BanCoViewModel.CurrentBanCo.AutoPlay();
                }
            }));
        }

        /// <summary>
        /// Xử lý sự kiện kết thức trò chơi
        /// </summary>
        void CurrentSocket_OnEndGame(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                chatBox.Items.Add(new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), message));
                chatBox.ScrollIntoView(chatBox.Items[chatBox.Items.Count - 1]);
                btnChange.Content = "New game!";
            }));
        }

        /// <summary>
        /// Xử lý nhận tin nhắn từ Server
        /// </summary>
        void CurrentSocket_OnChatMessage(string from, string message)
        {
            this.Dispatcher.Invoke((Action)(()=>
            {
                chatBox.Items.Add(new ChatMessage(from, DateTime.Now.ToString("hh:mm:ss tt"), message));
                chatBox.ScrollIntoView(chatBox.Items[chatBox.Items.Count - 1]);
            }));
        }

        /// <summary>
        /// Xuất kết quả
        /// </summary>
        void CurrentBanCo_OnPlayerWin(Models.OCo.CellValues player)
        {
            MessageBox.Show(player.ToString() + " win!", "Result", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        /// <summary>
        /// Xử lý sự kiện Đánh tại một ô cờ tại vị trí (row, col)
        /// </summary>
        void CurrentBanCo_OnPlayerAt(int row, int col)
        {
            this.Dispatcher.Invoke((Action)(()=>
            {
                SetButtonContent(board.Children[row * Settings.Default.BOARD_SIZE + col] as Button);

                if (BanCoViewModel.CurrentBanCo.CheDoChoi == Gomoku.Models.CheDoChoi.Computer && 
                    BanCoViewModel.CurrentBanCo.ActivePlayer == Models.OCo.CellValues.Player1 &&
                    BanCoViewModel.CurrentBanCo.Won == Models.OCo.CellValues.None)
                {
                    BanCoViewModel.CurrentBanCo.AutoPlay();
                }
            }));
        }

        // Reset button Set mode
        private void ResetButton()
        {
            btnPvP.Foreground = btnPvC.Foreground = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Thêm sự kiện click cho các button Cell
        /// </summary>
        private void SetButtonClick()
        {
            foreach (Control control in board.Children)
            {
                Button cell = (Button)control;
                cell.Click += this.Cell_Click;
            }
        }

        /// <summary>
        /// Xử lý sự kiện Cell click
        /// </summary>
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button cell = (Button)sender;
            if (btnOnline.IsChecked == true)    // Online
            {
                // Gửi bước đánh đến server
                BanCoViewModel.CurrentBanCo.CurrentSocket.PlayAt(Grid.GetRow(cell), Grid.GetColumn(cell));
            }
            else                                // Offline
            {
                if (BanCoViewModel.CanPlayAt(Grid.GetRow(cell), Grid.GetColumn(cell)))
                {
                    // Đánh vào ô cờ đang xét
                    BanCoViewModel.CurrentBanCo.PlayAt(Grid.GetRow(cell), Grid.GetColumn(cell));
                }
            }
        }

        /// <summary>
        /// Thêm content vào button cell đang xét
        /// </summary>
        private void SetButtonContent(Button cell)
        {
            if (BanCoViewModel.CurrentBanCo.ActivePlayer == Models.OCo.CellValues.Player1)
            {
                cell.Content = CreateEllipse(Colors.Black);
            }
            else
            {
                cell.Content = CreateEllipse(Colors.White);
            }
        }

        /// <summary>
        /// Tạo hình ellipse
        /// </summary>
        private Ellipse CreateEllipse(Color color)
        {
            Ellipse ellipse = new Ellipse();

            ellipse.Width = 30;
            ellipse.Height = 30;
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.Fill = new SolidColorBrush(color);

            return ellipse;
        }

        /// <summary>
        /// Reset bàn cờ
        /// </summary>
        private void ResetBoard()
        {
            foreach (Button cell in board.Children)
            {
                cell.Content = null;
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn các phím tắt
        /// </summary>
        private void GomokuWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Z)
            {
                Undo();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Y)
            {
                Redo();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.N)
            {
                NewGame();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.S)
            {
                SaveGame();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.L)
            {
                LoadGame();
            }
        }

        #region NewGame
        private void NewGame()
        {
            if (BanCoViewModel.CurrentBanCo.CheDoChoi == Models.CheDoChoi.Player)
            {
                PvPmode();
            }
            else
            {
                PvCmode();
            }
        }

        private void menuNewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }
        #endregion

        #region Load/Save
        private void menuSaveGame_Click(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }

        private void SaveGame()
        {
            if (BanCoViewModel.CurrentBanCo.IsOnline == false)
            {
                string savePath = "";

                if (BanCoViewModel.CurrentBanCo.CheDoChoi == Models.CheDoChoi.Player)
                {
                    savePath = @"SavePvP.dat";
                }
                else
                {
                    savePath = @"SavePvC.dat";
                }

                BanCoViewModel.CurrentBanCo.Save(savePath);
                MessageBox.Show("Saved successfully !", "System", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void menuLoadGame_Click(object sender, RoutedEventArgs e)
        {
            LoadGame();
        }

        private void LoadGame()
        {
            if (BanCoViewModel.CurrentBanCo.IsOnline == false)
            {
                ResetBoard();

                string savePath = "";

                if (BanCoViewModel.CurrentBanCo.CheDoChoi == Models.CheDoChoi.Player)
                {
                    savePath = @"SavePvP.dat";
                }
                else
                {
                    savePath = @"SavePvC.dat";
                }

                BanCoViewModel.CurrentBanCo.Load(savePath);
            }
        }

        void CurrentBanCo_OnLoad(Models.OCo Oco)
        {
            Button cell = board.Children[Oco.Row * Settings.Default.BOARD_SIZE + Oco.Col] as Button;

            if (Oco.OfPlayer == Models.OCo.CellValues.Player1)
            {
                cell.Content = CreateEllipse(Colors.Black);
            }
            else
            {
                cell.Content = CreateEllipse(Colors.White);
            }
        }
        #endregion

        /// <summary>
        /// Đóng chương trình
        /// </summary>
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region Set mode
        private void menuPvP_Click(object sender, RoutedEventArgs e)
        {
            PvPmode();
        }

        private void PvPmode()
        {
            ResetBoard();
            ResetButton();

            btnPvP.Foreground = new SolidColorBrush(Colors.Yellow);
            BanCoViewModel.PvP();
        }

        private void menuPvC_Click(object sender, RoutedEventArgs e)
        {
            PvCmode();
        }

        private void PvCmode()
        {
            ResetBoard();
            ResetButton();

            btnPvC.Foreground = new SolidColorBrush(Colors.Yellow);
            BanCoViewModel.PvC();
        }

        private void btnPvP_Click(object sender, RoutedEventArgs e)
        {
            if (btnOnline.IsChecked == true)
            {
                ResetButton();

                btnPvP.Foreground = new SolidColorBrush(Colors.Yellow);
                BanCoViewModel.CurrentBanCo.CheDoChoi = Models.CheDoChoi.Computer;
            }
            else
            {
                PvPmode();
            }
        }

        private void btnPvC_Click(object sender, RoutedEventArgs e)
        {
            if (btnOnline.IsChecked == true)
            {
                ResetButton();

                btnPvC.Foreground = new SolidColorBrush(Colors.Yellow);
                BanCoViewModel.CurrentBanCo.CheDoChoi = Models.CheDoChoi.Computer;
            }
            else
            {
                PvCmode();
            }
        }
        #endregion


        #region Undo/Redo
        private void menuUndo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void Undo()
        {
            if (btnOnline.IsChecked == false)
            {
                BanCoViewModel.CurrentBanCo.Undo();
            }
        }

        private void menuRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void Redo()
        {
            if (btnOnline.IsChecked == false)
            {
                BanCoViewModel.CurrentBanCo.Redo();
            }
        }

        void CurrentBanCo_OnRedo(Models.OCo OCo)
        {
            SetButtonContent(board.Children[OCo.Row * Settings.Default.BOARD_SIZE + OCo.Col] as Button);
        }

        void CurrentBanCo_OnUndo(Models.OCo OCo)
        {
            (board.Children[OCo.Row * Settings.Default.BOARD_SIZE + OCo.Col] as Button).Content = null;
        }

        private void menuEdit_MouseMove(object sender, MouseEventArgs e)
        {
            if (BanCoViewModel.CanUndo())
            {
                menuUndo.IsEnabled = true;
            }
            else
            {
                menuUndo.IsEnabled = false;
            }

            if (BanCoViewModel.CanRedo())
            {
                menuRedo.IsEnabled = true;
            }
            else
            {
                menuRedo.IsEnabled = false;
            }
        }
        #endregion

        /// <summary>
        /// Xử lý sự kiện Button Online bật tắt
        /// </summary>
        private void btnOnline_Click(object sender, RoutedEventArgs e)
        {
            if (btnOnline.IsChecked.Value == true)
            {
                btnChange.Content = "Start!";

                changeName.IsEnabled = true;
                chatMessage.IsEnabled = true;
                menuNewGame.IsEnabled = false;
                menuLoadGame.IsEnabled = false;


                BanCoViewModel.CurrentBanCo.CurrentSocket.Connect();
                BanCoViewModel.CurrentBanCo.CurrentSocket.Init();
                BanCoViewModel.CurrentBanCo.IsOnline = true;
            }
            else
            {
                changeName.IsEnabled = false;
                chatMessage.IsEnabled = false;
                menuNewGame.IsEnabled = true;
                menuLoadGame.IsEnabled = true;

                BanCoViewModel.CurrentBanCo.CurrentSocket.Disconnect();
                BanCoViewModel.CurrentBanCo.IsOnline = false;
            }
        }

        private void menuFile_MouseMove(object sender, MouseEventArgs e)
        {
            if (btnOnline.IsChecked == true)
            {
                menuSaveGame.IsEnabled = false;
                menuLoadGame.IsEnabled = false;
            }
            else
            {
                menuSaveGame.IsEnabled = true;
                menuLoadGame.IsEnabled = true;
            }
        }

        /// <summary>
        /// Xử lý sự kiện click của button Change
        /// </summary>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (btnChange.Content.ToString() == "Start!" || btnChange.Content.ToString() == "New game!")
            {
                btnChange.Content = "Change!";

                ResetBoard();
                BanCoViewModel.CurrentBanCo.Reset();
                BanCoViewModel.CurrentBanCo.CurrentSocket.ChangeName(txtYourName.Text);
                BanCoViewModel.CurrentBanCo.CurrentSocket.ConnectToOtherPlayer();
            }
            else
            {
                BanCoViewModel.CurrentBanCo.CurrentSocket.ChangeName(txtYourName.Text);
            }
        }

        #region Message / Send message
        /// <summary>
        /// Xử lý sự kiện click button Send message
        /// </summary>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Models.SocketModel.SendMessage(txtMessage.Text);
            txtMessage.Text = "";
        }

        /// <summary>
        /// Có focus
        /// </summary>
        private void txtMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMessage.Text = "";
        }

        /// <summary>
        /// Bắt sự kiện phím Enter để send message
        /// </summary>
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Models.SocketModel.SendMessage(txtMessage.Text);
                txtMessage.Text = "";
            }
        }

        /// <summary>
        /// Mất focus
        /// </summary>
        private void txtMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            //txtMessage.Text = "Type your message here...";
        }
        #endregion
    }
}
