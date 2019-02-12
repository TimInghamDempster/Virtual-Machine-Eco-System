using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	class VMKeyboard
	{
		InterconnectTerminal m_systemInterconnect;

		char m_currentKey;
		bool m_hasCurrentKey;

		bool m_isSendingKey;
		bool m_isInterrupting;

		public const uint InterruptNo = 32;

		public VMKeyboard(InterconnectTerminal systemInterconnect)
		{
			m_systemInterconnect = systemInterconnect;
		}

		public void Tick()
		{
			if(Console.KeyAvailable)
			{
				m_currentKey = Console.ReadKey().KeyChar;
				if(m_hasCurrentKey == false)
				{
					m_hasCurrentKey = true;
					m_isInterrupting = true;
				}
				Console.Write("\b \b");
			}
			else
			{
				m_hasCurrentKey = false;
			}

			if(m_systemInterconnect.HasPacket)
			{
				int[] buffer = new int[m_systemInterconnect.RecievedSize];
				m_systemInterconnect.ReadRecievedPacket(buffer);
				m_systemInterconnect.ClearRecievedPacket();

				m_isSendingKey = true;
			}

			if(m_isInterrupting)
			{
				int[] buffer = new int[2];
				buffer[0] = (int)MessageType.Interrupt;
				buffer[1] = (int)InterruptNo;
				
				bool sent = m_systemInterconnect.SendPacket(buffer, buffer.Count());

				if (sent)
				{
					m_isInterrupting = false;
				}
			}

			if(m_isSendingKey)
			{
				int[] buffer = new int[3];
				buffer[0] = (int)MessageType.Response;
				buffer[1] = (int)VirtualMachine.keyboardStartAddress;
				buffer[2] = m_currentKey;

				bool sent = m_systemInterconnect.SendPacket(buffer, buffer.Count());

				if(sent)
				{
					m_isSendingKey = false;
				}
			}
		}
	}
}
