using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    class InfoSocket : RelaySocket
    {
        public InfoSocket(Socket socket, RelayInfoServer server)
            : base(socket, server)
        {
            this.t = new Thread(this.receive);
            t.Start();
            this.isReady = true;

        }

        protected override void receive()
        {
            int channel = 0;
            int size = 0;
            byte[] header = new byte[8];
            byte[] payload;
            byte[] data;
            int received = 0;

            while (doListen)
            {
                try
                {
                    received = 0;
                    while (received < 8)
                    {
                        received += client.Receive(header, received, 8 - received, SocketFlags.None);
                    }
                    channel = BitConverter.ToInt32(header, 0);
                    size = BitConverter.ToInt32(header, 4);
                    Log.notify("Received " + 8 + size + " byte on channel " + channel, this);
                    if (size > 0)
                    {
                        payload = new byte[size];
                        received = 0;
                        while (received < size)
                        {
                            received += client.Receive(payload,received, size-received, SocketFlags.None);
                        }
                        
                        data = new byte[8 + size];
                        BitConverter.GetBytes(channel).CopyTo(data, 0);
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                        payload.CopyTo(data, 8);
                        if (channel == 1)
                        {
                            send(data, 8+size);
                        }
                    }
                }
                catch (SocketException e)
                {
                    Log.error(e.Message, this);
                    Log.notify("Client disconnected.", this);
                    this.disconnect();
                }
            }



        }
    }
}
