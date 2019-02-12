using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	enum InterruptPhase
	{
		RequestId,
		WaitForId,
		RequestInterruptPointer,
		WaitForInterruptPointer
	}

	struct Instruction
	{
		public int address;
		public int part1;
		public int part2;
	}

	class InstructionBlock
	{
		public Queue<Instruction> instructions;
		public Queue<Instruction> loopCache;

		public InstructionBlock()
		{
			instructions = new Queue<Instruction>();
			loopCache = new Queue<Instruction>();
		}
	}

	class InstructionFetchUnit
	{
		CPUCore m_CPUCore;
		InterconnectTerminal m_IOInterconnect;

		bool m_waitingForMemory;
		bool m_startInterrupt;

		InterruptPhase m_interruptPhase;
		int m_interruptId;

		Action EndInterrupt;

		public InstructionBlock m_instructionQueue;

		int m_pendingAddress;

		public InstructionFetchUnit(CPUCore cPUCore, InterconnectTerminal IOInterconnect, Action endInterrupt)
		{
			m_CPUCore = cPUCore;
			m_IOInterconnect = IOInterconnect;
			m_waitingForMemory = false;
			EndInterrupt = endInterrupt;
			m_instructionQueue = new InstructionBlock();
		}

		public void Tick()
		{
			if (!m_startInterrupt)
			{
				bool foundCurrentInstruction = false;

				foreach(Instruction inst in m_instructionQueue.loopCache)
				{
					if(inst.address == m_CPUCore.InstructionPointer)
					{
						foundCurrentInstruction = true;
					}
				}

				while(m_instructionQueue.instructions.Count > 0 && foundCurrentInstruction == false)
				{
					if(m_instructionQueue.instructions.Peek().address != m_CPUCore.InstructionPointer)
					{
						Instruction inst = m_instructionQueue.instructions.Dequeue();
						m_instructionQueue.loopCache.Enqueue(inst);
					}
					else
					{
						foundCurrentInstruction = true;
					}
				}

				while(m_instructionQueue.loopCache.Count > 32)
				{
					m_instructionQueue.loopCache.Dequeue();
				}

				if (m_waitingForMemory == false)
				{
					if(!foundCurrentInstruction)
					{
						int[] newPacket = new int[3];
						newPacket[0] = (int)MessageType.Read;
						newPacket[1] = (int)m_CPUCore.InstructionPointer;
						newPacket[2] = 8;

						bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

						if (requestSent)
						{
							m_pendingAddress = (int)m_CPUCore.InstructionPointer;
							m_waitingForMemory = true;
						}
					}
					else
					{
						if(m_instructionQueue.instructions.Count < 5)
						{
							int[] newPacket = new int[3];
							newPacket[0] = (int)MessageType.Read;
							newPacket[1] = m_instructionQueue.instructions.Last().address + 2;
							newPacket[2] = 8;

							bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

							if (requestSent)
							{
								m_pendingAddress = m_instructionQueue.instructions.Last().address + 2;
								m_waitingForMemory = true;
							}
						}
					}
				}
				else
				{
					if (m_IOInterconnect.HasPacket)
					{
						int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
						m_IOInterconnect.ReadRecievedPacket(receivedPacket);

						if(receivedPacket[0] == (int)MessageType.Response && receivedPacket[1] == m_pendingAddress)
						{
							m_instructionQueue.instructions.Enqueue(new Instruction() { address = m_pendingAddress + 0, part1 = receivedPacket[2], part2 = receivedPacket[3]});
							m_instructionQueue.instructions.Enqueue(new Instruction() { address = m_pendingAddress + 2, part1 = receivedPacket[4], part2 = receivedPacket[5] });
							m_instructionQueue.instructions.Enqueue(new Instruction() { address = m_pendingAddress + 4, part1 = receivedPacket[6], part2 = receivedPacket[7] });
							m_instructionQueue.instructions.Enqueue(new Instruction() { address = m_pendingAddress + 6, part1 = receivedPacket[8], part2 = receivedPacket[9] });
							m_waitingForMemory = false;
							m_IOInterconnect.ClearRecievedPacket();
						}
					}
				}
			}
			else
			{
				switch (m_interruptPhase)
				{
					case InterruptPhase.RequestId:
						{
							int[] newPacket = new int[3];
							newPacket[0] = (int)MessageType.Read;
							newPacket[1] = (int)InterruptController.InterruptRegisterAddress;
							newPacket[2] = 1;

							bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

							if (requestSent)
							{
								m_interruptPhase = InterruptPhase.WaitForId;
							}
							else
							{
                                VirtualMachine.Counters.InterruptWaits++;
							}
						} break;
					case InterruptPhase.WaitForId:
						{
							if (m_IOInterconnect.HasPacket)
							{
								int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
								m_IOInterconnect.ReadRecievedPacket(receivedPacket);

								if(receivedPacket[0] == (int)MessageType.Response && receivedPacket[1] == (int)InterruptController.InterruptRegisterAddress)
								{
									m_IOInterconnect.ClearRecievedPacket();
									m_interruptId = receivedPacket[2];
									m_interruptPhase = InterruptPhase.RequestInterruptPointer;
								}
							}
							else
							{
                                VirtualMachine.Counters.InterruptWaits++;
							}
						} break;
					case InterruptPhase.RequestInterruptPointer:
						{
							int[] newPacket = new int[3];
							newPacket[0] = (int)MessageType.Read;
							newPacket[1] = m_interruptId;
							newPacket[2] = 1;

							bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

							if (requestSent)
							{
								m_interruptPhase = InterruptPhase.WaitForInterruptPointer;
							}
							else
							{
                                VirtualMachine.Counters.InterruptWaits++;
							}
						} break;
					case InterruptPhase.WaitForInterruptPointer:
						{
							if (m_IOInterconnect.HasPacket)
							{
								int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
								m_IOInterconnect.ReadRecievedPacket(receivedPacket);
								
								if(receivedPacket[0] == (int)MessageType.Response && receivedPacket[1] == m_interruptId)
								{
									m_IOInterconnect.ClearRecievedPacket();
									int handlerLocation = receivedPacket[2];

									Instruction newInstruction = new Instruction()
									{
										address = (int)m_CPUCore.InstructionPointer,
										part1 =(int)UnitCodes.Branch | (int)BranchOperations.Jump,
										part2 = handlerLocation
									};

									m_instructionQueue.instructions.Enqueue(newInstruction);

									m_CPUCore.NextStage = PipelineStages.InstructionDispatch;

									m_interruptPhase = InterruptPhase.RequestId;
									m_startInterrupt = false;
								}
							}
							else
							{
                                VirtualMachine.Counters.InterruptWaits++;
							}
						} break;
				}
			}
		}

		internal void DoInterrupt()
		{
			m_interruptPhase = InterruptPhase.RequestId;
			m_instructionQueue.instructions.Clear();
			m_instructionQueue.loopCache.Clear();
			m_startInterrupt = true;
		}
	}
}
