using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SpectateServer
{
    abstract class Log
    {
        private static int loglvl=1;
        //static StreamWriter file = new StreamWriter(@"C:\Users\alex\Downloads\hexa.txt",true);

        public static void notify(string message, Object sender)
        {
            if (loglvl >= 2)
            {
                Console.Out.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")+"    Notify: " + sender.ToString() + ":  " + message);
                //file.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "    Notify: " + sender.ToString() + ":  " + message);
                //file.Flush();
            }
        }

        public static void error(string message, Object sender)
        {
            Console.Out.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "    ERROR: " + sender.ToString() + ":  " + message);
            
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
