using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace QuestionApp_DataComProject
{
    public partial class Form1 : Form
    {
        RandomDBEntities rde;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            rde = new RandomDBEntities();
        }

        public int clientcount = 0;
        List<Client_Info> clients = new List<Client_Info>();
        string dogru_cevap = "";
        int lock_soru = 0;
        List<Client_Info> wrong_answer_clients = new List<Client_Info>();
        List<Client_Info> true_answer_clients = new List<Client_Info>();

        public void AcceptClient(TcpClient client)
        {
            clientcount++;
            string clientname = "";
            Client_Info clinfo = null;
            NetworkStream netstream = client.GetStream();
            bool connected = true;
            while (connected)
            {
                Thread.Sleep(10);
                try
                {
                    byte[] bytes = new byte[client.ReceiveBufferSize];

                    //read the identifier from client
                    netstream.Read(bytes, 0, bytes.Length);

                    string readed = Encoding.UTF8.GetString(bytes);
                    readed = readed.Substring(0, readed.IndexOf("\0"));


                    if (readed.Contains("Name:"))
                    {
                        clientname = readed.Split(':')[1];
                        richTextBox1.AppendText("Bağlanan oyuncu : " + clientname + Environment.NewLine);
                        clinfo = new Client_Info(clientname, client);
                        clients.Add(clinfo);
                    }
                    else if (readed.Contains("Cevap:") && lock_soru == 0)
                    {
                        int wrong_answer_before = 0;
                        foreach (Client_Info ci in wrong_answer_clients)
                        {
                            if (ci.GetClientName() == clinfo.GetClientName())
                            {
                                wrong_answer_before = 1;
                            }
                        }

                        if (wrong_answer_before == 0)
                        {
                            string users_answer = readed.Split(':')[1];
                            if (users_answer != dogru_cevap)
                            {
                                wrong_answer_clients.Add(clinfo);
                                richTextBox1.AppendText("Yanlış cevap veren oyuncu : " + clientname + " - Yanlış şık : " + users_answer + "\r\n");
                            }
                            else
                            {
                                true_answer_clients.Add(clinfo);
                                richTextBox1.AppendText("Doğru cevap veren oyuncu : " + clientname + " - Doğru şık : " + users_answer +  "\r\n");
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    connected = false;
                    clientcount--;
                    clients.Remove(clinfo);
                    //richTextBox1.AppendText(e.ToString());
                    //richTextBox1.AppendText(e.StackTrace.ToString());
                }
            }

        }

        public void Listener()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
            TcpClient client;
            listener.Start();

            while (true) // Add your exit flag here
            {
                client = listener.AcceptTcpClient();

                Thread client_creator = new Thread(() => AcceptClient(client));
                client_creator.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread mainthread = new Thread(() => Listener());
            mainthread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock_soru = 0;
            Random rnd = new Random();
            int random_soru_id = rnd.Next(1, rde.tblQuestions.Count());
            tblQuestion question_row = rde.tblQuestions.Where(t => t.Id == random_soru_id).FirstOrDefault();

            dogru_cevap = question_row.Cevap;

            foreach (Client_Info cl in clients)
            {
                question_row = rde.tblQuestions.Where(t => t.Id == random_soru_id).FirstOrDefault();

                if (question_row != null)
                {
                    NetworkStream netstream = cl.GetTcpClient().GetStream();
                    string data = "|" + question_row.Soru + "-" + question_row.A + "-" + question_row.B + "-" + question_row.C + "-" + question_row.D + "|";
                    byte[] send_data = Encoding.UTF8.GetBytes(data);

                    netstream.Write(send_data, 0, send_data.Length);
                }
                else
                {
                    richTextBox1.AppendText("Oyunda anlaşılamayan bir hata var.\n");
                }
            }
            Thread thTimer = new Thread(() => timerthread());
            thTimer.Start();
        }

        public void ClientCountEditor()
        {
            while (true)
            {
                Thread.Sleep(1000);
                this.Invoke((MethodInvoker)(() => { lblConnectionCount.Text = clients.Count.ToString(); }));
                

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread counterth = new Thread(() => ClientCountEditor());
            counterth.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tblQuestion question_row = rde.tblQuestions.Where(t => t.Id == 1).FirstOrDefault();

            MessageBox.Show(question_row.Soru);
        }

        public void timerthread()
        {
            Thread.Sleep(10000);

            lock_soru = 1;

            List<Client_Info> silincekler = new List<Client_Info>();

            foreach (Client_Info ci in clients)
            {
                int can_found_in_list = 0;
                foreach (Client_Info cit in true_answer_clients)
                {
                    if (ci == cit)
                    {
                        can_found_in_list = 1;
                    }
                }

                if (can_found_in_list == 0)
                {
                    silincekler.Add(ci);
                   // ci.GetTcpClient().Close();
                  // clients.Remove(ci);
                }
            }

            for (int i = 0; i < silincekler.Count; i++)
            {
                NetworkStream netstream = silincekler[i].GetTcpClient().GetStream();
                string data = "!ELENDİNİZ!";
                byte[] send_data = Encoding.UTF8.GetBytes(data);
                netstream.Write(send_data, 0, send_data.Length);

                silincekler[i].GetTcpClient().Close();
                clients.Remove(silincekler[i]);
            }

            if (true_answer_clients.Count == 1)
            {
                NetworkStream netstream = true_answer_clients[0].GetTcpClient().GetStream();
                string data = "!KAZANDIN!";
                byte[] send_data = Encoding.UTF8.GetBytes(data);
                netstream.Write(send_data, 0, send_data.Length);
                richTextBox1.AppendText("Oyun bitti kazanan : " + true_answer_clients[0].GetClientName() + "\r\n");
                return;
            }

            foreach(Client_Info ci in true_answer_clients)
            {
                NetworkStream netstream = ci.GetTcpClient().GetStream();
                string data = "!DOGRUCEVAP!";
                byte[] send_data = Encoding.UTF8.GetBytes(data);
                netstream.Write(send_data, 0, send_data.Length);
            }

           

            wrong_answer_clients.Clear();
            true_answer_clients.Clear();

        }
    }
}
