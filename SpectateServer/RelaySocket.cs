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
            int size = BitConverter.ToInt32(data, 4)+8;
            int channel = BitConverter.ToInt32(data, 0);
            if (length != size || size > data.Length)
            {
                Log.error("Trying to send an invalid amount of data ("+size+" "+length+ " "+data.Length+")", this);
                disconnect();
                return;
            }
            client.Send(data, length,SocketFlags.None);
            Analytics.Statistics.addSentBytes(length);
        }

        public void send(Object o,Protocol.ChannelID channel)
        {

            //Todo move to a static list for each object (performance)
            Protocol.SerialInterface proc = Protocol.SerialInterface.Build(o.GetType());
            Protocol.ByteBuffer buf = new Protocol.ByteBuffer(1);
            proc.SerializePacket((uint) channel, o, buf);
            client.Send(buf.GetArray(),buf.Length,SocketFlags.None);
            Log.notify("Sent packet on channel " + ((Protocol.ChannelID)channel).ToString(),this);
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
