using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public class RAM
	{

		uint m_startAddress;
		int[] m_Data;
		InterconnectTerminal m_cpuInterconnect;

		bool m_sending;
		bool m_storing;
		int[] m_sendData;
		int m_sendCountdown;
		int m_storeCountdown;

		const int CyclesPerAccess = 200; // assumes 0.1ms at 2.0GHz

		public RAM(InterconnectTerminal CPUInterconnect, int size, uint startAddress)
		{
			m_Data = new int[size];
			m_cpuInterconnect = CPUInterconnect;
			m_startAddress = startAddress;
		}

        public int[] GetData(UInt32 lowAddress, UInt32 highAddress)
        {
            UInt32 size = highAddress - lowAddress;
            int[] data = new int[size];

            UInt32 j = 0;
            for(UInt32 i = lowAddress - m_startAddress; i < highAddress - m_startAddress; i++)
            {
                if( i < m_Data.Length)
                {
                    data[j] = m_Data[i];
                    j++;
                }
            }

            return data;
        }

		public void Tick()
		{
			if (m_cpuInterconnect.HasPacket && !m_sending && !m_storing)
			{
				int[] packet = new int[m_cpuInterconnect.RecievedSize];
				m_cpuInterconnect.ReadRecievedPacket(packet);
				m_cpuInterconnect.ClearRecievedPacket();

				if (packet[0] == (int)MessageType.Read)
				{
					uint localAddress = (uint)packet[1] - m_startAddress;
					int readLength = packet[2];

					m_sending = true;
					m_sendData = new int[readLength + 2];

					if (localAddress < m_Data.Count() - (readLength - 1))
					{
						for (int i = 0; i < readLength; i++)
						{
							m_sendData[i + 2] = m_Data[localAddress + i];
						}
					}
					m_sendData[0] = (int)MessageType.Response;
					m_sendData[1] = packet[1];

					m_sendCountdown = CyclesPerAccess;
				}
				else if (packet[0] == (int)MessageType.Write)
				{
					uint localAddress = (uint)packet[1] - m_startAddress;

					if (localAddress < m_Data.Count())
					{
						m_Data[localAddress] = packet[2];
					}

					m_storing = true;
					m_storeCountdown = CyclesPerAccess;
				}
			}

			if(m_storing)
			{
				if(m_storeCountdown > 0)
				{
					m_storeCountdown--;
				}
				else
				{
					m_storing = false;
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
					bool sent = m_cpuInterconnect.SendPacket(m_sendData, m_sendData.Count());
					if (sent)
					{
						m_sending = false;
					}
				}
			}
		}
	}
}
