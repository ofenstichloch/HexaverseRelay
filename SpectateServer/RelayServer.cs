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
        protected Host host;
       public RelayServer(string name, int port, Host host) : base(name)
        {
            this.name = name;
            this.port = port;
            this.host = host;
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
        public void sendToClients(byte[] data, int length)
        {
            foreach (RelaySocket c in clients)
            {
                if (c.isReady)
                {
                    c.send(data, length);
                }
                
            }
        }

        public byte[] getPhase()
        {
            return host.phase;
        }
        public Protocol.ServerInfo getServerInfo()
        {
            return host.serverInfo;
        }

        public Protocol.PlanetConfig getPlanetConfig()
        {
            return host.planetConfig;
        }
    }
}
