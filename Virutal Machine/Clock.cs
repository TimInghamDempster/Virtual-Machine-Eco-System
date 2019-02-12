using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    class Clock
    {
        ulong m_tickCount;

        public void Tick()
        {
            m_tickCount++;
        }
    }
}
