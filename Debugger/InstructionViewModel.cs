using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debugger
{
    public class InstructionViewModel : INotifyPropertyChanged
    {
        bool m_isCurrentInstruction;
        bool m_isBreakpoint;
        int m_location;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<int, bool> BreakpointChanged;

        public String Location { get; private set; }
        public String ExecutionUnit { get; private set; }
        public String Instruction { get; private set; }
        public String Arg1 { get; private set; }
        public String Arg2 { get; private set; }
        public String Arg3 { get; private set; }

        public bool IsBreakpoint
        {
            get { return m_isBreakpoint; }
            set { m_isBreakpoint = value; BreakpointChanged?.Invoke(m_location, value); }
        }

        public bool IsCurrentInstruction
        {
            get
            {
                return m_isCurrentInstruction;
            }
            set
            {
                m_isCurrentInstruction = value;
                OnPropetyChanged("IsCurrentInstruction");
                OnPropetyChanged("IsNotCurrentInstruction");
            }
        }

        public bool IsNotCurrentInstruction { get { return !IsCurrentInstruction; } }

        public InstructionViewModel(int location, int instruction1, int instruction2)
        {
            Location = location.ToString();
            m_location = location;
            var unitCode = (Virtual_Machine.UnitCodes)(instruction1 & 0xFF000000);
            ExecutionUnit = unitCode.ToString();
            Instruction = GetInstruction(unitCode, instruction1 & 0x00FF0000);
            Arg1 = ((instruction1 & 0xFF00) >> 8).ToString();
            Arg2 = (instruction1 & 0xFF).ToString();
            Arg3 = instruction2.ToString();
        }

        static String GetInstruction(Virtual_Machine.UnitCodes unit, int instruction)
        {
            switch(unit)
            {
                case Virtual_Machine.UnitCodes.ALU:
                    return ((Virtual_Machine.ALUOperations)instruction).ToString();
                case Virtual_Machine.UnitCodes.Branch:
                    return ((Virtual_Machine.BranchOperations)instruction).ToString();
                case Virtual_Machine.UnitCodes.Interrupt:
                    return ((Virtual_Machine.InterruptInstructions)instruction).ToString();
                case Virtual_Machine.UnitCodes.Load:
                    return ((Virtual_Machine.LoadOperations)instruction).ToString();
                case Virtual_Machine.UnitCodes.Nop:
                    return "";
                case Virtual_Machine.UnitCodes.Stack:
                    return ((Virtual_Machine.StackOperations)instruction).ToString();
                case Virtual_Machine.UnitCodes.Store:
                    return ((Virtual_Machine.StoreOperations)instruction).ToString();
                default:
                    return "";
            }
        }

        void OnPropetyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
