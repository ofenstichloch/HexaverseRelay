using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    public class RelayServer
    {
        private string name;
        private int port;
        private TcpListener tcpServer;
        private bool doListen = false;
        private List<Client> clients;

        public RelayServer(string name, int port)
        {
            tcpServer = new TcpListener(System.Net.IPAddress.Any, port);
            clients = new List<Client>();
        }

        private void listen()
        {
            Log.notify(name + " listing.", this);
            while (doListen)
            {
                Socket client = tcpServer.AcceptSocket();
                Client c = new Client(client, this);
            }
            
        }

        public void sendToClients(byte[] data)
        {
            foreach (Client c in clients)
            {
                c.send(data);
            }
        }

        public void registerClient(Client c)
        {
            clients.Add(c);
        }

        public void unregisterClient(Client c)
        {
            clients.Remove(c);
        }

        public void connect()
        {
            doListen = true;
            Thread t = new Thread(this.listen);
            t.Start();
            tcpServer.Start();
        }

        public void disconnect()
        {
            doListen = false;
        }
    }
}
