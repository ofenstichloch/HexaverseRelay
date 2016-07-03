using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SpectateServer
{
    class RelayInfoServer: RelayServer
    {
        bool doListen = false;

        public RelayInfoServer(string name, int port, Host h)
            : base(name, port, h)
        {
        }



        public override void disconnect()
        {
            doListen = false;
        }

        protected override void acceptSockets()
        {
            Log.notify("Listening", this);

            while (doListen)
            {
                Socket client = tcpServer.AcceptSocket();
                InfoSocket c = new InfoSocket(client, this);
            }
            
        }

    }
}
