using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectateServer
{
    class MessageBuffer
    {
        const int ROUNDSTOSAVE = 10;
        const int DELAY = 5;
        Host host;
        //private List<byte[]> buffer;
        uint firstRound;
        uint lastRound;
        bool empty;
        Semaphore access;
        Semaphore notReady;
        bool initialized = false;

        Queue<int> roundPointer;
        Queue<byte[]> buffer;
        bool hasDelayedData;
        public MessageBuffer(Host host)
        {
            this.host = host;
            buffer = new Queue<byte[]>();
            roundPointer = new Queue<int>(ROUNDSTOSAVE);
            access = new Semaphore(1, 1);   
            notReady = new Semaphore(0, 1); //Start blocked, queue has nothing to serve
        }

        public void add(byte[] paket)
        {
            int channel = BitConverter.ToInt32(paket, 0);
            if (channel == (int)Protocol.ChannelID.PhaseChange)
            {
             
            }
            if(lastRound > firstRound+ROUNDSTOSAVE)
            {
                Log.notify("Cache went full, resetting", this);
                notReady.WaitOne();
                initialized = false;
                buffer.Clear();
                firstRound = 0;
                lastRound = 0;
                empty = true;
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
