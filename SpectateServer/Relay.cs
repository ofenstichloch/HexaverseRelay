using System;
using System.Threading;
using System.Net.Sockets;

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
            Thread t = new Thread(r.listen);
            r.setThread(t);
            t.Start();
            return r;
        }

        public Relay(string name, string h, int p)
		{
            this.name = name;
            HOST = h;
            PORT = p;
            tcpClient = new TcpClient(h, p);
		}

		private void listen(){
            while (doListen)
            {
                if (!connected)
                {
                    Log.notify("Not connected, trying to connect", this);
                    connected = loginToSever();
                    Thread.Sleep(2000);
                }
                else
                {
                    try
                    {
                        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
                        int read = stream.Read(buffer, 0, tcpClient.ReceiveBufferSize);
                        Log.notify("Received " + read + " byte", this);

                    }
                    catch (Exception e)
                    {
                        Log.error(e.Message, this);
                    }
                }

            }
		}

		private bool loginToSever(){
            try
            {
                tcpClient.Connect(HOST,PORT);
                stream = tcpClient.GetStream();
                byte[] hello = new byte[]
            }
            catch (Exception e)
            {
            }
			return false;
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

