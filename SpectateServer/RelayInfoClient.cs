﻿using System;
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

            byte[] headerBuffer = new byte[8];

            while (doListen || connected)
            {
                try
                {
                    //Wait for 8 bytes
                    if (tcpClient.ReceiveBufferSize >= 8 && !readPayload)
                    {
                        //Get channel and size
                        clientStream.Read(headerBuffer, 0, 8);
                        channel = BitConverter.ToInt32(headerBuffer, 0);
                        size = BitConverter.ToInt32(headerBuffer, 4);
                        //Log.notify("Received " + size + "B on channel " + channel, this);
                        readPayload = true;

                    }
                    if (readPayload && tcpClient.ReceiveBufferSize >=size)
                    {
                        payload = new byte[size];
                        clientStream.Read(payload, 0, size);
                        byte[] data = new byte[8 + size];
                        BitConverter.GetBytes(channel).CopyTo(data, 0);
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                        payload.CopyTo(data, 8);
                        buf.Clear();
                        Object o = proc.Deserialize(payload,size);
                        Protocol.ServerInfo serverInfo = (Protocol.ServerInfo) o;
                        serverInfo.hostInfo.hostID = host.hostID;
                        serverInfo.hostInfo.serverBasePort = (ushort) host.serverPort;
                        host.serverInfo = serverInfo;
                        proc.SerializePacket(0, serverInfo, buf);
                        data = buf.GetArray();

                        server.sendToClients(buf.GetArray(), buf.Length);
                        //Save serverinfo
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
                Log.notify("connected", this);
                
                //Thread t1 = new Thread(this.maintainConnection);
                //t1.Start();
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.Message, this);
                disconnect();
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
