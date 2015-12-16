using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SpectateServer
{
    class RelayControlServer
    {
        public string name;
        private UdpClient server;
        private int port;

        public RelayControlServer(string name,int port)
        {
            this.name = name;
            this.port = port;
            server = new UdpClient(port);
            server.BeginReceive(new AsyncCallback(recv), null);
        }
        
        private void recv(IAsyncResult res)
        {
            Log.notify("Received on control port", this);
            IPEndPoint e = new IPEndPoint(IPAddress.Any, port);
            byte[] data = server.EndReceive(res, ref e);
            server.BeginReceive(new AsyncCallback(recv), null);
        }
    }
}
