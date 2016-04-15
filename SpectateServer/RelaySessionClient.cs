using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Protocol;

namespace SpectateServer
{
    class RelaySessionClient : GameClient
    {
        bool initializing = true;
        MessageBuffer buffer;
        public RelaySessionClient(string name, string h, int p, Host host, MessageBuffer buffer) : base(name,h,p, host)
		{
            this.buffer = buffer;
		}

		protected override void listen(){
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
                    //Wait for 8 bytes
                    if (!readPayload)
                    {
                        received = 0;
                        while (received < 8)
                        {
                            received += clientStream.Read(headerBuffer, received, 8 - received);
                        }
                        channel = BitConverter.ToInt32(headerBuffer, 0);
                        size = BitConverter.ToInt32(headerBuffer, 4);
                        readPayload = true;
                    }
                    else if (readPayload)
                    {
                        received = 0;
                        payload = new byte[size];
                        while (received < size)
                        {
                            received += clientStream.Read(payload, received, size - received);
                        }
                        byte[] data = new byte[8 + size];
                        BitConverter.GetBytes(channel).CopyTo(data, 0);
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                        payload.CopyTo(data, 8);
                        if (initializing)
                        {
                            if (channel == (int)ChannelID.Hello)
                            {
                                SerialInterface proc = SerialInterface.Build(typeof(Result));
                                Result r = (Result)proc.Deserialize(payload, size);
                                if (r.success == false)
                                {
                                    disconnect();
                                    return;
                                }
                                joinAsSpectator();
                            }
                            else if (channel == (int)ChannelID.ClientFaction)
                            {
                                SerialInterface proc = SerialInterface.Build(typeof(ClientFactionResponse));
                                ClientFactionResponse r = (ClientFactionResponse)proc.Deserialize(payload, size);
                                host.planetConfig = r.planetConfig;
                                initializing = false;
                            }
                        }
<<<<<<< HEAD
                        else {
                            if (channel == (int)ChannelID.PhaseChange) host.phase = data;
                            if (channel != 2)
                            {
                                buffer.add(data);
                                server.sendToClients(data, data.Length);
                                //TODO Redirect to analytics
                            }
=======
                        else if (channel != 2)
                        {
                            
                            server.sendToClients(data, data.Length);
                            //TODO Redirect to analytics
>>>>>>> master
                        }
                        readPayload = false;
                    }

                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Log.error("Error while reading data from client.",this);
                    Log.error(e.Message, this);
                    this.disconnect();
                }


            }
		}

		private bool loginToSever(){
            try
            {
                tcpClient.Connect(ServerAddress,ServerPort);
                clientStream = tcpClient.GetStream();

                Protocol.SerialInterface proc = Protocol.SerialInterface.Build(typeof(String));
                String name = "SpecRelay";
                Protocol.ByteBuffer buf = new Protocol.ByteBuffer(name.Length);
                proc.SerializePacket((int) Protocol.ChannelID.Hello, name, buf);
                clientStream.Write(buf.GetArray(), 0, buf.Length);
                return true;
            }
            catch (Exception e)
            {
                Log.error("Error while logging in.", this);
                Log.error(e.Message, this);
            }
            disconnect();
			return false;
		}

        private bool joinAsSpectator()
        {
            try
            {
                Protocol.SerialInterface proc = Protocol.SerialInterface.Build(typeof(ClientFactionRequest));
                ClientFactionRequest req = new ClientFactionRequest();
                req.startFactionTypes = 2;
                Protocol.ByteBuffer buf = new Protocol.ByteBuffer(288);
                proc.SerializePacket((int)ChannelID.ClientFaction, req, buf);
                clientStream.Write(buf.GetArray(), 0, buf.Length);


                return true;
            }
            catch (Exception e)
            {
                Log.error("Error while joining.",this);
                Log.error(e.Message, this);
            }
            disconnect();
            return false;
        }

        private void maintainConnection()
        {
            while (connected)
            {
                clientStream.Write(Signals.Ping, 0, 8);
                Thread.Sleep(1000);
            }
            
        }

        public override bool connect()
        {
            connected = loginToSever();
            if (connected)
            {
                Thread t = new Thread(this.listen);
                this.setThread(t);
                t.Start();
                Thread t1 = new Thread(this.maintainConnection);
                t1.Start();
                Log.notify("Connected", this);
                return true;
            }
            
            disconnect();
            return false;
        }

        public override void disconnect()
        {
            Log.notify("Disconnecting...", this);
            doListen = false;
            connected = false;
            tcpClient.Close();
        }

        public void requestEverything()
        {
            Log.notify("Requesting Everything..", this);
            clientStream.Write(Signals.RequestEverything, 0, 8);
        }

    }
}
