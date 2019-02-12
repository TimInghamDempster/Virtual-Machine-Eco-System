using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	enum MessageType
	{
		Read,
		Write,
		Response,
		Command,
		Interrupt
	}

    public class InterconnectTerminal
    {
        int m_cyclesPerInt;
        InterconnectTerminal m_otherTerminal;
        int m_timeSinceLastSent;

        int[] m_recieveBuffer;
        int[] m_sendBuffer;

        int m_sendHead;
        int m_sendSize;
        int m_recievedSize;

        bool m_sendInProgress;
        bool m_hasRecievedPacket;

        public int RecievedSize
        {
            get
            {
                return m_recievedSize;
            }
        }

        public bool HasPacket
        {
            get
            {
                return m_hasRecievedPacket;
            }
        }

        public InterconnectTerminal(int cyclesPerInt, int bufferSize)
        {
            m_cyclesPerInt = cyclesPerInt;
            m_timeSinceLastSent = 0;
            m_recieveBuffer = new int[bufferSize];
            m_sendBuffer = new int[bufferSize];
            m_sendInProgress = false;
            m_hasRecievedPacket = false;
        }

        public void SetOtherEnd(InterconnectTerminal otherTerminal)
        {
            if (otherTerminal.m_recieveBuffer.Count() == m_sendBuffer.Count())
            {
                m_otherTerminal = otherTerminal;
                otherTerminal.m_otherTerminal = this;
            }
        }

        public bool SendPacket(int[] packet, int packetSize)
        {
            if (packet.Count() > m_sendBuffer.Count()
                || m_otherTerminal.m_hasRecievedPacket
                || m_sendInProgress
                || packetSize != packet.Count())
            {
                return false;
            }

            m_sendInProgress = true;
            m_sendHead = 0;
            m_sendSize = packetSize;

            for (int i = 0; i < packet.Count(); i++)
            {
                m_sendBuffer[i] = packet[i];
            }

            return true;
        }

        public bool ReadRecievedPacket(int[] m_buffer)
        {
            if (m_buffer.Count() < m_recievedSize || m_hasRecievedPacket == false)
            {
                return false;
            }

            for (int i = 0; i < m_recievedSize; i++)
            {
                m_buffer[i] = m_recieveBuffer[i];
            }

            return true;
        }

        public void ClearRecievedPacket()
        {
            m_hasRecievedPacket = false;
        }

        public void Tick()
        {
            m_timeSinceLastSent++;

            if (m_timeSinceLastSent >= m_cyclesPerInt)
            {
                if (m_sendInProgress)
                {
                    m_otherTerminal.m_recieveBuffer[m_sendHead] = m_sendBuffer[m_sendHead];
                    m_sendHead++;
                    m_timeSinceLastSent = 0;

                    if (m_sendHead == m_sendSize)
                    {
                        m_sendInProgress = false;
                        m_otherTerminal.m_hasRecievedPacket = true;
                        m_otherTerminal.m_recievedSize = m_sendSize;
                    }
                }
            }
        }
    }
}
