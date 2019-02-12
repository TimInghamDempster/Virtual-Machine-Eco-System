using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    class RetireUnit
    {
        CPUCore m_CPUCore;

        public RetireUnit(CPUCore cPUCore)
        {
            m_CPUCore = cPUCore;
        }
        public void Tick()
        {
        }
    }
}
