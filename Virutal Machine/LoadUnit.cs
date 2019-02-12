using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public enum LoadOperations
	{
		Nop,
		LoadFromRegisterLocation = 1 << 16,
		LoadFromLiteralLocation = 2 << 16
	}

	class LoadUnit
	{
		CPUCore m_CPUCore;
		InterconnectTerminal m_IOInterconnect;

		int[] m_currentInstruction;
		bool m_hasInstruction;
		bool m_waitingForMemory;
		int m_waitingAddress;
		int[] m_registers;

		public LoadUnit(CPUCore cPUCore, InterconnectTerminal IOInterconnect, int[] registers)
		{
			m_CPUCore = cPUCore;
			m_IOInterconnect = IOInterconnect;
			m_registers = registers;
		}

		public void Tick()
		{
			if (m_CPUCore.CurrentStage == PipelineStages.Execution && m_hasInstruction == true)
			{
				switch((LoadOperations)(m_currentInstruction[0] & 0x00ff0000))
				{
					case LoadOperations.LoadFromRegisterLocation:
					{
						if (m_waitingForMemory == false)
						{
							int[] newPacket = new int[3];
							newPacket[0] = (int)MessageType.Read;
							newPacket[1] = m_registers[m_currentInstruction[0] & 0x000000ff] + m_currentInstruction[1];
							newPacket[2] = 1;

							bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

							if (requestSent)
							{
								m_waitingForMemory = true;
								m_waitingAddress = newPacket[1];
							}
						}
						else
						{
							WaitForLoad();
						}
					}break;
					case LoadOperations.LoadFromLiteralLocation:
						{
							if (m_waitingForMemory == false)
							{
								int[] newPacket = new int[3];
								newPacket[0] = (int)MessageType.Read;
								newPacket[1] = m_currentInstruction[1];
								newPacket[2] = 1;

								bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

								if (requestSent)
								{
									m_waitingForMemory = true;
									m_waitingAddress = newPacket[1];
								}
							}
							else
							{
								WaitForLoad();
							}
						} break;
				}
			}
		}

		void WaitForLoad()
		{
			if (m_IOInterconnect.HasPacket)
			{
				int[] recivedPacket = new int[m_IOInterconnect.RecievedSize];
				m_IOInterconnect.ReadRecievedPacket(recivedPacket);

				if(recivedPacket[0] == (int)MessageType.Response && recivedPacket[1] == m_waitingAddress)
				{
					m_IOInterconnect.ClearRecievedPacket();
					m_waitingForMemory = false;
					m_hasInstruction = false;

					m_registers[(m_currentInstruction[0] >> 8) & 0x000000ff] = recivedPacket[2];

					m_CPUCore.NextStage = PipelineStages.BranchPredict;
                    VirtualMachine.Counters.InstructionsExecuted++;
				}
			}
			else
			{
                VirtualMachine.Counters.LoadWaits++;
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_hasInstruction = true;
			m_currentInstruction = instruction;
		}
	}
}
