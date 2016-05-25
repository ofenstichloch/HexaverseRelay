using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer.Analytics
{


    class Statistics
    {
        public static Statistics instance;

        MessageBuffer buffer;
        RelaySessionClient client;
        RelaySessionServer server;

        List<long> cacheSize;
        List<long> stateCacheSize;


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

        public static Statistics getInstance()
        {
            return instance;
        }

    }
}
