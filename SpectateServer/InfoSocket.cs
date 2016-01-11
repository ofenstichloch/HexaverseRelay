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
        }

        protected override void listen()
        {
            int channel = 0;
            int size = 0;
            byte[] header = new byte[8];
            byte[] payload;
            byte[] data;
            while (doListen)
            {
                 
                 client.Receive(header, 8, SocketFlags.None);
                 channel = BitConverter.ToInt32(header, 0);
                 size = BitConverter.ToInt32(header, 4);
                 if (size > 0)
                 {
                     payload = new byte[size];
                     client.Receive(payload, size, SocketFlags.None);
                     data = new byte[8 + size];
                     BitConverter.GetBytes(channel).CopyTo(data, 0);
                     BitConverter.GetBytes(size).CopyTo(data, 4);
                     payload.CopyTo(data, 8);
                     if (channel == 1)
                     {
                         client.Send(data);
                     }
                     else
                     {
                         server.redirect(data);
                     }
                     
                 }
            }
           


        }
    }
}
