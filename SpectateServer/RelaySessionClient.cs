using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
    class RelaySessionClient : GameClient
    {
        public RelaySessionClient(string name, string h, int p, Host host) : base(name,h,p, host)
		{
		}

		protected override void listen(){
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
                        channel = BitConverter.ToInt32(headerBuffer,0);
                        size = BitConverter.ToInt32(headerBuffer,4);
                        //Log.notify("Received " + size + "B on channel " + channel, this);
                        readPayload = true;
 
                    }
                    if (readPayload)
                    {
                        if (channel != 2 && channel != 8)
                        {
                            payload = new byte[size];
                            clientStream.Read(payload, 0, size);
                            byte[] data = new byte[8 + size];
                            BitConverter.GetBytes(channel).CopyTo(data, 0);
                            BitConverter.GetBytes(size).CopyTo(data, 4);
                            payload.CopyTo(data, 8);
                            server.sendToClients(data, data.Length);
                            //TODO Redirect to analytics
                        }
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

		private bool loginToSever(){
            try
            {
                tcpClient.Connect(ServerAddress,ServerPort);
                clientStream = tcpClient.GetStream();

                Protocol.SerialInterface proc = Protocol.SerialInterface.Build(typeof(String));
                String name = "SpecRelay";
                Protocol.ByteBuffer buf = new Protocol.ByteBuffer(name.Length);
                proc.SerializePacket(7, name, buf);
                clientStream.Write(buf.GetArray(), 0, buf.Length);
              
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.Message, this);
            }
            disconnect();
			return false;
		}

        private void maintainConnection()
        {
            while (connected)
            {
                Thread.Sleep(1000);
                byte[] ping = { 1, 0, 0, 0, 0, 0, 0, 0 };
                clientStream.Write(ping, 0, 8);
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
                Log.notify("Connected.", this);
                return true;
            }
            
            disconnect();
            return false;
        }

        public override void disconnect()
        {
            doListen = false;
            connected = false;
            tcpClient.Close();
        }

    }
}
