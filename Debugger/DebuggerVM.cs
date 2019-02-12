using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Debugger
{
    class DebuggerVM : INotifyPropertyChanged
    {
        // We need a VM to debug!
        Virtual_Machine.VirtualMachine m_vm;

        // Will bind to the currently broken on instruction
        InstructionViewModel m_currentInstruction;

        // Tracks if we are currently broken or not, needs to
        // be volatile because the VM runs on a worker thread
        // which needs to know whether to keep going or not, but
        // that decision is made from the UI thread
        volatile bool m_running;
        
        // We need to run the VM on a background thread so that we
        // can use the UI thread to browse the code / set breakpoints /
        // etc while the VM is running
        BackgroundWorker m_runVMThread;

        // A breakpoint is just defined by the location of the instruction in
        // memory, might need to extend this if we want to do conditional breakpoints
        HashSet<int> m_breakPoints;

        public event PropertyChangedEventHandler PropertyChanged;

        public UInt32 InstructionPointer { get { return m_vm.Processor.Cores[0].InstructionPointer; } }

        /// <summary>
        /// When exectuted steps a broken VM to the next instruction
        /// </summary>
        public LambdaCommand StepVMCommand { get; private set; }

        /// <summary>
        /// When exectued resumes a broken VM or breaks into a running one
        /// </summary>
        public LambdaCommand StartStopCommand { get; private set; }

        // The instruction window we are displaying can be from any location in memory
        // so we need to store it as a dictionary rather than a list
        public Dictionary<UInt32, InstructionViewModel> InstructionStream { get; private set; }

        public List<RegisterViewModel> Registers { get; private set; }

        public String StartStop { get { return m_running ? "Pause" : "Run"; } }

        public DebuggerVM()
        {
            // When we start up, run the compiler so that we have the latest version of whatever
            // we are debugging
            var comp = new Process();
            comp.StartInfo.FileName = (@"..\..\..\Compiler\Compiler\bin\Debug\Compiler.exe");
            comp.Start();

            // Spin up a VM
            m_vm = new Virtual_Machine.VirtualMachine();

            StepVMCommand = new LambdaCommand(
                (o) =>
                {
                    StepVM();
                    RefreshMachineData();
                });

            StartStopCommand = new LambdaCommand((o) => { SwitchRunning(); });

            InstructionStream = new Dictionary<UInt32, InstructionViewModel>();

            LoadInstructionStream();
            m_currentInstruction = InstructionStream[InstructionPointer];

            Registers = new List<RegisterViewModel>();
            // TODO: support multi-core
            for (int registerIndex = 0; registerIndex < m_vm.Processor.Cores[0].Registers.Count(); registerIndex++)
            {
                Registers.Add(new RegisterViewModel(registerIndex));
            }

            m_runVMThread = new BackgroundWorker();
            // Do work is just stepping the VM one instruction
            m_runVMThread.DoWork += new DoWorkEventHandler((o, e) => StepVM());
            // When we've done one instruction, check if we've been broken into, if not run the next instruction
            m_runVMThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, e) => { if (m_running) m_runVMThread.RunWorkerAsync(); });

            m_breakPoints = new HashSet<int>();
        }

        /// <summary>
        /// Gets the instruction window around the current instruction
        /// </summary>
        void LoadInstructionStream()
        {
            var newIS = new Dictionary<uint, InstructionViewModel>();
            const int instructionSizeInWords = 2;

            // There are two possible places that programs could be stored: main memory or the BIOS
            if (InstructionPointer >= Virtual_Machine.VirtualMachine.biosStartAddress &&
                InstructionPointer < Virtual_Machine.VirtualMachine.displayStartAddress)
            {
                var biosData = m_vm.BIOS.Data;

                // If in the BIOS load the whole thing
                for (int instructionId = 0; instructionId < biosData.Length - 1; instructionId += instructionSizeInWords)
                {
                    UInt32 location = (UInt32)instructionId + Virtual_Machine.VirtualMachine.biosStartAddress;
                    var ivm = new InstructionViewModel((int)location, biosData[instructionId], biosData[instructionId + 1]);
                    ivm.BreakpointChanged += ChangeBreakpoint;
                    newIS.Add(location, ivm);
                }
            }
            else
            {
                // If in main memory load a window around the current instruction
                UInt32 min = Math.Max(InstructionPointer - 20, Virtual_Machine.VirtualMachine.RAMStartAddress);
                UInt32 max = Math.Min(InstructionPointer + 40, Virtual_Machine.VirtualMachine.RAMStartAddress + Virtual_Machine.VirtualMachine.RAMSize);

                var ramData = m_vm.RAM.GetData(min, max);

                for (int i = 0; i < ramData.Length - 1; i += instructionSizeInWords)
                {
                    UInt32 location = (UInt32)i + min;
                    var ivm = new InstructionViewModel((int)location, ramData[i], ramData[i + 1]);
                    ivm.BreakpointChanged += ChangeBreakpoint;
                    newIS.Add(location, ivm);
                }
            }

            InstructionStream = newIS;
            OnPropertyChanged("InstructionStream");
        }

        /// <summary>
        /// Sets or unsets a breakpoint at the given location
        /// </summary>
        private void ChangeBreakpoint(int location, bool set)
        {
            if(set)
            {
                m_breakPoints.Add(location);
            }
            else
            {
                m_breakPoints.Remove(location);
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
            OnPropertyChanged("StartStop");

            if(m_running)
            {
                m_runVMThread.RunWorkerAsync();
            }
            else
            {
                RefreshMachineData();
            }
        }

        /// <summary>
        /// Updates the CurrentInstruction VM, sets it to the VM of
        /// the instruction aimed at by the InstructionPointer
        /// </summary>
        void UpdateCurrentInstruction()
        {
            if (!InstructionStream.ContainsKey(InstructionPointer))
            {
                LoadInstructionStream();
            }

            var newCurrentInstruction = InstructionStream[InstructionPointer];

            if (newCurrentInstruction != m_currentInstruction)
            {
                m_currentInstruction.IsCurrentInstruction = false;
                newCurrentInstruction.IsCurrentInstruction = true;
                m_currentInstruction = newCurrentInstruction;
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
                m_vm.Tick();
            }           

            // Need to make sure the instruction window contains the
            // current instruction so we can check for break instructions.
            // This might become a performance issue
            if(!InstructionStream.ContainsKey(InstructionPointer))
            {
                LoadInstructionStream();
            }

            // Check for a user set breakpoint
            if (m_breakPoints.Contains((int)InstructionPointer))
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }

            // Check for a break instruction
            if (InstructionStream[InstructionPointer].Instruction == "Break")
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }
        }

        // When we break in we need to update the UI to display the current machine state
        // but don't want to do this every step as that would be slow, instead we call this
        // whenever we hit a breakpoint or the user steps when broken in
        public void RefreshMachineData()
        {
            UpdateCurrentInstruction();

            for(int i = 0; i < Registers.Count; i++)
            {
                Registers[i].SetValue(m_vm.Processor.Cores[0].Registers[i]);
            }
            OnPropertyChanged("");
        }

        void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
