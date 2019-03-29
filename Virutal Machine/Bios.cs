using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public class Bios
	{
		uint m_startAddress;
		int[] m_biosData;
		InterconnectTerminal m_systemInterconnect;

		bool m_sending;
		int[] m_sendData;
		int m_sendCountdown;

		const int CyclesPerAccess = 2000; // assumes 1ms at 2.0GHz

		public uint Size { get { return (uint)m_biosData.Count(); } }

        public int[] Data { get { return m_biosData; } }

		public Bios(uint startAddress, InterconnectTerminal systemInterconnect)
		{
			m_systemInterconnect = systemInterconnect;
			m_startAddress = startAddress;
			m_sending = false;
			m_biosData = new int[] {
										// Instructions

										// Set up keyboard interrupt handler
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 10,				// Set register 0 to address 6 (keyboard ISR address)
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)VMKeyboard.InterruptNo,			// Write keyboard ISR address from register 0 to PIC
										// Set up block device interrupt handler
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 14,				// Set register 0 to address 6 (keyboard ISR address)
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)BlockDevice.InterruptNo,			// Write block device ISR address from register 0 to PIC
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)m_startAddress + 18,				// Jump to program start

										// Keyboard interrupt handler
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	15 << 8	|	0,	(int)VirtualMachine.keyboardStartAddress,		// Copy last key pressed into register 9
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Block device interrupt handler
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	15 << 8	|	0,	(int)VirtualMachine.SSDInterruptAcknowledgeAddress,// Acknowledge so device stops skwaking
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Get size of program from storage
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										// Set the block we want into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)VirtualMachine.SSDSeekAddress,			// Set storage to block in register 0
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	0 << 8	|	0,	(int)VirtualMachine.SSDFIFOAddress,			// Load number of blocks in program into register 0
										
										// Init program read
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	1,										// Set initial block
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	1,	(int)VirtualMachine.SSDSeekAddress,			// Set storage
										
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	0,										// Init block counter (r1)
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	0,										// Init data counter (r2)
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	0,										// Init RAM counter (r3)
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	4 << 8	|	0,	4096,									// Set block counter to r4
										
										// Load program into RAM
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	5 << 8	|	0,	(int)VirtualMachine.SSDFIFOAddress,			// Load value into r5
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	3 << 8	|	5,	(int)VirtualMachine.RAMStartAddress,			// Store value from r5 into RAM

										//Do loops
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Increment data counter
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	3 << 8	|	3,	1,										// Increment RAM counter
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLess					|	2 << 8	|	4,	(int)VirtualMachine.biosStartAddress + 36,		// Loop through block
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	0,										// Reset data counter
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment block counter
										(int)UnitCodes.Store        |   (int)StoreOperations.StoreToLiteralLocation     |   0 << 8  |   1,  (int)VirtualMachine.SSDSeekAddress,			// Set storage to block in register 0
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLessEqual				|	1 << 8	|	0,	(int)VirtualMachine.biosStartAddress + 36,		// Start next block

										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)VirtualMachine.RAMStartAddress,			// Jump to start of program just loaded
										
			};
		}

		public void Tick()
		{
			if (m_systemInterconnect.HasPacket)
			{
				int[] packet = new int[3];
				m_systemInterconnect.ReadRecievedPacket(packet);

				if(packet[0] == (int)MessageType.Read)
				{

					if(!m_sending)
					{
						m_systemInterconnect.ClearRecievedPacket();
						uint localAddress = (uint)packet[1] - m_startAddress;
						int readLength = packet[2];

						m_sending = true;
						m_sendData = new int[readLength + 2];

						//if (localAddress < m_biosData.Count() - (readLength - 1))
						{
							for (int i = 0; i < readLength; i++)
							{
								if(localAddress + i >= m_biosData.Count())
								{
									break;
								}
								m_sendData[i + 2] = m_biosData[localAddress + i];
							}
						}
						m_sendData[0] = (int)MessageType.Response;
						m_sendData[1] = packet[1];

						m_sendCountdown = CyclesPerAccess;
					}
				}
				else
				{
					m_systemInterconnect.ClearRecievedPacket();
				}
			}

			if (m_sending)
			{
				if (m_sendCountdown > 0)
				{
					m_sendCountdown--;
				}
				else
				{
					bool sent = m_systemInterconnect.SendPacket(m_sendData, m_sendData.Count());
					if (sent)
					{
						m_sending = false;
					}
				}
			}
		}
	}
}
