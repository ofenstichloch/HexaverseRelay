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
        private MessageBuffer buffer;

        public RelaySessionServer(string name, int port, Host h, MessageBuffer buffer)
            : base(name, port, h)
        {
            this.buffer = buffer;
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
                Log.error(e.Message, this);
                return false;
            }
        }

        public override void disconnect()
        {
            doListen = false;
        }

        public void requestEverything()
        {
            host.sessionClient.requestEverything();
        }

        public byte[][] getBuffer()
        {
            return buffer.getData();
        }

    }
}
