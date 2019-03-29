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
		CPUCore _CPUCore;

		int[] _currentInstruction;
		int[] _registers;
		bool _hasInstruction;

		public ArithmeticLogicUnit(CPUCore cPUCore, int[] registers)
		{
			_CPUCore = cPUCore;
			_registers = registers;
		}

		public void Tick()
		{
			if (_CPUCore.CurrentStage == PipelineStages.Execution && _hasInstruction == true)
			{
				ALUOperations instructionCode = (ALUOperations)(_currentInstruction[0] & 0x00ff0000);
				int targetRegister = (_currentInstruction[0] & 0x0000ff00) >> 8;
				int sourceRegister = _currentInstruction[0] & 0x000000ff;
				switch (instructionCode)
				{

					case ALUOperations.Add:
						_registers[targetRegister] = _registers[sourceRegister] + _registers[_currentInstruction[1]];
						break;
					case ALUOperations.AddLiteral:
						_registers[targetRegister] = _registers[sourceRegister] + _currentInstruction[1];
						break;
					case ALUOperations.Subtract:
						_registers[targetRegister] = _registers[sourceRegister] - _registers[_currentInstruction[1]];
						break;
					case ALUOperations.SubtractLiteral:
						_registers[targetRegister] = _registers[sourceRegister] - _currentInstruction[1];
						break;
					case ALUOperations.Multiply:
						_registers[targetRegister] = _registers[sourceRegister] * _registers[_currentInstruction[1]];
						break;
					case ALUOperations.MultiplyLiteral:
						_registers[targetRegister] = _registers[sourceRegister] * _currentInstruction[1];
						break;
					case ALUOperations.Divide:
						_registers[targetRegister] = _registers[sourceRegister] / _registers[_currentInstruction[1]];
						break;
					case ALUOperations.DivideLiteral:
						_registers[targetRegister] = _registers[sourceRegister] / _currentInstruction[1];
						break;
					case ALUOperations.SetLiteral:
						_registers[targetRegister] = _currentInstruction[1];
						break;
					case ALUOperations.Copy:
						_registers[targetRegister] = _registers[sourceRegister];
						break;
				}
				_hasInstruction = false;
				_CPUCore.NextStage = PipelineStages.BranchPredict;
                VirtualMachine.Counters.InstructionsExecuted++;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			_hasInstruction = true;
			_currentInstruction = instruction;
		}
	}
}
