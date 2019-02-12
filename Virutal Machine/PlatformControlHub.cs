using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    class PlatformControlHub
    {
        InterconnectTerminal m_cpuTerminal;
        List<InterconnectTerminal> m_deviceTerminals;
        List<uint> m_deviceAddresses;

        public PlatformControlHub(InterconnectTerminal cpuTerminal, uint baseAddress)
        {
            m_cpuTerminal = cpuTerminal;
            m_deviceTerminals = new List<InterconnectTerminal>();
            m_deviceAddresses = new List<uint>();
        }

        public void AddDevice(InterconnectTerminal deviceTerminal, uint address)
        {
            m_deviceTerminals.Add(deviceTerminal);
			m_deviceAddresses.Add(address);
        }

        public void Tick()
        {
            if (m_cpuTerminal.HasPacket)
            {
                int[] packet = new int[m_cpuTerminal.RecievedSize];
                m_cpuTerminal.ReadRecievedPacket(packet);

                uint destAddress = (uint)packet[1];

                for (int i = 0; i < m_deviceTerminals.Count; i++)
                {
                    if (i + 1 < m_deviceAddresses.Count)
					{
						if( (destAddress >= m_deviceAddresses[i]) && (destAddress < m_deviceAddresses[i + 1]))
						{
							bool sent = m_deviceTerminals[i].SendPacket(packet, packet.Count());
							if(sent)
							{
								m_cpuTerminal.ClearRecievedPacket();
							}
							break;
						}
					}
					else //if(i + 1 == m_deviceAddresses.Count)
					{
						bool sent = m_deviceTerminals[i].SendPacket(packet, packet.Count());
						if (sent)
						{
							m_cpuTerminal.ClearRecievedPacket();
						}
					}
                }
            }

            for (int i = 0; i < m_deviceTerminals.Count; i++)
            {
                if (m_deviceTerminals[i].HasPacket)
                {
                    int[]  packet = new int[m_deviceTerminals[i].RecievedSize];

                    m_deviceTerminals[i].ReadRecievedPacket(packet);

                    bool sent = m_cpuTerminal.SendPacket(packet, packet.Count());

                    if (sent)
                    {
                        m_deviceTerminals[i].ClearRecievedPacket();
                    }

                    break;
                }
            }
        }
    }
}
