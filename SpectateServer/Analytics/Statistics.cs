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
        Dictionary<int,int> channels;
        Dictionary<int,List<int>> channelSizes;
        long sentBytes;

        

        public Statistics(MessageBuffer buffer,RelaySessionClient client, RelaySessionServer server)
        {
            if (instance != null) throw new Exception("Statistics already exist");
            this.buffer = buffer;
            this.client = client;
            this.server = server;
            cacheSize = new List<long>();
            stateCacheSize = new List<long>();
            channels = new Dictionary<int, int>();
            channelSizes = new Dictionary<int, List<int>>();
            Statistics.instance = this;
        }

        public static void addSentBytes(int count)
        {
			//TODO: Add mutex?
            instance.sentBytes += count;
        }

        public static void updateCache(UInt32 round, long cacheSize, long stateCacheSize)
        {
            instance.cacheSize.Add(cacheSize);
            instance.stateCacheSize.Add(stateCacheSize);
           
        }

        public static void addPacket(byte[] data)
        {
            int currentVal = 0;
            int channel = BitConverter.ToInt32(data, 0);
            instance.channels.TryGetValue(channel, out currentVal);
            instance.channels[channel] = currentVal + 1;
            List<int> l;
            instance.channelSizes.TryGetValue(channel, out l);
            if (l == null)
            {
                l = new List<int>();
                l.Add(data.Length);
                instance.channelSizes.Add(channel, l);
            }
            else
            {
                instance.channelSizes[channel].Add(data.Length);
            }

        }

        public static String getStatistics()
        {
            if (instance != null && instance.cacheSize.Count > 0 && instance.stateCacheSize.Count > 0)
            {
                String s = "---------------Statistics------------\n\r";
				s += "Ran with " + MessageBuffer.DELAY + " rounds delay\n\r";
				s += "Requested a new state every " + MessageBuffer.REQUESTAFTER + " rounds\n\r";
                s += "RoundCache\t" + instance.cacheSize.Average(x => x) + "\tbytes for an average round\n\r";
                s += "StateCache\t" + instance.stateCacheSize.Average(x => x) + "\tbytes for an average state\n\r";
                foreach(KeyValuePair<int,List<int>> entry in instance.channelSizes)
                {
                    double avg = 0;
                    foreach(int i in entry.Value)
                    {
                        avg += i;
                    }
                    avg /= entry.Value.Count;
                    s += "Average size for Channel,count\t" + ((Protocol.ChannelID)entry.Key).ToString() + "\t" + avg+"\t"+entry.Value.Count+"\n\r";
                }
                return s;
            }
            return null;
        }
    
        public static void printStatus()
        {
            uint round = instance.buffer.getSpectatorRound();
            Log.notify("\tRound: "+round+"\tRoundSize\t" + instance.cacheSize[instance.cacheSize.Count-1], instance);
            Log.notify("\tRound: "+round + "\tStateSize\t" + instance.stateCacheSize[instance.stateCacheSize.Count-1], instance);
            Log.notify("\tRound: "+round + "\tTotal players\t" + instance.server.getServerInfo().gameInfo.numFactionsCreated, instance);
            Log.notify("\tRound: "+round + "\tBytes sent\t" + instance.sentBytes, instance);
            instance.sentBytes = 0;

        }

        public static Statistics getInstance()
        {
            return instance;
        }

    }
}
