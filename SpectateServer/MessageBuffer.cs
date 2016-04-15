using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectateServer
{
    class MessageBuffer
    {
        Host host;
        private List<byte[]> buffer;
        uint firstRound;
        Semaphore access;
        bool initialized = false;
        public MessageBuffer(Host host)
        {
            this.host = host;
            buffer = new List<byte[]>();
            firstRound = 0;
            access = new Semaphore(0, 1);
        }

        public void add(byte[] paket)
        {
            int channel = BitConverter.ToInt32(paket, 0);
            if (channel == (int)Protocol.ChannelID.BeginEverything)
            {
                initialized = false;
                buffer.Clear();
                access.WaitOne();
            }
            else if (channel == (int)Protocol.ChannelID.EndEverything)
            {
                initialized = true;
                access.Release();
            }

            if(buffer.Count >= 200)
            {
                initialized = false;
                buffer.Clear();
                return;
            }

            if (initialized)
            {
                access.WaitOne();
                buffer.Add(paket);
                access.Release();
            }
            else
            {
                buffer.Add(paket);
            }

        }

        public byte[][] getData()
        {
            access.WaitOne();
            byte[][] array = buffer.ToArray();
            access.Release();
            return array;

        }

    }
}
