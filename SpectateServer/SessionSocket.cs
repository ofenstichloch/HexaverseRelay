using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Protocol;

// TODO: Add map for SerialItem for every needed serializer with type

namespace SpectateServer
{
    
    class SessionSocket : RelaySocket
    {
        new RelaySessionServer server;
        public SessionSocket(Socket socket, RelaySessionServer server)
            : base(socket, server)
        {
            this.t = new Thread(this.receive);
            this.t.Start();
            this.server = server;
        }

        protected override void receive()
        {
            int channel = 0;
            int size = 0;
            byte[] header = new byte[8];
            byte[] payload;
            byte[] data;

            while (doListen)
            {

                try
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
                       
                        if (channel == (int) ChannelID.Hello)
                        {
                            processHello(payload);
                        }
                        else if(channel == (int) ChannelID.ClientFaction)
                        {
                            processClientFactionRequest(payload);
                        }
                        else
                        {
                            Log.notify("Received messsage on channel " + ((ChannelID)channel).ToString(),this);
                        }

                    }
                    else if (channel == (int) ChannelID.Ping)
                    {
                        send(Signals.pong, 8);
                    }
                }
                catch (SocketException e)
                {
                    Log.notify("Client disconnected.", this);
                    Log.notify(e.Message, this);
                    this.disconnect();
                }
            }
        }

        private void processClientFactionRequest(byte[] payload)
        {
            ClientFactionResponse r = new ClientFactionResponse();
            r.factionToken = new CryptographicID();
            r.info = server.getServerInfo().gameInfo;
            r.planetConfig = server.getPlanetConfig();
            r.startFactionTypes = 0x2;
            send(r, ChannelID.ClientFaction);
            Log.notify("Requesting everything from Gameserver", this);
            server.requestEverything();
            this.isReady = true;
        }

        private void processHello(byte[] name)
        {
            //reply to hello
            Result r = new Result();
            r.success = true;
            r.message = name;
            send(r, ChannelID.Hello);
            //send initial phasechange
            byte[] phase = server.getPhase();
            send(phase, phase.Length);

        }



    }
}
