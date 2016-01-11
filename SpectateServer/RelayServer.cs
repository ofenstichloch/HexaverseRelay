using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    abstract class RelayServer : RelayComponent
    {
        protected TcpListener tcpServer;
        protected List<RelaySocket> clients;
        protected int port;
       public RelayServer(string name, int port) : base(name)
        {
            this.name = name;
            this.port = port;
            tcpServer = new TcpListener(System.Net.IPAddress.Any, port);
            clients = new List<RelaySocket>();
        }

        public void registerClient(RelaySocket c)
        {
            clients.Add(c);
        }

        public void unregisterClient(RelaySocket c)
        {
            clients.Remove(c);
        }

        public abstract bool connect();

        public abstract void disconnect();

        protected abstract void acceptSockets();


        //TODO  Sends fom server thread, maybe change to sender Thread(pool)?
        public void sendToClients(byte[] data)
        {
            foreach (RelaySocket c in clients)
            {
                c.send(data);
            }
        }

        public abstract void redirect(byte[] data);
    }
}
