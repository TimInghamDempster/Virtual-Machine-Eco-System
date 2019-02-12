using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	class Uncore
	{
		private InterconnectTerminal m_IOInterconnect;
		List<InterconnectTerminal> m_coreInterconencts;
		InterconnectTerminal m_LPICInterconnenct;
		InterconnectTerminal m_RAMInterconnect;

		public Uncore(InterconnectTerminal IOInterconnect, InterconnectTerminal LPICInterconnenct, InterconnectTerminal RAMInterconnect)
		{
			m_IOInterconnect = IOInterconnect;
			m_LPICInterconnenct = LPICInterconnenct;
			m_RAMInterconnect = RAMInterconnect;
			m_coreInterconencts = new List<InterconnectTerminal>();
		}

		public void AddCoreInterconnect(InterconnectTerminal coreInterconnect)
		{
			m_coreInterconencts.Add(coreInterconnect);
		}

		public void Tick()
		{
			foreach(InterconnectTerminal ic in m_coreInterconencts)
			{
				if(ic.HasPacket)
				{
					int[] packet = new int[ic.RecievedSize];
					ic.ReadRecievedPacket(packet);
					bool forwarded = false;

					// Interrupt request can come from the core itself, not just the io side
					if (packet[0] == (int)MessageType.Interrupt)
					{
						forwarded = m_LPICInterconnenct.SendPacket(packet, packet.Length);
					}
					else if((packet[0] == (int) MessageType.Read ||
						packet[0] == (int) MessageType.Write))
						{
							if(	packet[1] < VirtualMachine.PCHStartAddress)
							{
								forwarded = m_LPICInterconnenct.SendPacket(packet, packet.Length);
							}
							else if(packet[1] >= VirtualMachine.RAMStartAddress && packet[1] < VirtualMachine.RAMStartAddress + VirtualMachine.RAMSize)
							{
								forwarded = m_RAMInterconnect.SendPacket(packet, packet.Length);
							}
						else
						{
							forwarded = m_IOInterconnect.SendPacket(packet, packet.Length);
						}
					}

					if(forwarded)
					{
						ic.ClearRecievedPacket();
					}
				}
			}

			if(m_RAMInterconnect.HasPacket)
			{
				int[] packet = new int[m_RAMInterconnect.RecievedSize];
				m_RAMInterconnect.ReadRecievedPacket(packet);

				// Broadcast to all cores before attempting anything multicore.
				bool forwarded = m_coreInterconencts[0].SendPacket(packet, packet.Length);
				if (forwarded)
				{
					m_RAMInterconnect.ClearRecievedPacket();
				}
			}

			if(m_IOInterconnect.HasPacket)
			{
				int[] packet = new int[m_IOInterconnect.RecievedSize];
				m_IOInterconnect.ReadRecievedPacket(packet);
				
				// assumes single core, fix when moving to multi-core system
				bool forwarded = false;

				if (packet[0] == (int)MessageType.Interrupt)
				{
					forwarded = m_LPICInterconnenct.SendPacket(packet, packet.Length);
				}
				else
				{
					forwarded = m_coreInterconencts[0].SendPacket(packet, packet.Length);
				}

				if(forwarded)
				{
					m_IOInterconnect.ClearRecievedPacket();
				}
			}

			if(m_LPICInterconnenct.HasPacket)
			{
				int[] packet = new int[m_LPICInterconnenct.RecievedSize];
				m_LPICInterconnenct.ReadRecievedPacket(packet);

				bool forwarded = m_coreInterconencts[0].SendPacket(packet, packet.Length);
				if(forwarded)
				{
					m_LPICInterconnenct.ClearRecievedPacket();
				}
			}
		}
	}
}
