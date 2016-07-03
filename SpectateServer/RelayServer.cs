﻿using System;
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
        protected bool doListen = false;
        public RelayServer(string name, int port, Host host) : base(name)
        {
            this.name = name;
            this.port = port;
            this.host = host;
            if (port != 0)
            {
                tcpServer = new TcpListener(System.Net.IPAddress.Any, port);
            }
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

        public bool connect()
        {
            if(port == 0)
            {
                doListen = false;
                return true;
            }
            try
            {
                doListen = true;
                Thread t = new Thread(this.acceptSockets);
                t.Start();
                tcpServer.Start();
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.Message, this);
                host.disconnect();
                return false;
            }
        }

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

        public void sendToClients(byte[][] data, int length)
        {
            foreach (byte[] packet in data)
            {
                sendToClients(packet, packet.Length);
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
