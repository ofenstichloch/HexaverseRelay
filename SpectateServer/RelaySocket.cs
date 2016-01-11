﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    abstract class RelaySocket
    {
        protected Socket client;
        protected RelayServer server;
        protected bool doListen = true;

        public RelaySocket(Socket s, RelayServer server)
        {
            this.client = s;
            this.server = server;
            //Thread t = new Thread(this.listen);
            //t.Start();
            server.registerClient(this);
        }

        protected abstract void listen();

        public void send(byte[] data)
        {
            client.Send(data);
        }

        public void disconnect()
        {
            server.unregisterClient(this);
            doListen = false;
            client.Close();
        }
    }
}