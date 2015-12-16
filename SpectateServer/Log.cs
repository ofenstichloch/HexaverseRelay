using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer
{
    abstract class Log
    {
        private static int loglvl=1;

        public static void notify(string message, Object sender)
        {
            if (loglvl >= 2)
            {
                Console.Out.WriteLine("Notify: " + sender.ToString() + ":  " + message);
            }
        }

        public static void error(string message, Object sender)
        {
            Console.Out.WriteLine("ERROR: " + sender.ToString() + ":  " + message);
        }

        public static void setLevel(int lvl)
        {
            Log.loglvl = lvl;
        }
        public static int getLevel()
        {
            return loglvl;
        }
        
    }
}
