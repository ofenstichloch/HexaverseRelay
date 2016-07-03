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

        public override void disconnect()
        {
            doListen = false;
            RelaySocket[] list = clients.ToArray<RelaySocket>();
            foreach (RelaySocket c in list)
            {
                c.disconnect();
            }
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
