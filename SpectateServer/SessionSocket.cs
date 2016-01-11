using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    class SessionSocket : RelaySocket
    {
        public SessionSocket(Socket socket, RelayServer server)
            : base(socket, server)
        {

        }

        protected override void listen()
        {

        }

    }
}
