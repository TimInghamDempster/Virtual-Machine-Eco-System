using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    public enum PipelineStages
    {
        BranchPredict,
        InstructionDispatch,
        Execution,
        Retirement
    }

    public enum UnitCodes
    {
        Nop,
        ALU			= 1 << 24,
        Load		= 2 << 24,
        Store		= 3 << 24,
		Branch		= 4 << 24,
		Fetch		= 5 << 24,
		Interrupt	= 6 << 24,
		Stack		= 7 << 24
    }

    public class CPUCore
    {
        uint m_instructionPointer;
		uint m_storedInstructionPointer;
        
        int[] m_registers;

		

		uint m_coreId;

		bool m_interruptWaiting;
		bool m_interrupted;

        PipelineStages m_currentStage;
        PipelineStages m_nextStage;

        InterconnectTerminal m_IOInterconnect;
        BranchUnit m_branchUnit;
        InstructionFetchUnit m_fetchUnit;
        InstructionDispatchUnit m_dispatchUnit;
        ArithmeticLogicUnit m_ALU;
        LoadUnit m_loadUnit;
        StoreUnit m_storeUnit;
        RetireUnit m_retireUnit;
		StackEngine m_stackEngine;

		public PipelineStages CurrentStage { get { return m_currentStage; } }
		public PipelineStages NextStage { set { m_nextStage = value; } }
		public uint InstructionPointer { get { return m_instructionPointer; } }
		public uint StackPointer {get;set;}
        public int[] Registers { get { return m_registers; } }

        public CPUCore(InterconnectTerminal IOInterconnect, uint id, InterruptController interruptController)
		{
			m_coreId = id;
            m_instructionPointer = VirtualMachine.biosStartAddress;
			interruptController.AddCore(Interrupt);
            m_registers = new int[16];
            m_currentStage = PipelineStages.InstructionDispatch;
			m_nextStage = PipelineStages.InstructionDispatch;
            m_IOInterconnect = IOInterconnect;
            m_retireUnit = new RetireUnit(this);
            m_ALU = new ArithmeticLogicUnit(this, m_registers);
            m_loadUnit = new LoadUnit(this, m_IOInterconnect, m_registers);
            m_storeUnit = new StoreUnit(this, IOInterconnect, m_registers);
			m_stackEngine = new StackEngine(this, m_storeUnit, m_loadUnit, m_registers);
            m_branchUnit = new BranchUnit(this, m_registers, (uint ip) => m_instructionPointer = ip );
			m_fetchUnit = new InstructionFetchUnit(this, IOInterconnect, EndInterrupt);
			m_dispatchUnit = new InstructionDispatchUnit(this, m_branchUnit, m_ALU, m_loadUnit, m_storeUnit, m_stackEngine, m_fetchUnit, EndInterrupt);
			StackPointer = VirtualMachine.RAMSize - 1;
            
        }

		void Interrupt()
		{
			m_interruptWaiting = true;
		}

		void EndInterrupt()
		{
			m_interrupted = false;
			m_instructionPointer = m_storedInstructionPointer;
			m_nextStage = PipelineStages.InstructionDispatch;
		}

        public void Tick()
        {

            m_branchUnit.Tick();
            m_fetchUnit.Tick();
            m_dispatchUnit.Tick();
            m_ALU.Tick();
            m_loadUnit.Tick();
            m_storeUnit.Tick();
            m_retireUnit.Tick();
			m_stackEngine.Tick();

			// Only safe time to do this is right before dispatch
			if (m_interruptWaiting &&
			m_currentStage != PipelineStages.InstructionDispatch && m_nextStage == PipelineStages.InstructionDispatch
				&& !m_interrupted)
			{
				//System.Diagnostics.Debugger.Break();
				m_storedInstructionPointer = m_instructionPointer;
				m_fetchUnit.DoInterrupt();

				m_interruptWaiting = false;
				m_interrupted = true;
			}

            m_currentStage = m_nextStage;
        }
    }
}
