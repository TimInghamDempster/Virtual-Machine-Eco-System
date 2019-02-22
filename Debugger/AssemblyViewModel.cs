using System;
using System.Collections.Generic;
using Virtual_Machine;

namespace Debugger
{
    class AssemblyViewModel : ViewModelBase
    {
        private readonly VirtualMachine _virtualMachine;

        private UInt32 _instructionPointer;

        private HashSet<UInt32> _breakpoints;

        // Will bind to the currently broken on instruction
        InstructionViewModel m_currentInstruction;

        // The instruction window we are displaying can be from any location in memory
        // so we need to store it as a dictionary rather than a list
        public Dictionary<UInt32, InstructionViewModel> InstructionStream { get; private set; }

        public event Action<InstructionViewModel> BreakpointChanged;

        public AssemblyViewModel(VirtualMachine virtualMachine, UInt32 instructionPointer, HashSet<UInt32> breakpoints)
        {
            _virtualMachine = virtualMachine;
            _breakpoints = breakpoints;

            UpdateAllInstructions(instructionPointer);
        }

        public void UpdateAllInstructions(uint instructionPointer)
        {
            _instructionPointer = instructionPointer;

            LoadInstructionStream();
            UpdateCurrentInstruction();
            OnPropertyChanged(nameof(InstructionStream));
        }
        
        /// <summary>
        /// Updates the CurrentInstruction VM, sets it to the VM of
        /// the instruction aimed at by the InstructionPointer
        /// </summary>
        void UpdateCurrentInstruction()
        {
            if (!InstructionStream.ContainsKey(_instructionPointer))
            {
                LoadInstructionStream();
            }

            var newCurrentInstruction = InstructionStream[_instructionPointer];

            if (newCurrentInstruction != m_currentInstruction)
            {
                // Will be null when constructing ourselves
                if (m_currentInstruction != null)
                {
                    m_currentInstruction.IsCurrentInstruction = false;
                }

                newCurrentInstruction.IsCurrentInstruction = true;
                m_currentInstruction = newCurrentInstruction;
            }
        }

        /// <summary>
        /// Gets the instruction window around the current instruction
        /// </summary>
        void LoadInstructionStream()
        {
            var newInstructionStream = new Dictionary<uint, InstructionViewModel>();
            const int instructionSizeInWords = 2;

            // There are two possible places that programs could be stored: main memory or the BIOS
            if (_instructionPointer >= Virtual_Machine.VirtualMachine.biosStartAddress &&
                _instructionPointer < Virtual_Machine.VirtualMachine.displayStartAddress)
            {
                var biosData = _virtualMachine.BIOS.Data;

                // If in the BIOS load the whole thing
                for (int instructionId = 0; instructionId < biosData.Length - 1; instructionId += instructionSizeInWords)
                {
                    UInt32 location = (UInt32)instructionId + Virtual_Machine.VirtualMachine.biosStartAddress;
                    var ivm = new InstructionViewModel(location, biosData[instructionId], biosData[instructionId + 1], _breakpoints);
                    ivm.BreakpointChanged += () => BreakpointChanged?.Invoke(ivm);
                    newInstructionStream.Add(location, ivm);
                } 
            }
            else
            {
                // If in main memory load a window around the current instruction
                UInt32 min = Math.Max(_instructionPointer - 20, Virtual_Machine.VirtualMachine.RAMStartAddress);
                UInt32 max = Math.Min(_instructionPointer + 40, Virtual_Machine.VirtualMachine.RAMStartAddress + Virtual_Machine.VirtualMachine.RAMSize);

                var ramData = _virtualMachine.RAM.GetData(min, max);

                for (int i = 0; i < ramData.Length - 1; i += instructionSizeInWords)
                {
                    UInt32 location = (UInt32)i + min;
                    var ivm = new InstructionViewModel(location, ramData[i], ramData[i + 1], _breakpoints);
                    ivm.BreakpointChanged += () => BreakpointChanged?.Invoke(ivm);
                    newInstructionStream.Add(location, ivm);
                }
            }

            InstructionStream = newInstructionStream;
            OnPropertyChanged(nameof(InstructionStream));
        }
    }
}
