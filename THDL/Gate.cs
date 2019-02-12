using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THDL
{
    public enum GateType
    {
        Pin,
        And,
        Or,
        Not,
        Xor,
        NAnd,
        Nor
    }

    public class Gate
    {
        public GateType Type { get; set; }
        public UInt32 Input1 { get; set; }
        public UInt32 Input2 { get; set; }
        public UInt32 Output { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public static float Input1XOffset { get { return -0.05f; } }
        public static float Input1YOffset { get { return -0.1f; } }
        public static float Input2XOffset { get { return 0.05f; } }
        public static float Input2YOffset { get { return -0.1f; } }
    }
}
