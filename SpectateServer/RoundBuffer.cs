using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer
{
    class RoundBuffer : IComparable<uint>
    {
        /* 
        * Storing data for one round beginning with PhaseChange for phase 0 round x
        *
        */
        List<byte[]> data;
        uint round;


        public RoundBuffer(uint round)
        {
            this.round = round;
        }

        public RoundBuffer() { }

        public int getSize()
        {
            return data.Count;
        }

        public byte[][] getData()
        {
            return data.ToArray();
        }

        public void add(byte[] message)
        {
            data.Add(message);
        }

        public void setRound(uint newRound)
        {
            this.round = newRound;
        }

        public int CompareTo(uint other)
        {
            return round.CompareTo(other);
        }
    }

}
