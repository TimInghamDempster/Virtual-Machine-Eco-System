using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(args[0]);
            UInt32 lineCount = 0;
            List<int> binaryStream = new List<int>();

            Dictionary<string, Virtual_Machine.UnitCodes> unitsByInstruction = new Dictionary<string, Virtual_Machine.UnitCodes>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = Assembler.SplitLine(line);

                Assembler.ParseExecutionUnit(parts, lineCount, binaryStream);
                lineCount++;
            }
        }
    }
}
