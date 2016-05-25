using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer.Analytics
{


    class Statistics
    {
       //TODO make average statistics optional to allow long runtimes
        
        public static Statistics instance;

        MessageBuffer buffer;
        RelaySessionClient client;
        RelaySessionServer server;

        List<long> cacheSize;
        List<long> stateCacheSize;
        long sentBytes;

        

        public Statistics(MessageBuffer buffer,RelaySessionClient client, RelaySessionServer server)
        {
            if (instance != null) throw new Exception("Statistics already exist");
            this.buffer = buffer;
            this.client = client;
            this.server = server;
            cacheSize = new List<long>();
            stateCacheSize = new List<long>();
            Statistics.instance = this;
        }

        public static void addSentBytes(int count)
        {
            instance.sentBytes += count;
        }

        public static void updateCache(UInt32 round, long cacheSize, long stateCacheSize)
        {
            instance.cacheSize.Add(cacheSize);
            instance.stateCacheSize.Add(stateCacheSize);
           
        }

        public static String getStatistics()
        {
            if (instance != null && instance.cacheSize.Count > 0 && instance.stateCacheSize.Count > 0)
            {
                String s = "---------------Statistics------------\n\r";
                s += "RoundCache: " + instance.cacheSize.Average(x => x) + " bytes for an average round\n\r";
                s += "StateCache: " + instance.stateCacheSize.Average(x => x) + " bytes for an average round\n\r";
                return s;
            }
            return null;
        }
    
        public static void printStatus()
        {
            uint round = instance.buffer.getSpectatorRound();
            Log.notify(round+": RoundSize: " + instance.cacheSize[instance.cacheSize.Count-1], instance);
            Log.notify(round + ": StateSize: " + instance.stateCacheSize[instance.stateCacheSize.Count-1], instance);
            Log.notify(round + ": Total players: " + instance.server.getServerInfo().gameInfo.numFactionsCreated, instance);
            Log.notify(round + ": Bytes sent: " + instance.sentBytes, instance);
            instance.sentBytes = 0;

        }

        public static Statistics getInstance()
        {
            return instance;
        }

    }
}
