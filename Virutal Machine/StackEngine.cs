namespace Virtual_Machine
{
    public enum StackOperations
	{
		PushAndStore	= 0 << 16,
		PopAndLoad		= 1 << 16,
        Pop             = 2 << 16,
        Push            = 3 << 16
    }

	class StackEngine
	{
		private CPUCore _CPUCore;
		StoreUnit _storeUnit;
		LoadUnit _loadUnit;
		private int[] _instruction;
		bool _hasInstruction;

		public StackEngine(CPUCore CPUCore, StoreUnit storeUnit, LoadUnit loadUnit, int[] registers)
		{
			_CPUCore = CPUCore;
			_storeUnit = storeUnit;
			_loadUnit = loadUnit;
		}

		public void SetInstruction(int[] instruction)
		{
			_instruction = instruction;
			_hasInstruction = true;
		}

		public void Tick()
		{
			if (_CPUCore.CurrentStage == PipelineStages.Execution && _hasInstruction == true)
			{
				StackOperations operation = (StackOperations)(_instruction[0] & 0x00ff0000);

				switch(operation)
				{
                    // Editing the stack pointer is much faster than storing and loading
                    // so it makes sense to have separate operations that just do the fast
                    // part as the slow part isn't always necessary
                    case StackOperations.Pop:
                        _CPUCore.StackPointer++;
                        _hasInstruction = false;
                        _CPUCore.NextStage = PipelineStages.BranchPredict;
                        break;
                    case StackOperations.Push:
                        _CPUCore.StackPointer--;
                        _hasInstruction = false;
                        _CPUCore.NextStage = PipelineStages.BranchPredict;
                        break;
                    // Second int of instruction contains register to store from
					case StackOperations.PushAndStore:
						int[] storeInstruction = new int[2];
						storeInstruction[0] = (int)UnitCodes.Store | (int)StoreOperations.StoreToLiteralLocation | 0 << 8 | _instruction[1];
						storeInstruction[1] = (int)(VirtualMachine.RAMStartAddress + _CPUCore.StackPointer);
						_storeUnit.SetInstruction(storeInstruction);
						_CPUCore.StackPointer--;
						_hasInstruction = false;
					    break;
                    // Second int of instruction contains register to load to
                    case StackOperations.PopAndLoad:
						_CPUCore.StackPointer++;
						int[] loadInstruction = new int[2];
						loadInstruction[0] = (int)UnitCodes.Load | (int)LoadOperations.LoadFromLiteralLocation | 0 << 8 | _instruction[1];
						loadInstruction[1] = (int)(VirtualMachine.RAMStartAddress + _CPUCore.StackPointer);
						_loadUnit.SetInstruction(loadInstruction);
						_hasInstruction = false;
					    break;
				}
			}
		}
	}
}
