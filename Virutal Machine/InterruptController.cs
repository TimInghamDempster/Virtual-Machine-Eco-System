using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	public enum InterruptInstructions
	{
		SetInterrupt,
		CallInterrupt = 1 << 16,
		InterruptReturn = 2 << 16
	}

	public class InterruptController
	{
		List<Action> m_raiseInterruptList;
		public uint[] m_interruptVector = new uint[257];
		InterconnectTerminal m_systemTerminal;

		const int m_startAddress = 0;
		public const int InterruptRegisterAddress = 256;

		bool m_sending;
		int[] m_sendData;

		public InterruptController(InterconnectTerminal systemTerminal)
		{
			m_raiseInterruptList = new List<Action>();
			m_systemTerminal = systemTerminal;
		}

		public void AddCore(Action raiseInterrupt)
		{
			m_raiseInterruptList.Add(raiseInterrupt);
		}

		internal void Tick()
		{
			if(m_systemTerminal.HasPacket)
			{
				int[] packet = new int[m_systemTerminal.RecievedSize];
				m_systemTerminal.ReadRecievedPacket(packet);
				m_systemTerminal.ClearRecievedPacket();

				if(packet[0] == (int)MessageType.Interrupt)
				{
					m_interruptVector[InterruptRegisterAddress] = (uint)packet[1];
					m_raiseInterruptList[0]();
				}
				else if(packet[0] == (int)MessageType.Write)
				{
					int register = packet[1] - m_startAddress;
					m_interruptVector[register] = (uint)packet[2];
				}
				else if(packet[0] == (int)MessageType.Read && !m_sending)
				{
					int register = packet[1] - m_startAddress;

					m_sending = true;
					m_sendData = new int[3];

					m_sendData[0] = (int)MessageType.Response;
					m_sendData[1] = packet[1];
					m_sendData[2] = (int)m_interruptVector[register];
				}
			}

			if(m_sending)
			{
				bool sent = m_systemTerminal.SendPacket(m_sendData, m_sendData.Count());
				if (sent)
				{
					m_sending = false;
				}
			}
		}
	}
}
