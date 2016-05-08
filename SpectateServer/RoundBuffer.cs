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
            data = new List<byte[]>();
            this.round = round;
        }

        public RoundBuffer() {
            data = new List<byte[]>();
        }

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

        public void add(RoundBuffer r)
        {
            foreach (byte[] d in r.data)
            {
                this.add(d);
            }
        }

        public void setRound(uint newRound)
        {
            this.round = newRound;
        }

        public int CompareTo(uint other)
        {
            return round.CompareTo(other);
        }

        public int CompareTo(RoundBuffer other)
        {
            return this.CompareTo(other.round);
        }

        public override String ToString()
        {
            return "Round " + this.round + " with " + data.Count + " pakets";
        }

    }

}
