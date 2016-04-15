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
        Semaphore notReady;
        bool initialized = false;
        public MessageBuffer(Host host)
        {
            this.host = host;
            buffer = new List<byte[]>();
            firstRound = 0;
            access = new Semaphore(1, 1);
            notReady = new Semaphore(0, 1);
        }

        public void add(byte[] paket)
        {
            int channel = BitConverter.ToInt32(paket, 0);
            if (channel == (int)Protocol.ChannelID.BeginEverything)
            {
                Log.notify("Start filling cache",this);
                if (initialized) notReady.WaitOne();
                initialized = false;
                buffer.Clear();
                access.WaitOne();
            }
            else if (channel == (int)Protocol.ChannelID.EndEverything)
            {
                Log.notify("Finished filling cache with "+buffer.Count+" messages", this);
                initialized = true;
                access.Release();
                notReady.Release();
            }

            if(buffer.Count >= 200)
            {
                Log.notify("Cache went full, resetting", this);
                notReady.WaitOne();
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
            if (!initialized) host.sessionServer.requestEverything();
            notReady.WaitOne();
                access.WaitOne();
                byte[][] array = buffer.ToArray();
                access.Release();
            notReady.Release();
            return array;
        }

    }
}
