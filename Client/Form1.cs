using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        TcpClient client;


        public void ConnectAndLoop(TcpClient client)
        {
            byte[] bytesFrom = new byte[1024];
            string dataFromClient = null;
            byte[] sendBytes = null;
            string serverResponse = null;

            while ((true))
            {
                try
                {
                    NetworkStream networkStream = client.GetStream();

                    networkStream.Read(bytesFrom, 0, 1024);
                    dataFromClient = Encoding.UTF8.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("\0"));

                    if (dataFromClient.Contains("!ELENDİNİZ!"))
                    {
                        client.Close();
                        lblDurum.Text = "Maalesef oyunu kazanamadınız, bir daha ki sefere inşallah.";
                        break;
                        //MessageBox.Show("Soruya yanlış cevap verdiğiniz için elendiniz...", "Üzgünüz!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (dataFromClient.StartsWith("|") && dataFromClient.EndsWith("|"))
                    {
                        dataFromClient = dataFromClient.Replace("|", "");
                        string soru = dataFromClient.Split('-')[0];
                        string a = dataFromClient.Split('-')[1];
                        string b = dataFromClient.Split('-')[2];
                        string c = dataFromClient.Split('-')[3];
                        string d = dataFromClient.Split('-')[4];

                        lblSoru.Text = soru;

                        btnA.Text = a;
                        btnB.Text = b;
                        btnC.Text = c;
                        btnD.Text = d;

                        foreach (Control btn in this.Controls)
                        {
                            if (btn is Button)
                            {
                                if (btn.Name.Contains("btn"))
                                {
                                    btn.Enabled = true;
                                }
                            }
                        }

                        this.Invoke((MethodInvoker)(() => { timer1.Enabled = true; }));
                        lblDurum.Text = "Lütfen soruyu verilen süre içerisinde cevaplayınız!";
                    }
                    else if (dataFromClient.Contains("!DOGRUCEVAP!"))
                    {
                        lblDurum.Text = "Tebrikler! Doğru cevap verdiniz, öteki soru için lütfen bekleyiniz...";
                        lblSoru.Text = "";
                        btnA.Text = "";
                        btnB.Text = "";
                        btnC.Text = "";
                        btnD.Text = "";
                    }
                    else if (dataFromClient.Contains("!KAZANDIN!"))
                    {
                        lblDurum.Text = "Tebrikler! Oyunu kazandınız...";
                        MessageBox.Show("TEBRİKLER!!! OYUNU KAZANDINIZ!!!", "İŞTE BUDUR!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        client.Close();
                        break;
                    }

                    Array.Clear(bytesFrom, 0, 1024);
                    //richTextBox1.AppendText(" >> " + "From server -" + dataFromClient + "\n");
                    networkStream.Flush();
                }
                catch (Exception ex)
                {
                    client.Close();
                    lblDurum.Text = "Maalesef oyunu kazanamadınız, bir daha ki sefere inşallah.";
                    //MessageBox.Show("Soruya yanlış cevap verdiğiniz için elendiniz...", "Üzgünüz!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 8000);

            Thread th = new Thread(() => ConnectAndLoop(client));
            th.Start();

            NetworkStream networkStream = client.GetStream();

            string serverResponse = "Name:" + txtNickName.Text;
            byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
            networkStream.Write(sendBytes, 0, sendBytes.Length);

            button1.Enabled = false;
            lblDurum.Text = "Bağlantı sağlandı, oyunun başlaması bekleniyor...";

        }

        private void btnA_Click(object sender, EventArgs e)
        {
            NetworkStream networkStream = client.GetStream();

            string serverResponse = "Cevap:A";
            byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
            networkStream.Write(sendBytes, 0, sendBytes.Length);


            foreach (Control btn in this.Controls)
            {
                if (btn is Button)
                {
                    if (btn.Name.Contains("btn"))
                    {
                        btn.Enabled = false;
                    }
                }
            }

        }

        private void btnB_Click(object sender, EventArgs e)
        {
            NetworkStream networkStream = client.GetStream();

            string serverResponse = "Cevap:B";
            byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
            networkStream.Write(sendBytes, 0, sendBytes.Length);

            foreach (Control btn in this.Controls)
            {
                if (btn is Button)
                {
                    if (btn.Name.Contains("btn"))
                    {
                        btn.Enabled = false;
                    }
                }
            }
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            NetworkStream networkStream = client.GetStream();

            string serverResponse = "Cevap:C";
            byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
            networkStream.Write(sendBytes, 0, sendBytes.Length);

            foreach (Control btn in this.Controls)
            {
                if (btn is Button)
                {
                    if (btn.Name.Contains("btn"))
                    {
                        btn.Enabled = false;
                    }
                }
            }
        }

        private void btnD_Click(object sender, EventArgs e)
        {
            NetworkStream networkStream = client.GetStream();

            string serverResponse = "Cevap:D";
            byte[] sendBytes = Encoding.UTF8.GetBytes(serverResponse);
            networkStream.Write(sendBytes, 0, sendBytes.Length);

            foreach (Control btn in this.Controls)
            {
                if (btn is Button)
                {
                    if (btn.Name.Contains("btn"))
                    {
                        btn.Enabled = false;
                    }
                }
            }
        }

        int sabit_sure = 9;
        int sure = 9;
        int start = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (start == 0)
            {
                sure = sabit_sure;
                lblSure.Text = sure.ToString();
                start = 1;
            }
            else
            {
                sure--;
                lblSure.Text = sure.ToString();
                if (sure == 0)
                {
                    timer1.Enabled = false;
                    start = 0;
                }
            }
        }

    }
}
