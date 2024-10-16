using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Forms;

namespace lchatserver
{
    public partial class Form1 : MetroForm
    {
        private UdpClient udpServer;
        private IPEndPoint remoteEndPoint;
        private bool isRunning;
        private int port;
        private readonly object lockObj = new object();
        private List<IPEndPoint> connectedClients = new List<IPEndPoint>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            konsole.Items.Add("Program Yüklendi");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void StartServer()
        {
            int port; // Port numarasını tutacak değişken
                  
            if (int.TryParse(porttxt.Text, out port))
            {
                udpServer = new UdpClient(new IPEndPoint(IPAddress.Any, port)); // TextBox'tan alınan port ile dinle
                udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                remoteEndPoint = new IPEndPoint(IPAddress.Any, port); // Hedef IP
                isRunning = true;

                // Form durumunu güncelle
                konsole.Items.Add("Sunucu başlatıldı, dinleniyor...");
                

                Task.Run(() => ListenForClients()); // Arka planda dinleme başlat
            }
            else
            {
                MessageBox.Show("Geçerli bir port numarası girin."); // Hata mesajı
            }
        }
        private void ListenForClients()
        {
            while (true)
            {
                byte[] receivedData = udpServer.Receive(ref remoteEndPoint); // Gelen veriyi al
                string clientInfo = $"{remoteEndPoint.Address}:{remoteEndPoint.Port}";

                // İstemci listesini güncelle
                if (!Klist.Items.Contains(clientInfo))
                {
                    Klist.Invoke((MethodInvoker)delegate {
                        Klist.Items.Add(clientInfo);
                    });
                }

                // Gelişmiş işlem (Ses gönderme) ekleyebilirsin
                konsole.Items.Add($"Veri alındı: {receivedData.Length} bytes - {clientInfo}");
            }
            while (isRunning)
            {
                // İstemci bağlantısını dinle
                var receivedData = udpServer.Receive(ref remoteEndPoint);

                // Yeni istemci ekleme
                if (!connectedClients.Contains(remoteEndPoint))
                {
                    connectedClients.Add(remoteEndPoint);
                    konsole.Items.Add(remoteEndPoint.ToString()); // İstemci listesini güncelle
                }

                // Alınan veriyi diğer istemcilere gönderme
                BroadcastData(receivedData);
            }
            while (isRunning)
            {
                try
                {
                    var receivedData = udpServer.Receive(ref remoteEndPoint);
                    // Diğer işlemler
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"Socket hatası: {ex.Message}");
                    break; // Hata alındığında döngüden çık
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}");
                    break; // Diğer hatalar için
                }
            }
        }
        private void BroadcastData(byte[] data)
        {
            foreach (var client in connectedClients)
            {
                udpServer.Send(data, data.Length, client);
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            StartServer();
        }
        private void StopServer()
        {
            lock (lockObj) // Thread güvenliği için kilit kullanıyoruz
            {
                if (isRunning)
                {
                    try
                    {
                        if (udpServer != null)
                        {
                            udpServer.Close();
                            udpServer = null; // null yaparak tekrar kapatmayı önle
                        }

                        isRunning = false;
                        konsole.Items.Add("Sunucu durduruldu.");
                    }
                    catch (SocketException ex)
                    {
                        MessageBox.Show($"Socket hatası: {ex.Message}"); // Hata mesajını göster
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Hata: {ex.Message}"); // Diğer hataları göster
                    }
                }
            }
        }
        private void metroButton2_Click(object sender, EventArgs e)
        {
            StopServer();
        }

    }
}
