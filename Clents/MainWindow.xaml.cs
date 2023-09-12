using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Clents
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string host = "10.137.201.120";
        int port = 8888;
        TcpClient client = new TcpClient();
        StreamReader? Reader = null;
        StreamWriter? Writer = null;

        public MainWindow()
        {
            InitializeComponent();

        }
        string? userName;
        private async void btnConfirmName_Click(object sender, RoutedEventArgs e)
        {
            if (nameClient.Text.Length != 0)
            {
                userName = nameClient.Text;
                btnConfirmName.Visibility = Visibility.Collapsed;
                btnExit.Visibility = Visibility.Visible;

                try
                {
                    client.Connect(host, port); //подключение клиента
                    Reader = new StreamReader(client.GetStream());
                    Writer = new StreamWriter(client.GetStream());
                    if (Writer is null || Reader is null) return;
                    chat.Text += $"Добро пожаловать, {userName}!\n";

                    Task.Run(() => ReceiveMessageAsync(Reader));
                    await SendMessageUserNameAsync(Writer);
                }
                catch {
                    chat.Text += $"Нет соединения с сервером!\n";
                }
                finally
                {
                    Writer?.Close();
                    Reader?.Close();
                }
                
            }
            else
            {

                MessageBox.Show("Вы не ввели имя!");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Writer?.Close();
            Reader?.Close();
            this.Close();
            btnConfirmName.Visibility = Visibility.Visible;
            btnExit.Visibility = Visibility.Hidden;
        }

        // отправка сообщений
        async Task SendMessageUserNameAsync(StreamWriter writer)
        {
            // сначала отправляем имя
            await writer.WriteLineAsync(userName);
            await writer.FlushAsync();

        }

        // отправка сообщений
        async Task SendMessageAsync(StreamWriter writer)
        {
            string? message = textArea.Text;
            chat.Text += $"{userName}: "+ message+"\n";
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();

        }
        // получение сообщений
        async Task ReceiveMessageAsync(StreamReader reader)
        {

            while (true)
            {
                try
                {

                    // считываем ответ в виде строки
                    string? message = await reader.ReadLineAsync();

                    // если пустой ответ, ничего не выводим на консоль
                    if (string.IsNullOrEmpty(message)) continue;

                    Thread t = new Thread(
    o =>
    {
        Thread.Sleep(2000);
        chat.Dispatcher.BeginInvoke(
            (Action)(() => { chat.Text += message + "\n"; }));
        Thread.Sleep(2000);
    });
                    t.Start();

                   

                  
                }
                catch
                {
                    break;
                }
            }
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Writer is null)
                return;
            await SendMessageAsync(Writer);
        }
    }
}
