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
        Virtual_Machine.VirtualMachine m_vm;
        InstructionViewModel m_currentInstruction;
        volatile bool m_running;
        BackgroundWorker m_runVMThread;
        HashSet<int> m_breakPoints;

        public event PropertyChangedEventHandler PropertyChanged;

        public UInt32 InstructionPointer { get { return m_vm.Processor.Cores[0].InstructionPointer; } }

        public LambdaCommand StepVMCommand { get; private set; }
        public LambdaCommand StartStopCommand { get; private set; }

        public Dictionary<UInt32, InstructionViewModel> InstructionStream { get; private set; }
        public List<RegisterViewModel> Registers { get; private set; }
        public String StartStop { get { return m_running ? "Pause" : "Run"; } }

        public DebuggerVM()
        {
            var comp = new Process();
            comp.StartInfo.FileName = (@"..\..\..\Compiler\Compiler\bin\Debug\Compiler.exe");
            comp.Start();

            m_vm = new Virtual_Machine.VirtualMachine();
            StepVMCommand = new LambdaCommand((o) => { StepVM(); RefreshMachineData(); });
            StartStopCommand = new LambdaCommand((o) => { SwitchRunning(); });

            InstructionStream = new Dictionary<UInt32, InstructionViewModel>();

            LoadInstructionStream();
            m_currentInstruction = InstructionStream[InstructionPointer];

            Registers = new List<RegisterViewModel>();
            for (int i = 0; i < m_vm.Processor.Cores[0].Registers.Count(); i++)
            {
                Registers.Add(new RegisterViewModel(i));
            }

            m_runVMThread = new BackgroundWorker();
            m_runVMThread.DoWork += new DoWorkEventHandler((o, e) => StepVM());
            m_runVMThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, e) => { RefreshMachineData(); if (m_running) m_runVMThread.RunWorkerAsync(); });

            m_breakPoints = new HashSet<int>();
        }

        void LoadInstructionStream()
        {
            var newIS = new Dictionary<uint, InstructionViewModel>();


            if (InstructionPointer >= Virtual_Machine.VirtualMachine.biosStartAddress &&
                InstructionPointer < Virtual_Machine.VirtualMachine.displayStartAddress)
            {
                var biosData = m_vm.BIOS.Data;

                for (int i = 0; i < biosData.Length - 1; i += 2)
                {
                    UInt32 location = (UInt32)i + Virtual_Machine.VirtualMachine.biosStartAddress;
                    var ivm = new InstructionViewModel((int)location, biosData[i], biosData[i + 1]);
                    ivm.BreakpointChanged += ChangeBreakpoint;
                    newIS.Add(location, ivm);
                }
            }
            else
            {
                UInt32 min = Math.Max(InstructionPointer - 20, Virtual_Machine.VirtualMachine.RAMStartAddress);
                UInt32 max = Math.Min(InstructionPointer + 40, Virtual_Machine.VirtualMachine.RAMStartAddress + Virtual_Machine.VirtualMachine.RAMSize);

                var ramData = m_vm.RAM.GetData(min, max);

                for (int i = 0; i < ramData.Length - 1; i += 2)
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

        void SwitchRunning()
        {
            m_running = !m_running;
            OnPropertyChanged("StartStop");

            if(m_running)
            {
                m_runVMThread.RunWorkerAsync();
            }
        }

        public void StepVM()
        {
            UInt32 oldInstructionPointer = InstructionPointer;

            while(oldInstructionPointer == InstructionPointer)
            {
                m_vm.Tick();
            }

            if(!InstructionStream.ContainsKey(InstructionPointer))
            {
                LoadInstructionStream();
            }

            var newCurrentInstruction = InstructionStream[InstructionPointer];

            if(newCurrentInstruction != m_currentInstruction)
            {
                m_currentInstruction.IsCurrentInstruction = false;
                newCurrentInstruction.IsCurrentInstruction = true;
                m_currentInstruction = newCurrentInstruction;
            }

            if (m_breakPoints.Contains((int)InstructionPointer))
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }

            if (m_currentInstruction.Instruction == "Break")
            {
                if (m_running)
                {
                    SwitchRunning();
                }
            }
        }

        public void RefreshMachineData()
        {
            OnPropertyChanged("InstructionPointer");

            for(int i = 0; i < Registers.Count; i++)
            {
                Registers[i].SetValue(m_vm.Processor.Cores[0].Registers[i]);
            }
        }

        void OnPropertyChanged(string PropertyName)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
