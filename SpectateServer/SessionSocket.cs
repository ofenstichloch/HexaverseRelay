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
        public SessionSocket(Socket socket, RelayServer server)
            : base(socket, server)
        {
            this.t = new Thread(this.receive);
            this.t.Start();
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
                       
                        if (channel == 7)
                        {
                            processHello(payload);
                        }
                        else
                        {
                            Log.notify("Received messsage on channel " + ((ChannelID)channel).ToString(),this);
                        }

                    }
                    else if (channel == 1)
                    {
                        data = new byte[8];
                        BitConverter.GetBytes(2).CopyTo(data,0);
                        send(data, 8);
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

        private void processHello(byte[] name)
        {
            //reply to hello
            Result r = new Result();
            r.success = true;
            r.message = name;
            send(r, ChannelID.Hello);
            Log.notify("Sent Result", this);
            //send initial phasechange
            byte[] phase = server.getPhase();
            send(phase, phase.Length);
            Log.notify("Sent Phase", this);
            this.isReady = true;
        }



    }
}
