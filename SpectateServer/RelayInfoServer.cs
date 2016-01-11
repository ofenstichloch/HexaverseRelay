﻿using System;
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
        RelayInfoClient infoClient;

        public RelayInfoServer(string name, int port, RelayInfoClient infoClient)
            : base(name, port)
        {
            this.infoClient = infoClient;
        }

        public override bool connect()
        {
            try
            {
                Log.notify("Starting up", this);
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

        protected override void acceptSockets()
        {
            Log.notify("Listening", this);

            while (doListen)
            {
                Socket client = tcpServer.AcceptSocket();
                InfoSocket c = new InfoSocket(client, this);
            }
            
        }

        public override void redirect(byte[] data){
            infoClient.send(data);
        }

    }
}