using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    public enum StoreOperations
    {
        StoreToRegisterLocation,
        StoreToLiteralLocation = 1 << 16
    }

    class StoreUnit
    {
        CPUCore m_CPUCore;
        InterconnectTerminal m_ioInterconnect;

        int[] m_currentInstruction;
        bool m_hasInstruction;
		int[] m_registers;

        public StoreUnit(CPUCore cPUCore, InterconnectTerminal IOInterconenct, int[] registers)
        {
            m_CPUCore = cPUCore;
            m_ioInterconnect = IOInterconenct;
			m_registers = registers;
        }

        public void Tick()
        {
            if (m_CPUCore.CurrentStage == PipelineStages.Execution && m_hasInstruction == true)
            {
                StoreOperations operation = (StoreOperations)(m_currentInstruction[0] & 0x00ff0000);
                int targetRegister = (m_currentInstruction[0] >> 8) & 0x000000ff;
				int sourceRegister = m_currentInstruction[0] & 0x000000ff;
                switch(operation)
                {
                    case StoreOperations.StoreToRegisterLocation:
                        {
                            uint address = (uint)(m_registers[targetRegister] + m_currentInstruction[1]);
                            int value = m_registers[sourceRegister];

                            int[] packet = new int[3];
							packet[0] = (int)MessageType.Write;
                            packet[1] = (int)address;
                            packet[2] = value;
                            bool stored = m_ioInterconnect.SendPacket(packet, packet.Count());
                            
                            if (stored)
                            {
								m_hasInstruction = false;
                                m_CPUCore.NextStage = PipelineStages.BranchPredict;
                                VirtualMachine.Counters.InstructionsExecuted++;
                            }
							else
							{
                                VirtualMachine.Counters.StoreWaits++;
							}
                        }
                        break;
                    case StoreOperations.StoreToLiteralLocation:
                        {
                            uint address = (uint)m_currentInstruction[1];
                            int value = m_registers[sourceRegister];

                            int[] packet = new int[3];
							packet[0] = (int)MessageType.Write;
							packet[1] = (int)address;
							packet[2] = value;
                            bool stored = m_ioInterconnect.SendPacket(packet, packet.Count());
                            
                            if (stored)
                            {
								m_hasInstruction = false;
                                m_CPUCore.NextStage = PipelineStages.BranchPredict;
                                VirtualMachine.Counters.InstructionsExecuted++;
                            }
							else
							{
                                VirtualMachine.Counters.StoreWaits++;
							}
                        }
                        break;
                }
            }
        }

        public void SetInstruction(int[] instruction)
        {
            m_hasInstruction = true;
            m_currentInstruction = instruction;
        }
    }
}
