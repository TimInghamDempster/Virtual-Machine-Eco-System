using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virtual_Machine
{
	class BlockDevice
	{
		InterconnectTerminal m_systemInterconnect;
		
		int m_numberOfBlocks;
		
		int m_address;

		int m_seekTime;
		int m_readTime;

		int m_seekCounter;
		int m_readCounter;

		string m_backingFolder;

		int[] m_currentBlockData;
		int m_fifoPointer;
		int m_currentBlock;

		System.IO.BinaryReader m_reader;
		System.IO.BinaryWriter m_writer;
		System.IO.FileStream m_file;

		const int BlockSize = 4096;

		bool m_sending;
		int[] m_sendData;

		bool m_isSkwaking;
		int m_interruptCounter;
		const int m_interruptRepeatTime = 1000;

		public const uint InterruptNo = 33;

		public BlockDevice(int address, InterconnectTerminal systemInterconnect, string backingFolder, int numberOfBlocks, int seekTime, int readTime)
		{
			m_systemInterconnect = systemInterconnect;
			m_numberOfBlocks = numberOfBlocks;
			m_backingFolder = backingFolder;
			m_address = address;
			m_readTime = readTime;
			m_seekTime = seekTime;
			m_currentBlockData = new int[BlockSize];

			if(!System.IO.Directory.Exists(backingFolder))
			{
				System.IO.Directory.CreateDirectory(backingFolder);
			}

			string block0Path = backingFolder + "/0.block";
			ReadBlock(block0Path);			
		}

		void ReadBlock(string path)
		{
			//System.Diagnostics.Debugger.Break();
			if (System.IO.File.Exists(path))
			{
				m_file = new System.IO.FileStream(path, System.IO.FileMode.Open);
				m_reader = new System.IO.BinaryReader(m_file);
			
				for (int i = 0; i < BlockSize; i++)
				{
					m_currentBlockData[i] = m_reader.ReadInt32();
				}
			
				m_file.Close();
			}
		}

		void WriteBlock(string path)
		{
			m_file = new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
			m_writer = new System.IO.BinaryWriter(m_file);

			for(int i = 0; i < BlockSize; i++)
			{
				m_writer.Write(m_currentBlockData[i]);
			}

			m_file.Close();
		}

		string GetCurrentBlockPath()
		{
			return m_backingFolder + "/" + m_currentBlock.ToString() + ".block";
		}

		public void Tick()
		{
			if(m_seekCounter > 0)
			{
				m_seekCounter--;
				
				if(m_seekCounter == 0)
				{
					m_isSkwaking = true;
					m_interruptCounter = m_interruptRepeatTime;
				}
			}

			if(m_readCounter > 0)
			{
				m_readCounter--;
			}

			if(m_seekCounter != 0 || m_readCounter != 0)
			{
				return;
			}

			if(m_isSkwaking)
			{
				if(m_interruptCounter == m_interruptRepeatTime)
				{
					int[] buffer = new int[2];
					buffer[0] = (int)MessageType.Interrupt;
					buffer[1] = (int)InterruptNo;

					bool sent = m_systemInterconnect.SendPacket(buffer, buffer.Count());

					if (sent)
					{
						m_interruptCounter--;
					}
				}
				else
				{
					m_interruptCounter--;
					if(m_interruptCounter == 0)
					{
						m_interruptCounter = m_interruptRepeatTime;
					}
				}
			}

			if (m_systemInterconnect.HasPacket)
			{
				int[] packet = new int[m_systemInterconnect.RecievedSize];
				m_systemInterconnect.ReadRecievedPacket(packet);
				m_systemInterconnect.ClearRecievedPacket();

				if(packet[1] == VirtualMachine.SSDSeekAddress)
				{
					//System.Diagnostics.Debugger.Break();
					int nextBlock = packet[2];
					
					if(nextBlock < m_numberOfBlocks)
					{
						WriteBlock(GetCurrentBlockPath());
					
						m_currentBlock = packet[2];
					
						ReadBlock(GetCurrentBlockPath());

						m_seekCounter = m_seekTime;

						m_fifoPointer = 0;
					}
				}
				else if(packet[1] == VirtualMachine.SSDFIFOAddress)
				{
					//System.Diagnostics.Debugger.Break();
					if(packet[0] == (int)MessageType.Read)
					{
						if(m_fifoPointer < BlockSize)
						{
							m_readCounter = m_readTime;
						
							m_sending = true;
							m_sendData = new int[3];

							m_sendData[0] = (int)MessageType.Response;
							m_sendData[1] = packet[1];
							m_sendData[2] = m_currentBlockData[m_fifoPointer];
							m_fifoPointer++;
						}
					}
					else if(packet[0] == (int)MessageType.Write)
					{
						if (m_fifoPointer < BlockSize)
						{
							m_readCounter = m_readTime;
							int data = packet[2];
						
							m_currentBlockData[m_fifoPointer] = data;
							m_fifoPointer++;
						}
					}
				}
				else if(packet[1] == VirtualMachine.SSDInterruptAcknowledgeAddress)
				{
					//System.Diagnostics.Debugger.Break();
					m_isSkwaking = false;
				}
			}
			if (m_readCounter == 0 && m_sending)
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
