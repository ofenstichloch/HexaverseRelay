using System;

namespace SpectateServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Log.setLevel(2);
            int port = 10247;
            string host = "127.0.0.1";
            int listen = 10240;
            if (args.Length == 3)
            {
                port = int.Parse(args[0]);
                host = args[1];
                listen = int.Parse(args[2]);
            }

            Host h = new Host();
            h.startHost(port, host, listen);

		}
	}
}
