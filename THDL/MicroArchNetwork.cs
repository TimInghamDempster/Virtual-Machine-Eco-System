using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THDL
{
    public class MicroArchNetwork
    {
        public static Network GenerateNetwork()
        {
            var network = new Network();

            network.Gates.Add(new Gate() { Type = GateType.Pin, Output = 0, X = 0.2f, Y = 0.1f});//Pin 0
            network.Gates.Add(new Gate() { Type = GateType.Pin, Output = 1, X = 0.5f, Y = 0.1f });//Pin 1
            network.Gates.Add(new Gate() { Type = GateType.And, Input1 = 0, Input2 = 1, Output = 2, X = 0.4f, Y = 0.5f });//AND gate

            network.Wires.Add(new Wire ());
            network.Wires.Add(new Wire());
            network.Wires.Add(new Wire());

            ConnectWireToGateOutput(network.Gates[0], network.Wires[0]);
            ConnectWireToGateInput1(network.Gates[2], network.Wires[0]);

            ConnectWireToGateOutput(network.Gates[1], network.Wires[1]);
            ConnectWireToGateInput2(network.Gates[2], network.Wires[1]);

            ConnectWireToGateOutput(network.Gates[2], network.Wires[2]);

            network.Wires[2].Right = 0.4f;
            network.Wires[2].Bottom = 0.8f;

            return network;
        }

        public static void ConnectWireToGateInput1(Gate gate, Wire wire)
        {
            wire.Bottom = gate.Y + Gate.Input1YOffset;
            wire.Right = gate.X + Gate.Input1XOffset;
        }

        public static void ConnectWireToGateInput2(Gate gate, Wire wire)
        {
            wire.Bottom = gate.Y + Gate.Input2YOffset;
            wire.Right = gate.X + Gate.Input2XOffset;
        }

        public static void ConnectWireToGateOutput(Gate gate, Wire wire)
        {
            wire.Top = gate.Y;
            wire.Left = gate.X;
            wire.Input = gate.Output;
        }
    }
}
