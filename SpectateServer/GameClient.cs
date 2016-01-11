using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    abstract class GameClient : RelayComponent
    {

        //Client
        protected string HOST;
        protected int PORT;
        protected Thread thread;
        protected TcpClient tcpClient;
        protected bool connected = false;
        protected bool doListen = true;
        protected NetworkStream clientStream;
        protected RelayServer server;

        public GameClient(string name, string h, int p) : base(name)
		{
            HOST = h;
            PORT = p;
            tcpClient = new TcpClient();
		}

        protected abstract void listen();
        public abstract bool connect();
        public abstract void disconnect();

        public void setServer(RelayServer s)
        {
            this.server = s;
        }

        public void setThread(Thread t)
        {
            this.thread = t;
        }

        public Thread getThread()
        {
            return this.thread;
        }

    }
}
