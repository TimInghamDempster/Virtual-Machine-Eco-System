using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public enum StackOperations
	{
		Push	= 0 << 16,
		Pop		= 1 << 16
	}

	class StackEngine
	{
		private CPUCore m_CPUCore;
		StoreUnit m_storeUnit;
		LoadUnit m_loadUnit;
		private int[] m_instruction;
		bool m_hasInstruction;

		public StackEngine(CPUCore CPUCore, StoreUnit storeUnit, LoadUnit loadUnit, int[] registers)
		{
			m_CPUCore = CPUCore;
			m_storeUnit = storeUnit;
			m_loadUnit = loadUnit;
		}

		public void SetInstruction(int[] instruction)
		{
			m_instruction = instruction;
			m_hasInstruction = true;
		}

		public void Tick()
		{
			if (m_CPUCore.CurrentStage == PipelineStages.Execution && m_hasInstruction == true)
			{
				StackOperations operation = (StackOperations)(m_instruction[0] & 0x00ff0000);

				switch(operation)
				{
					case StackOperations.Push:
					{
						int[] storeInstruction = new int[2];
						storeInstruction[0] = (int)UnitCodes.Store | (int)StoreOperations.StoreToLiteralLocation | 0 << 8 | m_instruction[1];
						storeInstruction[1] = (int)(VirtualMachine.RAMStartAddress + m_CPUCore.StackPointer);
						m_storeUnit.SetInstruction(storeInstruction);
						m_CPUCore.StackPointer--;
						m_hasInstruction = false;
					}
					break;
					case StackOperations.Pop:
					{
						m_CPUCore.StackPointer++;
						int[] loadInstruction = new int[2];
						loadInstruction[0] = (int)UnitCodes.Load | (int)LoadOperations.LoadFromLiteralLocation | 0 << 8 | m_instruction[1];
						loadInstruction[1] = (int)(VirtualMachine.RAMStartAddress + m_CPUCore.StackPointer);
						m_loadUnit.SetInstruction(loadInstruction);
						m_hasInstruction = false;
					}
					break;
				}
			}
		}
	}
}
