using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace SpectateServer
{
	public class RelayClient
	{
        private string name;
        //Client
		private string HOST;
		private int PORT;
		private Thread thread;
        private TcpClient tcpClient;
        private bool connected = false;
        private bool doListen = true;
        private NetworkStream clientStream;
        private RelayServer server;

        public RelayClient(string name, string h, int p)
		{
            this.name = name;
            //start client
            HOST = h;
            PORT = p;
            tcpClient = new TcpClient();
		}

		private void listen(){
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
                        Log.notify("Received " + size + "B on channel " + channel, this);
                        readPayload = true;
                    }
                    if (readPayload)
                    {
                        payload = new byte[size];
                        clientStream.Read(payload, 0, size);
                        byte[] data = new byte[8 + size];
                        BitConverter.GetBytes(channel).CopyTo(data, 0);
                        BitConverter.GetBytes(size).CopyTo(data, 4);
                        payload.CopyTo(data, 8);
                        server.sendToClients(data);
                        //TODO Redirect to analytics
                        readPayload = false;
                    }
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
                Log.notify("Connecting..", this);
                tcpClient.Connect(HOST,PORT);
                clientStream = tcpClient.GetStream();
                byte[] hello = new byte[17];
                hello[0] = 7;
                hello[1] = 0;
                hello[2] = 0;
                hello[3] = 0;
                hello[4] = 9;
                hello[5] = 0;
                hello[6] = 0;
                hello[7] = 0;
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] name = enc.GetBytes("SpecRelay");
                name.CopyTo(hello, 8);
                clientStream.Write(hello,0,hello.Length);
                return true;
            }
            catch (Exception e)
            {
                Log.error(e.Message, this);
            }
			return false;
		}

        private void maintainConnection()
        {
            DateTime lastPing = DateTime.Now;
            while (connected)
            {
                if (DateTime.Now.Subtract(lastPing).TotalSeconds > 2)
                {
                    lastPing = DateTime.Now;
                    byte[] ping = { 1, 0, 0, 0, 0, 0, 0, 0 };
                    clientStream.Write(ping, 0, 8);
                }
            }
            
        }

        public void connect()
        {
            Thread t = new Thread(this.listen);
            this.setThread(t);
            t.Start();
            connected = loginToSever();
            if (connected)
            {
                Log.notify("Connected.", this);
                Thread t1 = new Thread(this.maintainConnection);
                t1.Start();
            }
        }

        public void disconnect()
        {
            doListen = false;
            connected = false;
            tcpClient.Close();
        }

        public override string ToString()
        {
            return this.name;
        }

        #region Get/Set
        public void setServer(RelayServer s)
        {
            this.server = s;
        }

        public void setThread(Thread t)
        {
            this.thread = t;
        }

        public Thread getThread()
        {
            return this.thread;
        }

        public String getName() { return this.name; }
        #endregion
    }
}

