using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THDL
{
    public class Network
    {
        List<Wire> m_wires = new List<Wire>();
        List<Gate> m_gates = new List<Gate>();

        public List<Wire> Wires { get { return m_wires; } }
        public List<Gate> Gates { get { return m_gates; } }
    }
}
