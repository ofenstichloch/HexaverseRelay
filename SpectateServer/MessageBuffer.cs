using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectateServer
{
    class MessageBuffer
    {
        const int REQUESTAFTER = 10;
        const int DELAY = 5;
        Host host;
        Semaphore access;
        Semaphore notReady;
        List<byte[]> buffer;
        List<Pointer> pointer;
        bool receivingEverything;
        uint specRound;
        uint round;
        int incrementals;
        bool hasData;

        class Pointer : IEquatable<Pointer>
        {
            public bool isEverything;
            public uint round;
            public int position;
            public Pointer(bool e, uint r,int pos)
            {
                this.isEverything = e;
                this.round = r;
                this.position = pos;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Pointer p = (Pointer)obj;
                return p.round == this.round;
            }

            public bool Equals(Pointer other)
            {
                return other.round == this.round;
            }
        }
        public MessageBuffer(Host host)
        {
            this.host = host;
            buffer = new List<byte[]>();
            pointer = new List<Pointer>();
            access = new Semaphore(1, 1);
            notReady = new Semaphore(0, 1);
            hasData = false;
            receivingEverything = false;
            specRound = 0;
            incrementals = 0;
            round = 0;

        }

        public void add(byte[] paket)
        {
            access.WaitOne(); //Full mutex
            int channel = BitConverter.ToInt32(paket, 0);

            if (channel == (int)Protocol.ChannelID.BeginEverything)
            {
                receivingEverything = true;
                pointer.Add(new Pointer(true, 0, buffer.Count));
                incrementals = 0;
            }
            else if (channel == (int)Protocol.ChannelID.EndEverything)
            {
                receivingEverything = false;
            }
            else if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0)
            {
                round = BitConverter.ToUInt32(paket, 9);
                if (receivingEverything) pointer[pointer.Count - 1].round = round;
                else
                {
                    pointer.Add(new Pointer(false, round, buffer.Count));
                    incrementals++;
                }
                //Initialize Buffer
                    if (!hasData && pointer.Count > DELAY)
                    {
                        notReady.Release();
                        hasData = true;
                        Log.notify("Gathered enough data to fill delay", this);
                    }
                if (hasData) {
                    specRound = round - DELAY ;
                    Pointer nextSpecRound = pointer.FindLast(x => x.round == specRound);
                    int pos = nextSpecRound.position;
                    if (nextSpecRound.isEverything)
                    {
                        Log.notify("Clearin cache", this);
                        buffer.RemoveRange(0, nextSpecRound.position);
                        for (int i = 0; i < pointer.Count; i++)
                        {
                            pointer[i].position -= pos;
                        }
                    }
                }

                // Delete oldest Pointer
                if (pointer[0].round < specRound && hasData) pointer.RemoveAt(0);
                // Refresh basedata
                if (incrementals >= REQUESTAFTER)
                {
                    pointer.RemoveAt(pointer.Count - 1);
                    host.sessionClient.requestEverything();
                }
            }

            buffer.Add(paket);
            if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) print();
            access.Release();

        }

        private void print()
        {
            Log.notify("----------------------------------------Buffer Begin---------------", this);
            int i = 0;
            foreach(byte[] paket in buffer.ToArray())
            {
                int channel = BitConverter.ToInt32(paket, 0);
                if(channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0)
                {
                    Log.notify(i + ": " + (Protocol.ChannelID)channel+" Round "+BitConverter.ToUInt32(paket,9), this);
                }
                else
                {
                    Log.notify(i + ": " + (Protocol.ChannelID)channel, this);
                }
                
                i++;
            }
            i = 0;
            foreach (Pointer p in pointer.ToArray())
            {
                Log.notify("Pointer to " + p.position + " for round " + p.round + " with " + p.isEverything, this);
            }

            Log.notify("Spectating Round " + specRound, this); 
            Log.notify("---------------------------------------Buffer End-------------------", this);
        }

        public byte[][] getData()
        {
            notReady.WaitOne();
            notReady.Release();
            access.WaitOne();
            Pointer nextSpecRound = pointer.FindLast(x => x.round == specRound+1);
            byte[][] data = new byte[nextSpecRound.position][];
            buffer.CopyTo(0, data, 0, nextSpecRound.position);
            access.Release();
            return data;

        }

        public byte[][] getNextRound()
        {
            notReady.WaitOne();
            notReady.Release();
            access.WaitOne();
            Pointer nextSpecRound = pointer.FindLast(x => x.round == specRound+1);
            Pointer lastSpecRound = pointer.FindLast(x => x.round == specRound);
            int pos;
            if (lastSpecRound == null) {
                pos = 0;
            }
            else
            {
                pos = Math.Max(lastSpecRound.position,0);
            }
            try {
                Log.notify("Sending Data " + pos + " to " + (nextSpecRound.position),this);
                byte[][] data = new byte[nextSpecRound.position - pos][];
                buffer.CopyTo(pos, data, 0, nextSpecRound.position - pos);
                access.Release();
                return data;
            }
            catch(Exception e)
            {
                Log.error("Uh OH", this);
            }
            return null;

        }
        public bool isReady()
        {
            return hasData;
        }

    }
}
