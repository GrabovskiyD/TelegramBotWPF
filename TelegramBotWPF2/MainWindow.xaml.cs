using System.Windows;
using System.Windows.Input;


namespace TelegramBotWPF2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            TgBot tgBot = new TgBot(this);
            usersList.ItemsSource = tgBot.Users;
            sendButton.Click += delegate { tgBot.SendMessage(); };
            messageBox.KeyDown += (s, e) => { if (e.Key == Key.Return) { tgBot.SendMessage(); } };
        }
    }
}
