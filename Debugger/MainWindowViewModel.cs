using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Virtual_Machine;

namespace Debugger
{
    class MainWindowViewModel : ViewModelBase
    {
        // We need a VM to debug!
        Virtual_Machine.VirtualMachine _virtualMachine;

        // Tracks if we are currently broken or not, needs to
        // be volatile because the VM runs on a worker thread
        // which needs to know whether to keep going or not, but
        // that decision is made from the UI thread
        volatile bool m_running;
        
        // We need to run the VM on a background thread so that we
        // can use the UI thread to browse the code / set breakpoints /
        // etc while the VM is running
        private readonly BackgroundWorker _runVirtualMachineThread;

        // A breakpoint is just defined by the location of the instruction in
        // memory, might need to extend this if we want to do conditional breakpoints
        private readonly HashSet<UInt32> _breakPoints;

        public UInt32 InstructionPointer => _virtualMachine.Processor.Cores[0].InstructionPointer;

        /// <summary>
        /// When executed steps a broken VM to the next instruction
        /// </summary>
        public LambdaCommand StepVirtualMachineCommand { get; }

        /// <summary>
        /// When executed resumes a broken VM or breaks into a running one
        /// </summary>
        public LambdaCommand StartStopCommand { get; }


        public List<RegisterViewModel> Registers { get; }

        public string StartStop => m_running ? "Pause" : "Run";

        public AssemblyViewModel AssemblyViewModel { get; }

        public MainWindowViewModel()
        {
            // When we start up, run the compiler so that we have the latest version of whatever
            // we are debugging
            using (var comp = new Process())
            {
                comp.StartInfo.FileName = (@"..\..\..\Compiler\Compiler\bin\Debug\Compiler.exe");
                comp.Start();
            }

            // Spin up a VM
            _virtualMachine = new Virtual_Machine.VirtualMachine();

            _breakPoints = new HashSet<UInt32>();

            StepVirtualMachineCommand = new LambdaCommand(
                (o) =>
                {
                    StepVM();
                    RefreshMachineData();
                });

            StartStopCommand = new LambdaCommand((o) => { SwitchRunning(); });

            AssemblyViewModel = new AssemblyViewModel(_virtualMachine, InstructionPointer, _breakPoints);
            AssemblyViewModel.BreakpointChanged += instructionViewModel =>
                ChangeBreakpoint(instructionViewModel.LocationInt, !instructionViewModel.IsBreakpoint);

            Registers = new List<RegisterViewModel>();
            // TODO: support multi-core
            for (int registerIndex = 0; registerIndex < _virtualMachine.Processor.Cores[0].Registers.Count(); registerIndex++)
            {
                Registers.Add(new RegisterViewModel(registerIndex));
            }

            _runVirtualMachineThread = new BackgroundWorker();
            // Do work is just stepping the VM one instruction
            _runVirtualMachineThread.DoWork += new DoWorkEventHandler((o, e) => StepVM());
            // When we've done one instruction, check if we've been broken into, if not run the next instruction
            _runVirtualMachineThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, e) => { if (m_running) _runVirtualMachineThread.RunWorkerAsync(); });
        }

        /// <summary>
        /// Sets or unsets a breakpoint at the given location
        /// </summary>
        private void ChangeBreakpoint(UInt32 location, bool set)
        {
            if(set)
            {
                _breakPoints.Add(location);
            }
            else
            {
                _breakPoints.Remove(location);
            }
        }

        /// <summary>
        /// Breaks a running VM or runs a broken one, can be called by a
        /// command on the UI, hitting a user defined breakpoint, or hitting
        /// a break instruction
        /// </summary>
        void SwitchRunning()
        {
            m_running = !m_running;
            OnPropertyChanged(nameof(StartStop);

            if(m_running)
            {
                _runVirtualMachineThread.RunWorkerAsync();
            }
            else
            {
                RefreshMachineData();
            }
        }

        /// <summary>
        /// Runs the VM for one instruction
        /// </summary>
        public void StepVM()
        {
            UInt32 oldInstructionPointer = InstructionPointer;

            // It can take multiple ticks to run one instruction
            // so keep going until the IP changes
            while(oldInstructionPointer == InstructionPointer)
            {
                _virtualMachine.Tick();
            }

            // Check for a user set breakpoint
            if (_breakPoints.Contains(InstructionPointer))
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }

            // Check for a break instruction
            if (IsBreakInstruction())
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }
        }

        /// <summary>
        /// Doesn't check if an instruction is in the breakpoint list, but rather
        /// if the instruction at the current InstructionPointer itself is a Branch|Break
        /// instruction, which is a breakpoint defined in the instruction stream of the
        /// program
        /// </summary>
        private bool IsBreakInstruction()
        {
            if (InstructionPointer >= VirtualMachine.biosStartAddress &&
                InstructionPointer < VirtualMachine.displayStartAddress)
            {
                var biosData = _virtualMachine.BIOS.Data;
                var instruction = biosData[InstructionPointer - VirtualMachine.biosStartAddress];

                if ((instruction & 0xFF000000) == (UInt32) UnitCodes.Branch &&
                    (instruction & 0x00FF0000) == (UInt32) BranchOperations.Break)
                {
                    return true;
                }
            }
            else
            {
                var RAMData = _virtualMachine.RAM.GetData(InstructionPointer, InstructionPointer + 1);
                var instruction = RAMData[0];

                if ((instruction & 0xFF000000) == (UInt32) UnitCodes.Branch &&
                    (instruction & 0x00FF0000) == (UInt32) BranchOperations.Break)
                {
                    return true;
                }
            }

            return false;
        }

        // When we break in we need to update the UI to display the current machine state
        // but don't want to do this every step as that would be slow, instead we call this
        // whenever we hit a breakpoint or the user steps when broken in
        public void RefreshMachineData()
        {
            AssemblyViewModel.UpdateAllInstructions(InstructionPointer);

            for(int i = 0; i < Registers.Count; i++)
            {
                Registers[i].SetValue(_virtualMachine.Processor.Cores[0].Registers[i]);
            }
            OnPropertyChanged(nameof(Registers));
        }
    }
}
