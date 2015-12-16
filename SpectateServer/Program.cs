using System;

namespace SpectateServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Log.setLevel(2);
            RelayServer rs = new RelayServer("RServer1", 20244);
            RelayControlServer rcs = new RelayControlServer("RControl1", 10242);
            RelayClient rc = new RelayClient("RClient1", "localhost", 10244);
            rc.setServer(rs);
            rs.connect();
            rc.connect();
            Console.ReadLine();
            rc.disconnect();
            rs.disconnect();

		}
	}
}
