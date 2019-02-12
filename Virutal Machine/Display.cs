using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
    enum DisplayCommands
    {
        Refresh,
        Clear
    }

    class Display
    {
        InterconnectTerminal m_systemInterconnect;

        uint m_startAddress;
        int m_lineLength = Console.WindowWidth - 1;
		int m_numlines = Console.WindowHeight - 1;
		uint m_commandAddress;
        char[] m_charData;
		ConsoleColor[] m_foregroundColours;
		ConsoleColor[] m_backgroundColours;
		ConsoleColor m_cursorForeground;
		ConsoleColor m_cursorBackground;
		private uint m_foregroundColourAddress;
		private uint m_backgroundColourAddress;

        public Display(uint startAddress, InterconnectTerminal systemInterconnect)
        {
            m_startAddress = startAddress;
			m_commandAddress = VirtualMachine.displayCommandAddress;
			m_foregroundColourAddress = VirtualMachine.displayFgColourAddress;
			m_backgroundColourAddress = VirtualMachine.displayBkgColourAddress;

            m_charData = new char[m_lineLength * m_numlines];
			m_backgroundColours = new ConsoleColor[m_lineLength * m_numlines];
			m_foregroundColours = new ConsoleColor[m_lineLength * m_numlines];

			m_cursorForeground = ConsoleColor.White;
			m_cursorBackground = ConsoleColor.Black;

            m_systemInterconnect = systemInterconnect;
        }

        public void Tick()
        {
            if(m_systemInterconnect.HasPacket)
            {
                int[] packet = new int[m_systemInterconnect.RecievedSize];
                m_systemInterconnect.ReadRecievedPacket(packet);

                m_systemInterconnect.ClearRecievedPacket();

				if(packet[0] == (int)MessageType.Write)
				{
					if (packet[1] < (int)m_commandAddress)
					{
						int index = packet[1] - (int)m_startAddress; 
						m_charData[index] = (char)packet[2];
						m_foregroundColours[index] = m_cursorForeground;
						m_backgroundColours[index] = m_cursorBackground;
					}
					else if (packet[1] == (int)m_commandAddress)
					{
						switch((DisplayCommands)packet[2])
						{
							case DisplayCommands.Clear:
							{
								Array.Clear(m_charData, 0, m_charData.Length);
								Refresh();
							}break;
							case DisplayCommands.Refresh:
							{
								Refresh();	
							}break;
						}
					}
					else if(packet[1] == (int)m_foregroundColourAddress)
					{
						m_cursorForeground = (ConsoleColor)packet[2];
					}
					else if (packet[1] == (int)m_backgroundColourAddress)
					{
						m_cursorBackground = (ConsoleColor)packet[2];
					}
				}
            }
        }

		public void Refresh()
		{
			Console.CursorTop = 0;
			Console.CursorLeft = 0;
			for(int y = 0; y < m_numlines; y++)
			{
				for(int x = 0; x < m_lineLength; x++)
				{
					int index = x + y * m_lineLength;
					Console.ForegroundColor = m_foregroundColours[index];
					Console.BackgroundColor = m_backgroundColours[index];
					Console.Write(m_charData[index]);
				}
				Console.Write('\n');
			}
		}
    }
}
