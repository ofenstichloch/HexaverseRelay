using System;

namespace SpectateServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Log.setLevel(2);
            RelaySessionClient sessionclient = new RelaySessionClient("SessionClient1", "192.168.56.101", 10244);
            RelayInfoClient infoclient = new RelayInfoClient("InfoClient1", "192.168.56.101", 10245);
            RelaySessionServer sessionserver = new RelaySessionServer("SessionServer1", 20244);
            RelayInfoServer infoserver = new RelayInfoServer("InfoServer1", 10245, infoclient);
            sessionclient.setServer(sessionserver);
            infoclient.setServer(infoserver);
            if (sessionserver.connect() && infoserver.connect() )
            {
              if(sessionclient.connect() && infoclient.connect() ){
                   Console.ReadLine();
                   sessionclient.disconnect();
                   infoclient.disconnect();
              }
            }
           
            sessionserver.disconnect();
            infoserver.disconnect();

		}
	}
}
