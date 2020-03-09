using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuestionApp_DataComProject
{
    public class Client_Info
    {
        TcpClient tcpClient;
        string clientName;

        public TcpClient GetTcpClient()
        {
            return tcpClient;
        }

        public string GetClientName()
        {
            return clientName;
        }

        public Client_Info(string cname, TcpClient client)
        {
            tcpClient = client;
            clientName = cname;
        }
    }
}
