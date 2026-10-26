using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace TcpClientWPF
{
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _recvBuffer = new byte[1024];

        public MainWindow()
        {
            InitializeComponent();
        }

        // 连接 PLC
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ip = txtIP.Text;
                int port = int.Parse(txtPort.Text);

                _client = new TcpClient();
                _client.Connect(ip, port);
                _stream = _client.GetStream();

                Log("已连接到 PLC");

                // 开始异步接收
                _stream.BeginRead(_recvBuffer, 0, _recvBuffer.Length, OnReceive, null);
            }
            catch (Exception ex)
            {
                Log("连接失败：" + ex.Message);
            }
        }

        // 接收数据回调
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int len = _stream.EndRead(ar);
                if (len == 0)
                {
                    Log("PLC 断开连接");
                    return;
                }

                string data = Encoding.UTF8.GetString(_recvBuffer, 0, len);

                Dispatcher.Invoke(() =>
                {
                    txtReceive.Text = data;
                });

                // 继续接收
                _stream.BeginRead(_recvBuffer, 0, _recvBuffer.Length, OnReceive, null);
            }
            catch
            {
                Log("接收异常");
            }
        }

        // 发送数据到 PLC
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string msg = txtSend.Text;
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _stream.Write(data, 0, data.Length);
                Log("已发送：" + msg);
            }
            catch (Exception ex)
            {
                Log("发送失败：" + ex.Message);
            }
        }

        // 断开
        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _stream?.Close();
            _client?.Close();
            Log("手动断开连接");
        }

        private void Log(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} {msg}\r\n");
                txtLog.ScrollToEnd();
            });
        }
    }
}