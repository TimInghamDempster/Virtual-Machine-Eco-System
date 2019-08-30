using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Virtual_Machine;

namespace Debugger
{
    class AssemblyViewModel : ViewModelBase
    {
        private readonly VirtualMachine _virtualMachine;

        private uint _instructionPointer;

        private HashSet<uint> _breakpoints;

        private List<DebugInfoViewModel> _debugInfo = new List<DebugInfoViewModel>();

        // Will bind to the currently broken on instruction
        InstructionViewModel m_currentInstruction;

        // The instruction window we are displaying can be from any location in memory
        // so we need to store it as a dictionary rather than a list
        private Dictionary<uint, InstructionViewModel> _instructionDictionary;

        public event Action<InstructionViewModel> BreakpointChanged;

        public IEnumerable<object> InstructionStream
        {
            get
            {
                var instructionList = new List<object>();
                InstructionViewModel previousInstruction = null;

                foreach(var instruction in _instructionDictionary.Values)
                {
                    foreach(var debugInfo in _debugInfo)
                    {
                        var previousLocation = previousInstruction?.LocationInt ?? 
                            instruction.LocationInt - 10;

                        // Account for program being loaded into an address space
                        var debugAddress = debugInfo.Address + VirtualMachine.RAMStartAddress;
                        if(debugAddress > previousLocation &&
                            debugAddress <= instruction.LocationInt)
                        {
                            instructionList.Add(debugInfo);
                        }
                    }
                    instructionList.Add(instruction);
                    previousInstruction = instruction;
                }
                return instructionList;
            }
        }

        public AssemblyViewModel(VirtualMachine virtualMachine, uint instructionPointer, HashSet<uint> breakpoints)
        {
            _virtualMachine = virtualMachine;
            _breakpoints = breakpoints;

            using(var debugReader = new StreamReader("Debug.dbg"))
            {
                var address = int.Parse(debugReader.ReadLine());
                while(!debugReader.EndOfStream)
                {
                    var nextLine = debugReader.ReadLine();
                    var info = "";

                    while (!int.TryParse(nextLine, out address) 
                        && !debugReader.EndOfStream)
                    {
                        info += nextLine;
                        nextLine = debugReader.ReadLine();
                    }

                    _debugInfo.Add(
                        new DebugInfoViewModel()
                        {
                            Address = address,
                            Info = info,
                        }); 
                }
            }

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
            if (!_instructionDictionary.ContainsKey(_instructionPointer))
            {
                LoadInstructionStream();
            }

            var newCurrentInstruction = _instructionDictionary[_instructionPointer];

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
            if (_instructionPointer >= VirtualMachine.biosStartAddress &&
                _instructionPointer < VirtualMachine.displayStartAddress)
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
                uint min = Math.Max(_instructionPointer - 20, VirtualMachine.RAMStartAddress);
                uint max = Math.Min(_instructionPointer + 40, VirtualMachine.RAMStartAddress + VirtualMachine.RAMSize);

                var ramData = _virtualMachine.RAM.GetData(min, max);

                for (int i = 0; i < ramData.Length - 1; i += instructionSizeInWords)
                {
                    uint location = (uint)i + min;
                    var ivm = new InstructionViewModel(location, ramData[i], ramData[i + 1], _breakpoints);
                    ivm.BreakpointChanged += () => BreakpointChanged?.Invoke(ivm);
                    newInstructionStream.Add(location, ivm);
                }
            }

            _instructionDictionary = newInstructionStream;
            OnPropertyChanged(nameof(_instructionDictionary));
        }
    }
}
