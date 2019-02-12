using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	class InstructionDispatchUnit
	{
		CPUCore m_CPUCore;

		BranchUnit m_branchUnit;
		ArithmeticLogicUnit m_ALU;
		LoadUnit m_loadUnit;
		StoreUnit m_storeUnit;
		StackEngine m_stackUnit;

		InstructionFetchUnit m_fetchUnit;

		Action EndInterrupt;

		public InstructionDispatchUnit(CPUCore cPUCore,
			BranchUnit branchUnit,
			ArithmeticLogicUnit ALU,
			LoadUnit loadUnit,
			StoreUnit storeUnit,
			StackEngine stackUnit,
			InstructionFetchUnit fetchUnit, 
			Action endInterrupt)
		{
			m_CPUCore = cPUCore;
			m_branchUnit = branchUnit;
			m_ALU = ALU;
			m_loadUnit = loadUnit;
			m_storeUnit = storeUnit;
			m_stackUnit = stackUnit;
			m_fetchUnit = fetchUnit;
			EndInterrupt = endInterrupt;
		}

		public void Tick()
		{
			if (m_CPUCore.CurrentStage == PipelineStages.InstructionDispatch)
			{
				int[] currentInstruction = new int[2];
				bool instructionInQueue = false;

				foreach(Instruction inst in m_fetchUnit.m_instructionQueue.instructions)
				{
					if(inst.address == m_CPUCore.InstructionPointer)
					{
						currentInstruction[0] = inst.part1;
						currentInstruction[1] = inst.part2;
						instructionInQueue = true;
						break;
					}
				}

				if(!instructionInQueue)
				{
					foreach (Instruction inst in m_fetchUnit.m_instructionQueue.loopCache)
					{
						if (inst.address == m_CPUCore.InstructionPointer)
						{
							currentInstruction[0] = inst.part1;
							currentInstruction[1] = inst.part2;
							instructionInQueue = true;
							break;
						}
					}
				}

				if(!instructionInQueue)
				{
                    VirtualMachine.Counters.FetchWaits++;
                    VirtualMachine.Counters.ICacheMisses++;
					return;
				}
				else
				{
                    VirtualMachine.Counters.ICacheHits++;
				}

				if ((currentInstruction[0] & 0xff000000) == (int)UnitCodes.Interrupt
							&& (currentInstruction[0] & 0x00ff0000) == (int)InterruptInstructions.InterruptReturn)
				{
					//System.Diagnostics.Debugger.Break();
					m_fetchUnit.m_instructionQueue.instructions.Clear();
					m_fetchUnit.m_instructionQueue.loopCache.Clear();
					EndInterrupt();
				}

				UnitCodes executionUnitCode = (UnitCodes)(currentInstruction[0] & 0xff000000);
				switch (executionUnitCode)
				{
					case UnitCodes.ALU:
						{
							m_ALU.SetInstruction(currentInstruction);
							m_CPUCore.NextStage = PipelineStages.Execution;
						} break;
					case UnitCodes.Load:
						{
							m_loadUnit.SetInstruction(currentInstruction);
							m_CPUCore.NextStage = PipelineStages.Execution;
						} break;
					case UnitCodes.Store:
						{
							m_storeUnit.SetInstruction(currentInstruction);
							m_CPUCore.NextStage = PipelineStages.Execution;
						} break;
					case UnitCodes.Branch:
						{
							m_branchUnit.SetInstruction(currentInstruction);
							m_CPUCore.NextStage = PipelineStages.BranchPredict;
						} break;
					case UnitCodes.Stack:
						{
							m_stackUnit.SetInstruction(currentInstruction);
							m_CPUCore.NextStage = PipelineStages.Execution;
						}break;
					case UnitCodes.Nop:
						{
							// Do we want to fetch here or move the isntruction pointer?
							// Almost certainly in an invalid state anyway so no correct answer.
							m_CPUCore.NextStage = PipelineStages.InstructionDispatch;
						}break;
				}
			}
		}
	}
}
