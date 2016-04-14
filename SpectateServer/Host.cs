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
            if (infoClient.connect() && sessionClient.connect())
            {
                Log.notify("Clients connected", this);
                if(infoServer.connect() && sessionServer.connect())
                {
                    Log.notify("Server started", this);
                    Console.ReadLine();
                    sessionServer.disconnect();
                    infoServer.disconnect();
                }
                sessionClient.disconnect();
                infoClient.disconnect();
            }
            else
            {
                Log.error("Failed to connect gameclient", this);
            }

        }

        public void disconnect()
        {
            sessionServer.disconnect();
            infoServer.disconnect();
            sessionClient.disconnect();
            infoClient.disconnect();
            Console.ReadLine();
        }

    }
}
