using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace SpectateServer
{
	public class Relay
	{
		private string HOST;
		private int PORT;
		private Thread thread;
        private TcpClient tcpClient;
        private bool connected = false;
        private bool doListen = true;
        private string name;
        private NetworkStream stream;

        public static Relay createNewRelay(string name, String h, int p)
        {
            Relay r = new Relay(name,h,p);
            return r;
        }

        public Relay(string name, string h, int p)
		{
            this.name = name;
            HOST = h;
            PORT = p;
            tcpClient = new TcpClient();
            Thread t = new Thread(this.listen);
            this.setThread(t);
            t.Start();
            Log.notify("Not connected, trying to connect", this);
            connected = loginToSever();
            DateTime lastPing = DateTime.Now;
            while (connected)
            {
                if (DateTime.Now.Subtract(lastPing).TotalSeconds > 2)
                {
                    lastPing = DateTime.Now;
                    maintainConnection();
                }
            }
		}

		private void listen(){
            bool readPayload = false;
            int channel;
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
                        stream.Read(headerBuffer, 0, 8);
                        channel = BitConverter.ToInt32(headerBuffer,0);
                        size = BitConverter.ToInt32(headerBuffer,4);
                        Log.notify("Received " + size + "B on channel " + channel, this);
                        readPayload = true;
                    }
                    if (readPayload)
                    {
                        payload = new byte[size];
                        stream.Read(payload, 0, size);
                        //TODO Redirect to output stream
                        //TODO Redirec to analytics
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
                stream = tcpClient.GetStream();
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
                stream.Write(hello,0,hello.Length);
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
            byte[] ping = { 1, 0, 0, 0, 0, 0, 0, 0 };
            stream.Write(ping, 0, 8);
        }

        public override string ToString()
        {
            return this.name;
        }

        #region Get/Set
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

