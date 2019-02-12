using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public enum ALUOperations
	{
		Nop,
		SetLiteral		= 1 << 16,
		Add				= 2 << 16,
		AddLiteral		= 3 << 16,
		Subtract		= 4 << 16,
		SubtractLiteral	= 5 << 16,
		Multiply		= 6 << 16,
		MultiplyLiteral	= 7 << 16,
		Divide			= 8 << 16,
		DivideLiteral	= 9 << 16,
		Copy			= 10 << 16
	}

	class ArithmeticLogicUnit
	{
		CPUCore m_CPUCore;

		int[] m_currentInstruction;
		int[] m_registers;
		bool m_hasInstruction;

		public ArithmeticLogicUnit(CPUCore cPUCore, int[] registers)
		{
			m_CPUCore = cPUCore;
			m_registers = registers;
		}

		public void Tick()
		{
			if (m_CPUCore.CurrentStage == PipelineStages.Execution && m_hasInstruction == true)
			{
				ALUOperations instructionCode = (ALUOperations)(m_currentInstruction[0] & 0x00ff0000);
				int targetRegister = (m_currentInstruction[0] & 0x0000ff00) >> 8;
				int sourceRegister = m_currentInstruction[0] & 0x000000ff;
				switch (instructionCode)
				{

					case ALUOperations.Add:
						m_registers[targetRegister] = m_registers[sourceRegister] + m_registers[m_currentInstruction[1]];
						break;
					case ALUOperations.AddLiteral:
						m_registers[targetRegister] = m_registers[sourceRegister] + m_currentInstruction[1];
						break;
					case ALUOperations.Subtract:
						m_registers[targetRegister] = m_registers[sourceRegister] - m_registers[m_currentInstruction[1]];
						break;
					case ALUOperations.SubtractLiteral:
						m_registers[targetRegister] = m_registers[sourceRegister] - m_currentInstruction[1];
						break;
					case ALUOperations.Multiply:
						m_registers[targetRegister] = m_registers[sourceRegister] * m_registers[m_currentInstruction[1]];
						break;
					case ALUOperations.MultiplyLiteral:
						m_registers[targetRegister] = m_registers[sourceRegister] * m_currentInstruction[1];
						break;
					case ALUOperations.Divide:
						m_registers[targetRegister] = m_registers[sourceRegister] / m_registers[m_currentInstruction[1]];
						break;
					case ALUOperations.DivideLiteral:
						m_registers[targetRegister] = m_registers[sourceRegister] / m_currentInstruction[1];
						break;
					case ALUOperations.SetLiteral:
						m_registers[targetRegister] = m_currentInstruction[1];
						break;
					case ALUOperations.Copy:
						m_registers[targetRegister] = m_registers[sourceRegister];
						break;
				}
				m_hasInstruction = false;
				m_CPUCore.NextStage = PipelineStages.BranchPredict;
                VirtualMachine.Counters.InstructionsExecuted++;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_hasInstruction = true;
			m_currentInstruction = instruction;
		}
	}
}
