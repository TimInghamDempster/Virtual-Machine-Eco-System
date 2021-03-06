﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Debugger
{
    /// <summary>
    /// Translates a VM instruction into various human usable properties
    /// </summary>
    public class InstructionViewModel : INotifyPropertyChanged
    {
        bool _isCurrentInstruction;
        UInt32 _location;
        private HashSet<UInt32> _breakpoints;

        public event PropertyChangedEventHandler PropertyChanged;
        
        // Notify the main debugger that a breakpoint has been set
        // or unset on us
        public event Action BreakpointChanged;

        public UInt32 LocationInt => _location;

        // The contents of the instruction translated into human readable strings
        public String Location { get; private set; }
        public String ExecutionUnit { get; private set; }
        public String Instruction { get; private set; }
        public String Arg1 { get; private set; }
        public String Arg2 { get; private set; }
        public String Arg3 { get; private set; }

        public bool IsBreakpoint
        {
            get { return _breakpoints.Contains(LocationInt); }
            set {  BreakpointChanged?.Invoke(); }
        }

        // The current instruction has a marker next to it so that the user
        // can see what point execution has reached
        public bool IsCurrentInstruction
        {
            get
            {
                return _isCurrentInstruction;
            }
            set
            {
                _isCurrentInstruction = value;
                OnPropetyChanged(nameof(IsCurrentInstruction));
                OnPropetyChanged(nameof(IsNotCurrentInstruction));
            }
        }

        // For binding convenience, allows us to hide the current instruction marker
        // without changing row width
        public bool IsNotCurrentInstruction { get { return !IsCurrentInstruction; } }

        public InstructionViewModel(UInt32 location, int instruction1, int instruction2, HashSet<UInt32> breakpoints)
        {
            Location = location.ToString();
            _location = location;

            //The unit is always the first byte of an instruction and maps directly
            // to the enum
            var unitCode = (Virtual_Machine.UnitCodes)(instruction1 & 0xFF000000);
            ExecutionUnit = unitCode.ToString();

            Instruction = GetInstruction(unitCode, instruction1 & 0x00FF0000);

            // Args 1 and 2 are just bytes 3 and 4 of the first instruction word
            Arg1 = ((instruction1 & 0xFF00) >> 8).ToString();
            Arg2 = (instruction1 & 0xFF).ToString();
            // Arg3 is the second instruction word
            Arg3 = instruction2.ToString();

            _breakpoints = breakpoints;
        }

        /// <summary>
        /// Given the second byte of the first word of an instruction and its
        /// execution unit, get a human readable name for the instruction
        /// </summary>
        static String GetInstruction(Virtual_Machine.UnitCodes unit, int instruction)
        {
            // The second byte of an instruction maps directly to the enum defining
            // the instruction name.  The only complexity is that there is one enum
            // per execution unit, so we have to switch on execution unit to get the
            // right one
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
