using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public enum BranchOperations
	{
		Nop,
		Jump = 1 << 16,
		JumpNotEqual = 2 << 16,
		JumpEqual = 3 << 16,
		JumpLessEqual = 4 << 16,
		JumpLess = 5 << 16,
		Break = 6 << 16,
		JumpRegister = 7 << 16,
	}

	class BranchUnit
	{
		CPUCore m_CPUCore;
		int[] m_currentOp;
		int[] m_registers;
		Action<uint> SetInstructionPointer;
		bool m_hasInstruction;

		public BranchUnit(CPUCore cPUCore, int[] registers, Action<uint> setInstructionPointer)
		{
			m_CPUCore = cPUCore;
			m_registers = registers;
			SetInstructionPointer = setInstructionPointer;
		}

		public void Tick()
		{
			if (m_CPUCore.CurrentStage == PipelineStages.BranchPredict)
			{
				if (m_hasInstruction)
				{
					switch ((BranchOperations)(m_currentOp[0] & 0x00ff0000))
					{
						case BranchOperations.Nop:
							{
								SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
							} break;
						case BranchOperations.Jump:
							{
								SetInstructionPointer((uint)m_currentOp[1]);
								m_hasInstruction = false;
							} break;
						case BranchOperations.JumpRegister:
							{
								SetInstructionPointer((uint)m_registers[m_currentOp[0] & 0xff] + (uint)m_currentOp[1]);
								m_hasInstruction = false;
							} break;
						case BranchOperations.JumpNotEqual:
							{
								int register1 = (m_currentOp[0] >> 8) & 0x000000ff;
								int register2 = m_currentOp[0] & 0x000000ff;

								if(m_registers[register1] != m_registers[register2])
								{
									SetInstructionPointer((uint)m_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
								}
								m_hasInstruction = false;
							}break;
						case BranchOperations.JumpEqual:
							{
								int register1 = (m_currentOp[0] >> 8) & 0x000000ff;
								int register2 = m_currentOp[0] & 0x000000ff;

								if (m_registers[register1] == m_registers[register2])
								{
									SetInstructionPointer((uint)m_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
								}
								m_hasInstruction = false;
							} break;
						case BranchOperations.JumpLess:
							{
								int register1 = (m_currentOp[0] >> 8) & 0x000000ff;
								int register2 = m_currentOp[0] & 0x000000ff;

								if (m_registers[register1] < m_registers[register2])
								{
									SetInstructionPointer((uint)m_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
								}
								m_hasInstruction = false;
							} break;
						case BranchOperations.JumpLessEqual:
							{
								int register1 = (m_currentOp[0] >> 8) & 0x000000ff;
								int register2 = m_currentOp[0] & 0x000000ff;

								if (m_registers[register1] <= m_registers[register2])
								{
									SetInstructionPointer((uint)m_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
								}
								m_hasInstruction = false;
							} break;
						case BranchOperations.Break:
							{
								/*if(!System.Diagnostics.Debugger.IsAttached)
								{
									System.Diagnostics.Debugger.Launch();
								}
								System.Diagnostics.Debugger.Break();*/
								SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
								m_hasInstruction = false;
                                //Console.ReadLine();
							}break;
					}
                    VirtualMachine.Counters.InstructionsExecuted++;
				}
				else
				{
					SetInstructionPointer(m_CPUCore.InstructionPointer + 2);
				}
				m_CPUCore.NextStage = PipelineStages.InstructionDispatch;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_currentOp = instruction;
			m_hasInstruction = true;
		}
	}
}
