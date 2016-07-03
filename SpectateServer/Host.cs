using System;
using SpectateServer.Analytics;

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
        public MessageBuffer buffer;

        int clientPort;
        public int serverPort;
        string clientHost;


        public void startHost(int cp, string host, int sp)
        {
            this.clientHost = host;
            this.clientPort = cp;
            this.serverPort = sp;
            
            hostID = Protocol.TUniqueID.generate();
            buffer = new MessageBuffer(this);
            sessionClient = new RelaySessionClient("SessionClient1", host, clientPort + 1, this, buffer);
            infoClient = new RelayInfoClient("InfoClient1", clientHost, clientPort, this);
            sessionServer = new RelaySessionServer("SessionServer1", serverPort + 1,this, buffer);
            infoServer = new RelayInfoServer("InfoServer1", serverPort, this);
            Statistics stats = new Statistics(buffer, sessionClient, sessionServer);
            sessionClient.setServer(sessionServer);
            infoClient.setServer(infoServer);
            if (infoClient.connect() && sessionClient.connect())
            {
                Log.notify("Clients connected", this);
                if(infoServer.connect() && sessionServer.connect())
                {
                    Log.notify("Server started on port "+serverPort, this);
                    Console.ReadLine();
                   
                    disconnect();
                }
                else
                {
                    Log.error("Failed to start server", this);
                }
            }
            else
            {
                Log.error("Failed to connect gameclient", this);
            }

        }

        public void disconnect()
        {
            Log.error(Statistics.getStatistics(), this);
            sessionServer.disconnect();
            infoServer.disconnect();
            sessionClient.disconnect();
            infoClient.disconnect();
        }

    }
}
