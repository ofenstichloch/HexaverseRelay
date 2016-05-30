using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectateServer
{
    class MessageBuffer
    {
        //Reqeustafter > Delay
        const int REQUESTAFTER = 4;
        const int DELAY = 2;
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
            rounds.Add(new RoundBuffer());
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
            if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) round = BitConverter.ToUInt32(paket, 9);
            if (channel == (int)Protocol.ChannelID.BeginEverything)
            {
                receivingEverything = true;
                baseStates.Add(new RoundBuffer());
                incrementals = 0;
            }
            //Add messages
            if (receivingEverything)
            {
                baseStates[baseStates.Count - 1].add(paket);
                if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) baseStates[baseStates.Count - 1].setRound(round);
                if (!hasData)
                {
                    rounds[0].setRound(round);
                    rounds[0].add(paket);
                }
            }
            else {
                if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0) rounds.Add(new RoundBuffer(round));
                rounds[rounds.Count - 1].add(paket);
            }

                if (channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0)
                {
                    //Set buffer ready
                    if (!hasData && rounds.Count == DELAY + 1)
                    {
                        Log.notify("Buffer ready", this);
                        notReady.Release();
                        hasData = true;
                    }
                    //set spectating round
                    if (hasData)
                    {
                        specRound = round - DELAY;

                        //remove old phases
                        if (rounds[0].CompareTo(specRound) < 0) rounds.RemoveAt(0);
                        if (baseStates.Count > 1 && baseStates[1].CompareTo(specRound) < 0) baseStates.RemoveAt(0);
                        //Statistics
                        long cacheSize = 0;
                        foreach (RoundBuffer r in rounds)
                        {
                            cacheSize += r.getByteCount();
                        }
                        long stateSize = 0;
                        foreach (RoundBuffer r in baseStates)
                        {
                            stateSize += r.getByteCount();
                        }
                        Analytics.Statistics.updateCache(round, cacheSize, stateSize);
                        Analytics.Statistics.printStatus();
                    }
                    //start a new baseState
                    if (rounds[0].CompareTo(specRound) == 0 && rounds[0].getRound() == baseStates[0].getRound()+1)
                    {
                        baseStates[0].add(rounds[0]);
                        baseStates[0].setRound(rounds[0].getRound());
                    }
                    //count to request basestate
                    if (!receivingEverything)
                    {
                        incrementals++;
                        if (incrementals == REQUESTAFTER) host.sessionClient.requestEverything();
                    }
                }
            
            if (channel == (int)Protocol.ChannelID.EndEverything)
            {
                receivingEverything = false;
            }
            //if(channel == (int)Protocol.ChannelID.PhaseChange && paket[8]==0) print();
            access.Release();
            //start a new round for spectators
            if (hasData && !receivingEverything && channel == (int)Protocol.ChannelID.PhaseChange && paket[8] == 0)
            {
                host.sessionClient.nextRound(this.getNextRound());
            }
        }

        public void print()
        {
            Log.notify("---------Current round: " + round + ". Spectating " + specRound+"----------", this);
            Log.notify("Round buffer:", this);
            int i = 0;
            foreach(RoundBuffer r in rounds)
            {
                Log.notify(i + ": " + r, this);
                i++;
            }
            Log.notify("State buffer:", this);
            i = 0;
            foreach (RoundBuffer r in baseStates)
            {
                Log.notify(i + ": " + r, this);
                i++;
            }
            Log.notify("Next rounds first message: " + ((Protocol.ChannelID)BitConverter.ToInt32(rounds[0].getData()[0], 0)).ToString(), this);
            Log.notify("------------------------------------------------------------", this);
        }

        public byte[][] getData()
        {
            notReady.WaitOne();
            notReady.Release();
            access.WaitOne();
            byte[][] data = baseStates[0].getData();
            access.Release();
            return data;

        }

        public byte[][] getNextRound()
        {
            if (receivingEverything) return new byte[0][];
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

        public UInt32 getSpectatorRound()
        {
            return specRound;
        }

    }
}
