using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    public class Client
    {
        Socket client;
        RelayServer server;
        bool doListen = true;

        public Client(Socket s, RelayServer server)
        {
            this.client = s;
            Thread t = new Thread(this.listen);
            t.Start();
            server.registerClient(this);
        }

        public void send(byte[] data)
        {
            client.Send(data);
        }

        public void listen()
        {
            while (doListen)
            {
                //TODO reply ping with pong 
            }
            //unregister this client
            server.unregisterClient(this);
        }

        public void disconnect()
        {
            server.unregisterClient(this);
            doListen = false;
            client.Close();
        }
    }
}
