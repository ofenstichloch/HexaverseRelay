using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    class RelayInfoClient : GameClient
    {

        Protocol.ByteBuffer buf = new Protocol.ByteBuffer(112);
        Protocol.SerialInterface proc = Protocol.SerialInterface.Build(typeof(Protocol.ServerInfo));

        public RelayInfoClient(string name, string h, int p, Host host)
            : base(name,h,p, host)
        {

        }

        protected override void listen()
        {
            bool readPayload = false;
            int channel = 0;
            int size = 0;
            byte[] payload;
            int received = 0;
            byte[] headerBuffer = new byte[8];

            while (doListen || connected)
            {
                try
                {
                    received = 0;
                    //Wait for 8 bytes
                    if (!readPayload)
                    {
                        while (received < 8)
                        {
                            clientStream.Read(headerBuffer, received, 8 - received);
                        }
                        channel = BitConverter.ToInt32(headerBuffer, 0);
                        size = BitConverter.ToInt32(headerBuffer, 4);
                        readPayload = true;

                    }
                    if (readPayload)
                    {
                        payload = new byte[size];
                        received = 0;
                        while(received < size)
                        {
                            clientStream.Read(payload, received, size - received);
                        }
                        byte[] data = new byte[8 + size];
                        BitConverter.GetBytes(channel).CopyTo(data, 0);
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                        payload.CopyTo(data, 8);
                        buf.Clear();
                        Object o = proc.Deserialize(payload,size);
                        Protocol.ServerInfo serverInfo = (Protocol.ServerInfo) o;
                        serverInfo.gameInfo.gameFlags &= ~((uint) Protocol.gameFlags.SupportRegularFactions);
                        serverInfo.gameInfo.gameFlags |= (uint)Protocol.gameFlags.SupportSpectatorFactions;//useless?
                        serverInfo.hostInfo.hostID = host.hostID;
                        serverInfo.hostInfo.serverBasePort = (ushort) host.serverPort;
                        host.serverInfo = serverInfo;
                        proc.SerializePacket(0, serverInfo, buf);
                        server.sendToClients(buf.GetArray(), buf.Length);
                        readPayload = false;
                    }
                    else if (readPayload && size == 0)
                    {
                        //process a signal?
                        readPayload = false;
                    }
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.error(e.Message, this);
                    host.disconnect();
                }


            }
        }

        public override bool connect()
        {
            try
            {
                tcpClient.Connect(ServerAddress, ServerPort);
                clientStream = tcpClient.GetStream();
                connected = true;
                Thread t = new Thread(this.listen);
                this.setThread(t);
                t.Start();
                Log.notify("Connected", this);
                
                //Thread t1 = new Thread(this.maintainConnection);
                //t1.Start();
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.Message, this);
                host.disconnect();
                return false;
            }
           
  
        }

        public override void disconnect()
        {
            Log.notify("Stopping", this);
            doListen = false;
        }

        public void send(byte[] data)
        {
            clientStream.Write(data, 0, data.Length);
        }

    }

}
