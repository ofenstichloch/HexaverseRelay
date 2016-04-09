using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer
{
    class Host
    {
        public Protocol.TUniqueID hostID;
        public Protocol.PlanetConfig planetConfig;
        public Protocol.ServerInfo serverInfo;
        public byte[] phase;


        public RelayInfoClient infoClient;
        public RelayInfoServer infoServer;
        public RelaySessionClient sessionClient;
        public RelaySessionServer sessionServer;

        int clientPort;
        public int serverPort;
        string clientHost;

        public void startHost(int cp, string host, int sp)
        {
            this.clientHost = host;
            this.clientPort = cp;
            this.serverPort = sp;

            hostID = Protocol.TUniqueID.generate();

            sessionClient = new RelaySessionClient("SessionClient1", host, clientPort + 1, this);
            infoClient = new RelayInfoClient("InfoClient1", clientHost, clientPort, this);
            sessionServer = new RelaySessionServer("SessionServer1", serverPort + 1,this);
            infoServer = new RelayInfoServer("InfoServer1", serverPort, this);
            sessionClient.setServer(sessionServer);
            infoClient.setServer(infoServer);
            if (sessionServer.connect() && infoServer.connect())
            {
                if (sessionClient.connect() && infoClient.connect())
                {
                    Console.ReadLine();
                    sessionClient.disconnect();
                    infoClient.disconnect();
                }
            }
            sessionServer.disconnect();
            infoServer.disconnect();
        }

    }
}
