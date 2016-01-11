using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    class RelaySessionServer : RelayServer
    {

        private bool doListen = false;

        public RelaySessionServer(string name, int port)
            : base(name, port)
        {

        }

        protected override void acceptSockets()
        {
            Log.notify("Listening", this);

            while (doListen)
            {
                Socket client = tcpServer.AcceptSocket();
                SessionSocket c = new SessionSocket(client, this);
            }
            
        }

        public override bool connect()
        {
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
                return false;
            }
        }

        public override void disconnect()
        {
            doListen = false;
        }

        public override void redirect(byte[] data)
        {
            
        }

    }
}
