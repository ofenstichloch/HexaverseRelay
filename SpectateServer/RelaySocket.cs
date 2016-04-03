using System;
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
        protected Thread t;
        public bool isReady = false;

        public RelaySocket(Socket s, RelayServer server)
        {
            this.client = s;
            this.server = server;
            //Thread t = new Thread(this.listen);
            //t.Start();
            server.registerClient(this);
        }

        protected abstract void receive();

        public void send(byte[] data, int length)
        {
            // TODO: check size
            client.Send(data, length,SocketFlags.None);
        }

        public void send(Object o,Protocol.ChannelID channel)
        {

            //Todo move to a static list for each object (performance)
            Protocol.SerialInterface proc = Protocol.SerialInterface.Build(o.GetType());
            Protocol.ByteBuffer buf = new Protocol.ByteBuffer(1);
            proc.SerializePacket((uint) channel, o, buf);
            client.Send(buf.GetArray(),buf.Length,SocketFlags.None);
        }

        public void disconnect()
        {
            this.isReady = false;
            server.unregisterClient(this);
            doListen = false;
            client.Close();
        }
    }
}
