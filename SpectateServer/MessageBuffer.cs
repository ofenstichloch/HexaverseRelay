using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectateServer
{
    class MessageBuffer
    {
        //Reqeustafter > Delay
        const int REQUESTAFTER = 10;
        const int DELAY = 5;
        Host host;
        Semaphore access;
        Semaphore notReady;
        List<RoundBuffer> rounds; //Stores only normal rounds, no "Everything"-Bundles
        List<RoundBuffer> baseStates;

        uint specRound;
        uint round;
        int incrementals;
        bool hasData;
        bool receivingEverything;

        public MessageBuffer(Host host)
        {
            this.host = host;
            rounds = new List<RoundBuffer>();
            baseStates = new List<RoundBuffer>();
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
                baseStates.Add(new RoundBuffer());
                incrementals = 0;
            }
            //Add message to the last round in the rounds or baseState list
            if (receivingEverything)
            {
                baseStates[baseStates.Count - 1].add(paket);
                if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) baseStates[baseStates.Count - 1].setRound(BitConverter.ToUInt32(paket, 9));
            }
            else {
                if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) rounds.Add(new RoundBuffer(BitConverter.ToUInt32(paket, 9)));
                    rounds[rounds.Count - 1].add(paket);
            }




            if (channel == (int)Protocol.ChannelID.EndEverything)
            {
                receivingEverything = false;
            }
            access.Release();

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
            byte[][] data = rounds[0].getData();
            access.Release();
            return data;

        }
        public bool isReady()
        {
            return hasData;
        }

    }
}
