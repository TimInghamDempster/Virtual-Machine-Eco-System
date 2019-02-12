using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    class MemeoryController
    {        
        int[] m_data;
        uint[] m_addresses;
        int[] m_requestTimers;
        bool[] m_requested;
        bool[] m_ready;
        bool[] m_cpuIsAskingForData;

        public MemeoryController()
        {
            m_data = new int[16];
            m_addresses = new uint[4];
            m_requestTimers = new int[4];
            m_requested = new bool[4];
            m_ready = new bool[4];
            m_cpuIsAskingForData = new bool[4];
        }

        public int Request(uint address, bool cpuIsAskingForData, int value1 = 0, int value2 = 0, int value3 = 0, int value4 = 0)
        {
            for (int i = 0; i < m_requested.Count(); i++)
            {
                if (!m_requested[i])
                {
                    m_addresses[i] = address;
                    m_requested[i] = true;
                    m_ready[i] = false;
                    m_requestTimers[i] = 200; 
                    m_cpuIsAskingForData[i] = cpuIsAskingForData;
                    m_data[i * 4 + 0] = value1;
                    m_data[i * 4 + 1] = value2;
                    m_data[i * 4 + 2] = value3;
                    m_data[i * 4 + 3] = value4;
                    return i;
                }
            }
            return -1;
        }

        public void Read(int channel, out int data1, out int data2, out int data3, out int data4)
        {
            m_requested[channel] = false;
            m_ready[channel] = false;

            data1 = m_data[channel * 4 + 0];
            data2 = m_data[channel * 4 + 1];
            data3 = m_data[channel * 4 + 2];
            data4 = m_data[channel * 4 + 3];
        }

        public void Snoop(int channel, out uint address, out bool ready, out bool isTowardSystem)
        {
            address = m_addresses[channel];
            ready = m_ready[channel];
            isTowardSystem = m_cpuIsAskingForData[channel];
        }

        public void Tick()
        {
        }
    }
}
