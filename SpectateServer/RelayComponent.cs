using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectateServer
{
    abstract class RelayComponent
    {
        public string name;


        public RelayComponent(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return this.name;
        }

    }
}
