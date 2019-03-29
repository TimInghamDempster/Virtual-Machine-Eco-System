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
        JumpNotEqualRegister = 8 << 16,
        JumpEqualRegister = 9 << 16,
        JumpLessEqualRegister = 10 << 16,
        JumpLessRegister = 11 << 16
    }

	class BranchUnit
	{
		CPUCore _CPUCore;
		int[] _currentOp;
		int[] _registers;
		Action<uint> SetInstructionPointer;
		bool _hasInstruction;

		public BranchUnit(CPUCore cPUCore, int[] registers, Action<uint> setInstructionPointer)
		{
			_CPUCore = cPUCore;
			_registers = registers;
			SetInstructionPointer = setInstructionPointer;
		}

		public void Tick()
		{
			if (_CPUCore.CurrentStage == PipelineStages.BranchPredict)
			{
				if (_hasInstruction)
				{
					switch ((BranchOperations)(_currentOp[0] & 0x00ff0000))
					{
						case BranchOperations.Nop:
							{
								SetInstructionPointer(_CPUCore.InstructionPointer + 2);
							} break;
						case BranchOperations.Jump:
							{
								SetInstructionPointer((uint)_currentOp[1]);
								_hasInstruction = false;
							} break;
						case BranchOperations.JumpRegister:
							{
								SetInstructionPointer((uint)_registers[_currentOp[0] & 0x000000ff] + (uint)_currentOp[1]);
								_hasInstruction = false;
							} break;
						case BranchOperations.JumpNotEqual:
							{
								int register1 = (_currentOp[0] >> 8) & 0x000000ff;
								int register2 = _currentOp[0] & 0x000000ff;

								if(_registers[register1] != _registers[register2])
								{
									SetInstructionPointer((uint)_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(_CPUCore.InstructionPointer + 2);
								}
								_hasInstruction = false;
							}break;
						case BranchOperations.JumpEqual:
							{
								int register1 = (_currentOp[0] >> 8) & 0x000000ff;
								int register2 = _currentOp[0] & 0x000000ff;

								if (_registers[register1] == _registers[register2])
								{
									SetInstructionPointer((uint)_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(_CPUCore.InstructionPointer + 2);
								}
								_hasInstruction = false;
							} break;
						case BranchOperations.JumpLess:
							{
								int register1 = (_currentOp[0] >> 8) & 0x000000ff;
								int register2 = _currentOp[0] & 0x000000ff;

								if (_registers[register1] < _registers[register2])
								{
									SetInstructionPointer((uint)_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(_CPUCore.InstructionPointer + 2);
								}
								_hasInstruction = false;
							} break;
						case BranchOperations.JumpLessEqual:
							{
								int register1 = (_currentOp[0] >> 8) & 0x000000ff;
								int register2 = _currentOp[0] & 0x000000ff;

								if (_registers[register1] <= _registers[register2])
								{
									SetInstructionPointer((uint)_currentOp[1]);
								}
								else
								{
									SetInstructionPointer(_CPUCore.InstructionPointer + 2);
								}
								_hasInstruction = false;
							} break;
                        case BranchOperations.JumpNotEqualRegister:
                        {
                            int register1 = (_currentOp[0] >> 8) & 0x000000ff;
                            int register2 = _currentOp[0] & 0x000000ff;
                            int target = _registers[_currentOp[1]];

                            if (_registers[register1] != _registers[register2])
                            {
                                SetInstructionPointer((uint)target);
                            }
                            else
                            {
                                SetInstructionPointer(_CPUCore.InstructionPointer + 2);
                            }
                            _hasInstruction = false;
                        }
                            break;
                        case BranchOperations.JumpEqualRegister:
                        {
                            int register1 = (_currentOp[0] >> 8) & 0x000000ff;
                            int register2 = _currentOp[0] & 0x000000ff;
                            int target = _registers[_currentOp[1]];

                            if (_registers[register1] == _registers[register2])
                            {
                                SetInstructionPointer((uint)target);
                            }
                            else
                            {
                                SetInstructionPointer(_CPUCore.InstructionPointer + 2);
                            }
                            _hasInstruction = false;
                        }
                            break;
                        case BranchOperations.JumpLessRegister:
                        {
                            int register1 = (_currentOp[0] >> 8) & 0x000000ff;
                            int register2 = _currentOp[0] & 0x000000ff;
                            int target = _registers[_currentOp[1]];

                            if (_registers[register1] < _registers[register2])
                            {
                                SetInstructionPointer((uint)target);
                            }
                            else
                            {
                                SetInstructionPointer(_CPUCore.InstructionPointer + 2);
                            }
                            _hasInstruction = false;
                        }
                            break;
                        case BranchOperations.JumpLessEqualRegister:
                        {
                            int register1 = (_currentOp[0] >> 8) & 0x000000ff;
                            int register2 = _currentOp[0] & 0x000000ff;
                            int target = _registers[_currentOp[1]];

                            if (_registers[register1] <= _registers[register2])
                            {
                                SetInstructionPointer((uint)target);
                            }
                            else
                            {
                                SetInstructionPointer(_CPUCore.InstructionPointer + 2);
                            }
                            _hasInstruction = false;
                        }
                            break;
                        case BranchOperations.Break:
							{
								/*if(!System.Diagnostics.Debugger.IsAttached)
								{
									System.Diagnostics.Debugger.Launch();
								}
								System.Diagnostics.Debugger.Break();*/
								SetInstructionPointer(_CPUCore.InstructionPointer + 2);
								_hasInstruction = false;
                                //Console.ReadLine();
							}break;
					}
                    VirtualMachine.Counters.InstructionsExecuted++;
				}
				else
				{
					SetInstructionPointer(_CPUCore.InstructionPointer + 2);
				}
				_CPUCore.NextStage = PipelineStages.InstructionDispatch;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			_currentOp = instruction;
			_hasInstruction = true;
		}
	}
}
